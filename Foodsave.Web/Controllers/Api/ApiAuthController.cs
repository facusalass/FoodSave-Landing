using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Foodsave.Web.Controllers.Api
{
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = model.Email.Trim().ToLowerInvariant();

            if (!_authService.ValidarCredenciales(email, model.Password))
            {
                _logger.LogWarning("API: Intento de login fallido para {Email}", email);
                return Unauthorized(new { error = "Credenciales inválidas." });
            }

            _logger.LogInformation("API: Login exitoso para {Email}", email);
            await _authService.IniciarSesionAsync(HttpContext);

            return Ok(new
            {
                message = "Inicio de sesión exitoso.",
                usuario = email,
                rol = "Administrador"
            });
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            await _authService.CerrarSesionAsync(HttpContext);
            return Ok(new { message = "Sesión cerrada." });
        }
    }
}
