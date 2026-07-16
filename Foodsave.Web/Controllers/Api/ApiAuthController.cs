using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Foodsave.Web.Controllers.Api
{
    // Endpoints de autenticación de la API REST.
    // El login recibe credenciales, consulta Supabase y devuelve un JWT.
    // Todas las demás rutas /api/* validan ese JWT automáticamente.
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    // API usa JSON, no formularios HTML → no necesita antiforgery token.
    [IgnoreAntiforgeryToken]
    public class ApiAuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<ApiAuthController> _logger;

        public ApiAuthController(
            AuthService authService,
            ILogger<ApiAuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // POST /api/auth/login — público, sin auth.
        [AllowAnonymous]
        [HttpPost("login")]
        [EnableRateLimiting("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            // Recibe JSON con email y password, pide JWT a Supabase via AuthService.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = model.Email.Trim().ToLowerInvariant();
            var session = await _authService.AutenticarAsync(
                email,
                model.Password,
                HttpContext.RequestAborted);

            if (session is null)
            {
                _logger.LogWarning(
                    "API: Intento de login fallido para {Email}",
                    email);
                return Unauthorized(new { error = "Credenciales inválidas." });
            }

            _logger.LogInformation("API: Login exitoso para {Email}", email);

            // Devuelve el JWT y refresh token. El cliente debe mandar el access_token
            // como "Authorization: Bearer <token>" en los próximos requests.
            return Ok(new
            {
                message = "Inicio de sesión exitoso.",
                usuario = email,
                accessToken = session.AccessToken,
                refreshToken = session.RefreshToken,
                tokenType = session.TokenType,
                expiresIn = session.ExpiresIn
            });
        }

        // POST /api/auth/logout — requiere JWT en header. Revoca el token en Supabase.
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var authorization = Request.Headers.Authorization.ToString();
            var accessToken = authorization["Bearer ".Length..].Trim();
            await _authService.CerrarSesionTokenAsync(
                accessToken,
                HttpContext.RequestAborted);

            return Ok(new { message = "Sesión cerrada." });
        }
    }
}
