using Foodsave.Web.Data;
using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Controllers
{
    [Authorize]
    public class EstadisticasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GestionSuscripcionesService _gestionSuscripciones;

        public EstadisticasController(
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
                .Include(c => c.Suscripciones)
                .ToListAsync();

            var estados = comercios.Select(comercio =>
            {
                var suscripcion = _gestionSuscripciones.ObtenerSuscripcionActual(
                    comercio.Suscripciones,
                    hoy);

                return new
                {
                    Comercio = comercio,
                    Suscripcion = suscripcion,
                    EstadoPago =
                        _gestionSuscripciones.ObtenerEstadoPagoEfectivo(
                            suscripcion,
                            hoy)
                };
            }).ToList();

            var model = new EstadisticasViewModel
            {
                TotalComercios = comercios.Count,
                ComerciosActivos = comercios.Count(
                    c => c.EstadoAdministrativo == Comercio.EstadoActivo),
                ComerciosInhabilitados = comercios.Count(
                    c => c.EstadoAdministrativo ==
                         Comercio.EstadoInhabilitado),
                ComerciosPendientesPago = comercios.Count(
                    c => c.EstadoAdministrativo ==
                         Comercio.EstadoPendientePago),
                ComerciosAlDia = estados.Count(
                    item => item.EstadoPago ==
                            Suscripcion.EstadoPagoAlDia),
                ComerciosVencidos = estados.Count(
                    item => item.EstadoPago ==
                            Suscripcion.EstadoPagoVencido),
                IngresosEstimadosMes = estados
                    .Where(item =>
                        item.Comercio.EstadoAdministrativo !=
                            Comercio.EstadoInhabilitado &&
                        item.Suscripcion is not null &&
                        item.Suscripcion.FechaInicio.Date <= hoy &&
                        item.Suscripcion.FechaFin.Date >= hoy)
                    .Sum(item => item.Suscripcion!.MontoMensual),
                IngresosRegistradosTotales =
                    await _context.Pagos.SumAsync(p => (decimal?)p.Monto) ?? 0,
                CantidadPagosRegistrados =
                    await _context.Pagos.CountAsync(),
                SolicitudesPendientes =
                    await _context.SolicitudesComercio.CountAsync(
                        s => s.Estado == SolicitudComercio.EstadoPendiente)
            };

            return View(model);
        }
    }
}
