using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Foodsave.Web.Controllers.Api
{
    // Controlador REST para autenticar administradores de la plataforma.
    // Ahora con JWT: el login devuelve un access token en vez de cookie.
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
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

        [AllowAnonymous]
        [HttpPost("login")]
        [EnableRateLimiting("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            // Recibe JSON con email y password. AutenticarAsync valida contra BD y genera el JWT.
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
