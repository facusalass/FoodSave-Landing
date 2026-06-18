using Foodsave.Web.Data;
using Foodsave.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Controllers
{
    [Authorize]
    public class SolicitudesComercioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SolicitudesComercioController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesComercio
                .OrderBy(s => s.Estado != SolicitudComercio.EstadoPendiente)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolicitudComercioInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var solicitud = new SolicitudComercio
            {
                NombreComercio = model.NombreComercio.Trim(),
                Rubro = model.Rubro.Trim(),
                Direccion = NormalizeOptional(model.Direccion),
                TelefonoComercio = model.TelefonoComercio.Trim(),
                NombreTitular = model.NombreTitular.Trim(),
                ApellidoTitular = NormalizeOptional(model.ApellidoTitular),
                TelefonoTitular = NormalizeOptional(model.TelefonoTitular),
                EmailTitular = model.EmailTitular.Trim().ToLowerInvariant(),
                Mensaje = NormalizeOptional(model.Mensaje),
                PlanInteres = NormalizeOptional(model.PlanInteres),
                Estado = SolicitudComercio.EstadoPendiente,
                FechaSolicitud = DateTime.UtcNow
            };

            _context.SolicitudesComercio.Add(solicitud);
            await _context.SaveChangesAsync();

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
            return await Review(
                id,
                SolicitudComercio.EstadoAceptada,
                observacionAdmin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(
            int id,
            string? observacionAdmin)
        {
            return await Review(
                id,
                SolicitudComercio.EstadoRechazada,
                observacionAdmin);
        }

        private async Task<IActionResult> Review(
            int id,
            string nextStatus,
            string? observation)
        {
            var solicitud = await _context.SolicitudesComercio.FindAsync(id);
            if (solicitud is null)
            {
                return NotFound();
            }

            if (solicitud.Estado != SolicitudComercio.EstadoPendiente)
            {
                TempData["Error"] = "La solicitud ya fue revisada.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var normalizedObservation = NormalizeOptional(observation);
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

            TempData["Success"] = nextStatus == SolicitudComercio.EstadoAceptada
                ? "Solicitud aceptada. Ya podés inscribir el comercio manualmente."
                : "Solicitud rechazada.";

            return RedirectToAction(nameof(Details), new { id });
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
