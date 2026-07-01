using Foodsave.Web.Data;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrWhiteSpace(port))
            {
                if (!int.TryParse(port, out var portNumber) || portNumber < 1 || portNumber > 65535)
                    throw new InvalidOperationException($"PORT inválido: '{port}'. Debe ser un número entre 1 y 65535.");
                builder.WebHost.UseUrls($"http://0.0.0.0:{portNumber}");
            }

            builder.Services
                .AddFoodSaveMvc()
                .AddFoodSaveSwagger()
                .AddFoodSaveHealthChecks()
                .AddFoodSaveServices()
                .AddFoodSaveAuth()
                .AddFoodSaveRateLimiting();

            if (TryConfigurePostgreSql(builder))
            {
                builder.Logging.AddConsole();
            }
            else if (builder.Environment.IsDevelopment())
            {
                ConfigureSqlite(builder);
                builder.Logging.AddConsole();
            }
            else
            {
                throw new InvalidOperationException(
                    "No se configuró PostgreSQL. Definí " +
                    "ConnectionStrings__DefaultConnection o DATABASE_URL.");
            }

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();
                DbInitializer.Initialize(context);
            }

            app.UseForwardedHeaders();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseWhen(
                    ctx => !ctx.Request.Path.StartsWithSegments("/api"),
                    pipeline => pipeline.UseStatusCodePagesWithReExecute("/404"));
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapHealthChecks("/health");

            app.Run();
        }

        private static bool TryConfigurePostgreSql(WebApplicationBuilder builder)
        {
            try
            {
                var connectionString =
                    DatabaseConnectionStringResolver.Resolve(builder.Configuration);

                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(
                        connectionString,
                        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static void ConfigureSqlite(WebApplicationBuilder builder)
        {
            var dbPath = Path.Combine(
                builder.Environment.ContentRootPath,
                "foodsave_dev.db");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));
        }
    }
}
