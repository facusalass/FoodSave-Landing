using System.Threading.RateLimiting;
using Foodsave.Web.Infrastructure;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

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
            services.AddHttpClient<SupabaseAuthClient>();

            services.AddHttpClient<FoodSaveApiClient>((sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(config["FoodSaveApi:BaseUrl"]
                    ?? throw new InvalidOperationException("FoodSaveApi:BaseUrl no está configurado."));
                client.DefaultRequestHeaders.Add("X-API-Key", config["FoodSaveApi:ApiKey"]
                    ?? throw new InvalidOperationException("FoodSaveApi:ApiKey no está configurado."));
            });

            return services;
        }

        // Configura dos esquemas de autenticación en paralelo:
        // - Cookie para las rutas MVC (Login web).
        // - JWT Bearer para las rutas /api/*.
        // Un PolicyScheme elige automáticamente cuál usar según la ruta.
        public static IServiceCollection AddFoodSaveAuth(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            const string smartScheme = "FoodSave";
            var supabaseUrl = configuration["Supabase:Url"]
                ?? throw new InvalidOperationException(
                    "Supabase:Url no está configurado.");

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = smartScheme;
                    options.DefaultAuthenticateScheme = smartScheme;
                    options.DefaultChallengeScheme = smartScheme;
                })
                // PolicyScheme decide: si arranca con /api → JWT, sino → Cookie.
                .AddPolicyScheme(smartScheme, smartScheme, options =>
                {
                    options.ForwardDefaultSelector = context =>
                        context.Request.Path.StartsWithSegments("/api")
                            ? JwtBearerDefaults.AuthenticationScheme
                            : CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.AccessDeniedPath = "/Auth/Login";
                    options.Cookie.Name = "FoodSave.Auth";
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = false;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                })
                // JWT configurado para validar contra Supabase Auth.
                .AddJwtBearer(options =>
                {
                    options.Authority = supabaseUrl.TrimEnd('/') + "/auth/v1";  // Supabase es el issuer.
                    options.Audience = "authenticated";  // Audience fija de Supabase.
                    options.RequireHttpsMetadata = true;
                    options.MapInboundClaims = false;  // No mapea claims a tipos .NET viejos.
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,       // Verifica que el token lo emitió Supabase.
                        ValidateAudience = true,     // Verifica "authenticated".
                        ValidateIssuerSigningKey = true,  // Verifica la firma criptográfica.
                        ValidateLifetime = true,      // Rechaza tokens expirados.
                        NameClaimType = "email",      // User.Identity.Name = email.
                        RoleClaimType = "role",
                        ClockSkew = TimeSpan.FromMinutes(1)  // Tolerancia de 1 minuto.
                    };
                });

            return services;
        }

        public static IServiceCollection AddFoodSaveRateLimiting(
            this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, ct) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
                    {
                        context.HttpContext.Response.ContentType = "application/json";
                        await context.HttpContext.Response.WriteAsync(
                            "{\"error\":\"Demasiados intentos. Esperá 15 minutos.\"}", ct);
                    }
                    else
                    {
                        context.HttpContext.Response.ContentType = "text/html; charset=utf-8";
                        await context.HttpContext.Response.WriteAsync(
                            "<!DOCTYPE html><html lang=\"es\"><head><meta charset=\"utf-8\"/></head>" +
                            "<body style=\"font-family:sans-serif;display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0;background:#fcf9f6;\">" +
                            "<div style=\"text-align:center;padding:40px;\"><h2 style=\"color:#e85c2c;\">Demasiados intentos</h2>" +
                            "<p>Esperá 15 minutos antes de intentar de nuevo.</p>" +
                            "<a href=\"/\" style=\"color:#FF6B35;\">Volver al inicio</a></div></body></html>", ct);
                    }
                };

                options.AddFixedWindowLimiter("SolicitudForm", config =>
                {
                    config.PermitLimit = 5;
                    config.Window = TimeSpan.FromMinutes(10);
                    config.QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst;
                    config.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("Login", config =>
                {
                    config.PermitLimit = 5;
                    config.Window = TimeSpan.FromMinutes(15);
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
