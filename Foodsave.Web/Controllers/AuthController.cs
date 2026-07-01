using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Foodsave.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
            ViewData["ActivePage"] = "Auth";
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Comercios");

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim().ToLowerInvariant();

            if (!_authService.ValidarCredenciales(email, model.Password))
            {
                _logger.LogWarning("Intento de login fallido para {Email}", email);
                ModelState.AddModelError(
                    string.Empty,
                    "El correo o la contraseña son incorrectos.");
                return View(model);
            }

            _logger.LogInformation("Login exitoso para {Email}", email);
            await _authService.IniciarSesionAsync(HttpContext);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Comercios");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.CerrarSesionAsync(HttpContext);
            return RedirectToAction("Index", "Home");
        }
    }
}
