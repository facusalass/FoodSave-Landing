using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Foodsave.Web.Services
{
    public class AuthService
    {
        private readonly string _demoEmail;
        private readonly string _demoPassword;

        public AuthService(IConfiguration configuration)
        {
            _demoEmail = configuration["DemoAuth:Email"]
                ?? throw new InvalidOperationException(
                    "DemoAuth:Email no está configurado.");
            _demoPassword = configuration["DemoAuth:Password"]
                ?? throw new InvalidOperationException(
                    "DemoAuth:Password no está configurado.");
        }

        public bool ValidarCredenciales(string email, string password)
        {
            return email == _demoEmail && password == _demoPassword;
        }

        public ClaimsPrincipal CrearPrincipal()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "Comercio FoodSave"),
                new(ClaimTypes.Email, _demoEmail),
                new(ClaimTypes.Role, "Administrador")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            return new ClaimsPrincipal(identity);
        }

        public async Task IniciarSesionAsync(HttpContext httpContext)
        {
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                CrearPrincipal(),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });
        }

        public async Task CerrarSesionAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
