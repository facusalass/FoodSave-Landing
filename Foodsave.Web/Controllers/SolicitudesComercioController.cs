using Foodsave.Web.Data;
using Foodsave.Web.Helpers;
using Foodsave.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Controllers
{
    [Authorize]
    public class SolicitudesComercioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SolicitudesComercioController> _logger;

        public SolicitudesComercioController(
            ApplicationDbContext context,
            ILogger<SolicitudesComercioController> logger)
        {
            _context = context;
            _logger = logger;
            ViewData["ActivePage"] = "Solicitudes";
        }

        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesComercio
                .OrderBy(s => s.Estado != EstadoSolicitud.Pendiente)
                .ThenByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var solicitud = await _context.SolicitudesComercio.FindAsync(id);
            return solicitud is null ? NotFound() : View(solicitud);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new SolicitudComercioInputModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [EnableRateLimiting("SolicitudForm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolicitudComercioInputModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var solicitud = model.ToEntity();
            _context.SolicitudesComercio.Add(solicitud);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Solicitud recibida: {Nombre}", solicitud.NombreComercio);

            return RedirectToAction(nameof(Confirmacion));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Confirmacion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aceptar(
            int id,
            string? observacionAdmin)
        {
            return await Review(id, EstadoSolicitud.Aceptada, observacionAdmin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(
            int id,
            string? observacionAdmin)
        {
            return await Review(id, EstadoSolicitud.Rechazada, observacionAdmin);
        }

        private async Task<IActionResult> Review(
            int id,
            EstadoSolicitud nextStatus,
            string? observation)
        {
            var solicitud = await _context.SolicitudesComercio.FindAsync(id);
            if (solicitud is null)
                return NotFound();

            if (solicitud.Estado != EstadoSolicitud.Pendiente)
            {
                TempData["Error"] = "La solicitud ya fue revisada.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var normalizedObservation = TextHelper.NormalizarOpcional(observation);
            if (normalizedObservation?.Length > 1000)
            {
                TempData["Error"] =
                    "La observación no puede superar los 1000 caracteres.";
                return RedirectToAction(nameof(Details), new { id });
            }

            solicitud.Estado = nextStatus;
            solicitud.FechaRevision = DateTime.UtcNow;
            solicitud.ObservacionAdmin = normalizedObservation;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Solicitud {Estado}: Id={Id}, {Nombre}",
                nextStatus, id, solicitud.NombreComercio);

            TempData["Success"] = nextStatus == EstadoSolicitud.Aceptada
                ? "Solicitud aceptada. Ya podés inscribir el comercio manualmente."
                : "Solicitud rechazada.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
