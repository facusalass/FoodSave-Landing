using Foodsave.Web.Data;
using Foodsave.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Services
{
    public class EstadisticasService
    {
        private readonly ApplicationDbContext _context;
        private readonly GestionSuscripcionesService _gestionSuscripciones;

        public EstadisticasService(
            ApplicationDbContext context,
            GestionSuscripcionesService gestionSuscripciones)
        {
            _context = context;
            _gestionSuscripciones = gestionSuscripciones;
        }

        public async Task<EstadisticasViewModel> ObtenerEstadisticasAsync()
        {
            var hoy = DateTime.Today;

            var conteos = await _context.Comercios
                .GroupBy(c => c.EstadoAdministrativo)
                .Select(g => new { Estado = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Estado, v => v.Count);

            var comercios = await _context.Comercios
                .AsNoTracking()
                .Include(c => c.Suscripciones)
                .ToListAsync();

            var datosPago = await _context.Pagos
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Sum(p => (decimal?)p.Monto) ?? 0,
                    Cantidad = g.Count()
                })
                .FirstOrDefaultAsync();

            var solicitudesPendientes = await _context.SolicitudesComercio
                .CountAsync(s => s.Estado == EstadoSolicitud.Pendiente);

            int GetCount(EstadoAdministrativo estado) =>
                conteos.TryGetValue(estado, out var count) ? count : 0;

            var totalComercios = conteos.Values.Sum();
            var activos = GetCount(EstadoAdministrativo.Activo);
            var inhabilitados = GetCount(EstadoAdministrativo.Inhabilitado);
            var pendientesPago = GetCount(EstadoAdministrativo.PendientePago);

            int alDia = 0, vencidos = 0;
            decimal ingresosEstimados = 0;

            foreach (var comercio in comercios)
            {
                var (suscripcion, estadoPago) =
                    _gestionSuscripciones.ObtenerEstadoCompleto(
                        comercio.Suscripciones, hoy);

                if (estadoPago == EstadoPagoSuscripcion.AlDia)
                    alDia++;
                else if (estadoPago == EstadoPagoSuscripcion.Vencido)
                    vencidos++;

                if (comercio.EstadoAdministrativo != EstadoAdministrativo.Inhabilitado &&
                    suscripcion is not null &&
                    suscripcion.FechaInicio.Date <= hoy &&
                    suscripcion.FechaFin.Date >= hoy)
                {
                    ingresosEstimados += suscripcion.MontoMensual;
                }
            }

            var datos = datosPago ?? new { Total = 0m, Cantidad = 0 };

            return new EstadisticasViewModel
            {
                TotalComercios = totalComercios,
                ComerciosActivos = activos,
                ComerciosInhabilitados = inhabilitados,
                ComerciosPendientesPago = pendientesPago,
                ComerciosAlDia = alDia,
                ComerciosVencidos = vencidos,
                IngresosEstimadosMes = ingresosEstimados,
                IngresosRegistradosTotales = datos.Total,
                CantidadPagosRegistrados = datos.Cantidad,
                SolicitudesPendientes = solicitudesPendientes
            };
        }
    }
}
