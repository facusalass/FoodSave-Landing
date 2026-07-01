using Foodsave.Web.Data;
using Foodsave.Web.Helpers;
using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Controllers
{
    [Authorize]
    public partial class ComerciosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GestionSuscripcionesService _gestionSuscripciones;
        private readonly RegistroPagoService _registroPagoService;
        private readonly ILogger<ComerciosController> _logger;

        public ComerciosController(
            ApplicationDbContext context,
            GestionSuscripcionesService gestionSuscripciones,
            RegistroPagoService registroPagoService,
            ILogger<ComerciosController> logger)
        {
            _context = context;
            _gestionSuscripciones = gestionSuscripciones;
            _registroPagoService = registroPagoService;
            _logger = logger;
            ViewData["ActivePage"] = "Comercios";
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Breadcrumb"] = new (string, string?, string?)[] { ("Comercios", null, null) };
            var hoy = DateTime.Today;
            var comercios = await _context.Comercios
                .AsNoTracking()
                .Include(c => c.Titular)
                .Include(c => c.Suscripciones)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var model = comercios.Select(comercio =>
            {
                var (suscripcion, estadoPago) =
                    _gestionSuscripciones.ObtenerEstadoCompleto(
                        comercio.Suscripciones, hoy);

                return new ComercioAdministracionViewModel
                {
                    Comercio = comercio,
                    SuscripcionActual = suscripcion,
                    EstadoPagoEfectivo = estadoPago
                };
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            return await DetailsView(id);
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
            var plan = PlanHelper.NormalizarPlan(planInicial);
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
            comercio.EstadoAdministrativo = EstadoAdministrativo.PendientePago;
            comercio.Suscripciones =
            [
                new Suscripcion
                {
                    Plan = Enum.Parse<PlanSuscripcion>(plan!),
                    Estado = EstadoSuscripcion.Activa,
                    FechaInicio = hoy,
                    MontoMensual = montoMensual,
                    FechaProximoVencimiento = hoy.AddDays(30),
                    EstadoPago = EstadoPagoSuscripcion.Pendiente
                }
            ];

            _context.Comercios.Add(comercio);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Comercio creado: {Nombre} (Id={Id})",
                comercio.Nombre, comercio.Id);

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
                return NotFound();

            comercio.EstadoAdministrativo = EstadoAdministrativo.Inhabilitado;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Comercio inhabilitado: Id={Id}", id);
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
                return NotFound();

            var suscripcion = _gestionSuscripciones.ObtenerSuscripcionActual(
                comercio.Suscripciones, DateTime.Today);
            comercio.EstadoAdministrativo =
                _gestionSuscripciones.ObtenerEstadoAlReactivar(
                    suscripcion, DateTime.Today);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Comercio reactivado: Id={Id}, Estado={Estado}",
                id, comercio.EstadoAdministrativo);

            TempData["Success"] =
                comercio.EstadoAdministrativo == EstadoAdministrativo.Activo
                    ? "El comercio fue reactivado."
                    : "El comercio fue reactivado y quedó pendiente de pago.";

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<IActionResult> DetailsView(int id)
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
                return NotFound();

            comercio.Suscripciones = comercio.Suscripciones
                .OrderByDescending(s => s.FechaInicio)
                .ToList();
            comercio.Pagos = comercio.Pagos
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.Id)
                .ToList();

            var (suscripcion, estadoPago) =
                _gestionSuscripciones.ObtenerEstadoCompleto(
                    comercio.Suscripciones, DateTime.Today);

            return View("Details", new ComercioAdministracionViewModel
            {
                Comercio = comercio,
                SuscripcionActual = suscripcion,
                EstadoPagoEfectivo = estadoPago
            });
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
                    comercio.Suscripciones, DateTime.Today);

            return (comercio, suscripcion);
        }
    }
}
