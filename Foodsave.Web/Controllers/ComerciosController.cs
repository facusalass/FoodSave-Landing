using Foodsave.Web.Data;
using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Controllers
{
    [Authorize]
    public class ComerciosController : Controller
    {
        private static readonly string[] PlanesPermitidos = ["Estandar", "Pro"];

        private readonly ApplicationDbContext _context;
        private readonly GestionSuscripcionesService _gestionSuscripciones;

        public ComerciosController(
            ApplicationDbContext context,
            GestionSuscripcionesService gestionSuscripciones)
        {
            _context = context;
            _gestionSuscripciones = gestionSuscripciones;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var comercios = await _context.Comercios
                .AsNoTracking()
                .Include(c => c.Titular)
                .Include(c => c.Suscripciones)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var model = comercios.Select(comercio =>
            {
                var suscripcion = _gestionSuscripciones.ObtenerSuscripcionActual(
                    comercio.Suscripciones,
                    hoy);

                return new ComercioAdministracionViewModel
                {
                    Comercio = comercio,
                    SuscripcionActual = suscripcion,
                    EstadoPagoEfectivo =
                        _gestionSuscripciones.ObtenerEstadoPagoEfectivo(
                            suscripcion,
                            hoy)
                };
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var comercio = await _context.Comercios
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Titular)
                .Include(c => c.Suscripciones)
                .Include(c => c.Pagos)
                    .ThenInclude(p => p.Suscripcion)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comercio is null)
            {
                return NotFound();
            }

            comercio.Suscripciones = comercio.Suscripciones
                .OrderByDescending(s => s.FechaInicio)
                .ToList();
            comercio.Pagos = comercio.Pagos
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.Id)
                .ToList();

            var suscripcion = _gestionSuscripciones.ObtenerSuscripcionActual(
                comercio.Suscripciones,
                DateTime.Today);

            return View(new ComercioAdministracionViewModel
            {
                Comercio = comercio,
                SuscripcionActual = suscripcion,
                EstadoPagoEfectivo =
                    _gestionSuscripciones.ObtenerEstadoPagoEfectivo(
                        suscripcion,
                        DateTime.Today)
            });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Comercio());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Comercio comercio,
            string planInicial,
            decimal montoMensual)
        {
            var plan = NormalizarPlan(planInicial);
            if (plan is null)
            {
                ModelState.AddModelError(
                    nameof(planInicial),
                    "Seleccioná un plan válido.");
            }

            if (montoMensual < 0)
            {
                ModelState.AddModelError(
                    nameof(montoMensual),
                    "El monto mensual debe ser cero o mayor.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["PlanInicial"] = planInicial;
                ViewData["MontoMensual"] = montoMensual;
                return View(comercio);
            }

            var hoy = DateTime.Today;
            comercio.EstadoAdministrativo = Comercio.EstadoPendientePago;
            comercio.Suscripciones =
            [
                new Suscripcion
                {
                    Plan = plan!,
                    Estado = Suscripcion.EstadoActiva,
                    FechaInicio = hoy,
                    FechaFin = hoy.AddMonths(1),
                    MontoMensual = montoMensual,
                    FechaProximoVencimiento = hoy,
                    EstadoPago = Suscripcion.EstadoPagoPendiente
                }
            ];

            _context.Comercios.Add(comercio);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Comercio creado. La suscripción quedó pendiente de pago.";
            return RedirectToAction(nameof(Details), new { id = comercio.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inhabilitar(int id)
        {
            var comercio = await _context.Comercios.FindAsync(id);
            if (comercio is null)
            {
                return NotFound();
            }

            comercio.EstadoAdministrativo = Comercio.EstadoInhabilitado;
            await _context.SaveChangesAsync();
            TempData["Success"] = "El comercio fue inhabilitado.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var comercio = await _context.Comercios
                .Include(c => c.Suscripciones)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comercio is null)
            {
                return NotFound();
            }

            var suscripcion = _gestionSuscripciones.ObtenerSuscripcionActual(
                comercio.Suscripciones,
                DateTime.Today);
            comercio.EstadoAdministrativo =
                _gestionSuscripciones.ObtenerEstadoAlReactivar(
                    suscripcion,
                    DateTime.Today);

            await _context.SaveChangesAsync();
            TempData["Success"] =
                comercio.EstadoAdministrativo == Comercio.EstadoActivo
                    ? "El comercio fue reactivado."
                    : "El comercio fue reactivado y quedó pendiente de pago.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarPendientePago(int id)
        {
            var resultado = await ObtenerComercioYSuscripcionActual(id);
            if (resultado.Comercio is null)
            {
                return NotFound();
            }

            if (resultado.Suscripcion is null)
            {
                TempData["Error"] =
                    "El comercio no tiene una suscripción para actualizar.";
                return RedirectToAction(nameof(Details), new { id });
            }

            resultado.Suscripcion.EstadoPago =
                Suscripcion.EstadoPagoPendiente;
            if (resultado.Comercio.EstadoAdministrativo !=
                Comercio.EstadoInhabilitado)
            {
                resultado.Comercio.EstadoAdministrativo =
                    Comercio.EstadoPendientePago;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "El comercio quedó pendiente de pago.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarAlDia(int id)
        {
            var resultado = await ObtenerComercioYSuscripcionActual(id);
            if (resultado.Comercio is null)
            {
                return NotFound();
            }

            if (resultado.Suscripcion is null)
            {
                TempData["Error"] =
                    "El comercio no tiene una suscripción para actualizar.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var hoy = DateTime.Today;
            resultado.Suscripcion.EstadoPago = Suscripcion.EstadoPagoAlDia;
            resultado.Suscripcion.Estado = Suscripcion.EstadoActiva;
            if (resultado.Suscripcion.FechaProximoVencimiento.Date < hoy)
            {
                resultado.Suscripcion.FechaProximoVencimiento =
                    hoy.AddMonths(1);
            }

            if (resultado.Comercio.EstadoAdministrativo ==
                Comercio.EstadoPendientePago)
            {
                resultado.Comercio.EstadoAdministrativo =
                    Comercio.EstadoActivo;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] =
                "La suscripción fue marcada al día sin registrar un cobro.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarSuscripcion(
            int id,
            ActualizarSuscripcionInputModel model)
        {
            if (id != model.ComercioId)
            {
                return BadRequest();
            }

            var plan = NormalizarPlan(model.Plan);
            if (plan is null)
            {
                ModelState.AddModelError(
                    nameof(model.Plan),
                    "Seleccioná un plan válido.");
            }

            if (model.FechaProximoVencimiento == default)
            {
                ModelState.AddModelError(
                    nameof(model.FechaProximoVencimiento),
                    "Ingresá el próximo vencimiento.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(
                    " ",
                    ModelState.Values
                        .SelectMany(value => value.Errors)
                        .Select(error => error.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id });
            }

            var suscripcion = await _context.Suscripciones
                .FirstOrDefaultAsync(
                    s => s.Id == model.SuscripcionId &&
                         s.ComercioId == model.ComercioId);
            if (suscripcion is null)
            {
                return NotFound();
            }

            suscripcion.Plan = plan!;
            suscripcion.MontoMensual = model.MontoMensual;
            suscripcion.FechaProximoVencimiento =
                model.FechaProximoVencimiento.Date;
            await _context.SaveChangesAsync();

            TempData["Success"] = "La suscripción fue actualizada.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> RegistrarPago(int id)
        {
            var comercio = await _context.Comercios
                .AsNoTracking()
                .Include(c => c.Suscripciones)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comercio is null)
            {
                return NotFound();
            }

            var suscripcion = _gestionSuscripciones.ObtenerSuscripcionActual(
                comercio.Suscripciones,
                DateTime.Today);
            if (suscripcion is null)
            {
                TempData["Error"] =
                    "El comercio no tiene una suscripción para cobrar.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(new RegistrarPagoInputModel
            {
                ComercioId = comercio.Id,
                SuscripcionId = suscripcion.Id,
                ComercioNombre = comercio.Nombre,
                Plan = suscripcion.Plan,
                Monto = suscripcion.MontoMensual,
                FechaPago = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPago(
            int id,
            RegistrarPagoInputModel model)
        {
            if (id != model.ComercioId)
            {
                return BadRequest();
            }

            if (model.FechaPago == default)
            {
                ModelState.AddModelError(
                    nameof(model.FechaPago),
                    "Ingresá la fecha del pago.");
            }
            else if (model.FechaPago.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.FechaPago),
                    "La fecha del pago no puede ser futura.");
            }

            var comercio = await _context.Comercios
                .Include(c => c.Suscripciones)
                .FirstOrDefaultAsync(c => c.Id == id);
            var suscripcion = comercio?.Suscripciones
                .FirstOrDefault(s => s.Id == model.SuscripcionId);

            if (comercio is null || suscripcion is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.ComercioNombre = comercio.Nombre;
                model.Plan = suscripcion.Plan;
                return View(model);
            }

            var fechaPago = model.FechaPago.Date;
            _context.Pagos.Add(new Pago
            {
                ComercioId = comercio.Id,
                SuscripcionId = suscripcion.Id,
                Monto = model.Monto,
                FechaPago = fechaPago,
                Observacion = NormalizarOpcional(model.Observacion)
            });

            suscripcion.FechaUltimoPago = fechaPago;
            suscripcion.FechaProximoVencimiento = fechaPago.AddMonths(1);
            suscripcion.EstadoPago = Suscripcion.EstadoPagoAlDia;
            suscripcion.Estado = Suscripcion.EstadoActiva;

            if (comercio.EstadoAdministrativo ==
                Comercio.EstadoPendientePago)
            {
                comercio.EstadoAdministrativo = Comercio.EstadoActivo;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Pago registrado y suscripción actualizada.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<(
            Comercio? Comercio,
            Suscripcion? Suscripcion)> ObtenerComercioYSuscripcionActual(int id)
        {
            var comercio = await _context.Comercios
                .Include(c => c.Suscripciones)
                .FirstOrDefaultAsync(c => c.Id == id);
            var suscripcion = comercio is null
                ? null
                : _gestionSuscripciones.ObtenerSuscripcionActual(
                    comercio.Suscripciones,
                    DateTime.Today);

            return (comercio, suscripcion);
        }

        private static string? NormalizarPlan(string? plan)
        {
            return PlanesPermitidos.FirstOrDefault(
                permitido => permitido.Equals(
                    plan?.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        private static string? NormalizarOpcional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
