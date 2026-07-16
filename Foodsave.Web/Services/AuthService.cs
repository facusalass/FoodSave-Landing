using System.Security.Claims;
using Foodsave.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Foodsave.Web.Services
{
    public class AuthService
    {
        private readonly SupabaseAuthClient _supabaseAuth;

        public AuthService(SupabaseAuthClient supabaseAuth)
        {
            _supabaseAuth = supabaseAuth;
        }

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

        public Task CerrarSesionTokenAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            return _supabaseAuth.SignOutAsync(accessToken, cancellationToken);
        }
    }
}
