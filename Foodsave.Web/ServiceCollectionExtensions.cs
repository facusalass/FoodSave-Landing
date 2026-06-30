using System.Threading.RateLimiting;
using Foodsave.Web.Infrastructure;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Foodsave.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFoodSaveMvc(
            this IServiceCollection services)
        {
            services.AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Insert(
                    0,
                    new InvariantDecimalModelBinderProvider());
                options.Filters.Add(
                    new AutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add<ApiExceptionFilter>();
            });

            return services;
        }

        public static IServiceCollection AddFoodSaveSwagger(
            this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new()
                {
                    Title = "FoodSave API",
                    Version = "v1",
                    Description = "API REST para gestión de comercios, suscripciones, pagos y solicitudes."
                });
            });

            return services;
        }

        public static IServiceCollection AddFoodSaveServices(
            this IServiceCollection services)
        {
            services.AddScoped<AuthService>();
            services.AddScoped<GestionSuscripcionesService>();
            services.AddScoped<EstadisticasService>();
            services.AddScoped<RegistroPagoService>();

            return services;
        }

        public static IServiceCollection AddFoodSaveAuth(
            this IServiceCollection services)
        {
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.AccessDeniedPath = "/Auth/Login";
                    options.Cookie.Name = "FoodSave.Auth";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                });

            return services;
        }

        public static IServiceCollection AddFoodSaveRateLimiting(
            this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter("SolicitudForm", config =>
                {
                    config.PermitLimit = 5;
                    config.Window = TimeSpan.FromMinutes(10);
                    config.QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst;
                    config.QueueLimit = 0;
                });
            });

            return services;
        }

        public static IServiceCollection AddFoodSaveHealthChecks(
            this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<Data.ApplicationDbContext>(
                    tags: ["database"]);

            return services;
        }
    }
}
