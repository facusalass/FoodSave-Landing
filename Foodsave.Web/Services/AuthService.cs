using System.Security.Claims;
using Foodsave.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Foodsave.Web.Services
{
    // Servicio que orquesta la autenticación.
    // Para API usa JWT (Stateless), para MVC usa Cookie (Stateful con tokens guardados).
    public class AuthService
    {
        private readonly SupabaseAuthClient _supabaseAuth;

        public AuthService(SupabaseAuthClient supabaseAuth)
        {
            _supabaseAuth = supabaseAuth;
        }

        // API: valida credenciales contra Supabase y devuelve el JWT.
        public Task<SupabaseSession?> AutenticarAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            return _supabaseAuth.SignInAsync(
                email,
                password,
                cancellationToken);
        }

        // Construye el ClaimsPrincipal con los datos del usuario desde la sesión de Supabase.
        public ClaimsPrincipal CrearPrincipal(SupabaseSession session)
        {
            var user = session.User;
            var name = user.GetMetadataString(user.UserMetadata, "name");
            var role = user.GetMetadataString(user.AppMetadata, "role");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, string.IsNullOrWhiteSpace(name)
                    ? user.Email
                    : name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, string.IsNullOrWhiteSpace(role)
                    ? "Usuario"
                    : role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            return new ClaimsPrincipal(identity);
        }

        // MVC: guarda el JWT dentro de la cookie para usarlo al hacer logout.
        public async Task IniciarSesionAsync(
            HttpContext httpContext,
            SupabaseSession session)
        {
            var properties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(session.ExpiresIn)
            };
            properties.StoreTokens(
            [
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = session.AccessToken
                },
                new AuthenticationToken
                {
                    Name = "refresh_token",
                    Value = session.RefreshToken
                }
            ]);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                CrearPrincipal(session),
                properties);
        }

        // MVC logout: saca el token de la cookie, lo revoca en Supabase y borra la cookie.
        public async Task CerrarSesionAsync(HttpContext httpContext)
        {
            var accessToken = await httpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                await _supabaseAuth.SignOutAsync(
                    accessToken,
                    httpContext.RequestAborted);
            }

            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // API logout: recibe el token del header y lo revoca en Supabase.
        public Task CerrarSesionTokenAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            return _supabaseAuth.SignOutAsync(accessToken, cancellationToken);
        }
    }
}
