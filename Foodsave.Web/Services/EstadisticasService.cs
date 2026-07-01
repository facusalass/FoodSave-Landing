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
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioAno = new DateTime(hoy.Year, 1, 1);

            var conteos = await _context.Comercios
                .GroupBy(c => c.EstadoAdministrativo)
                .Select(g => new { Estado = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Estado, v => v.Count);

            var comercios = await _context.Comercios
                .AsNoTracking()
                .Include(c => c.Suscripciones)
                .ToListAsync();

            int GetCount(EstadoAdministrativo estado) =>
                conteos.TryGetValue(estado, out var count) ? count : 0;

            int alDia = 0, vencidos = 0;
            decimal mrr = 0;

            foreach (var comercio in comercios)
            {
                var (suscripcion, estadoPago) =
                    _gestionSuscripciones.ObtenerEstadoCompleto(
                        comercio.Suscripciones, hoy);

                if (estadoPago == EstadoPagoSuscripcion.AlDia) alDia++;
                else if (estadoPago == EstadoPagoSuscripcion.Vencido) vencidos++;

                if (comercio.EstadoAdministrativo != EstadoAdministrativo.Inhabilitado &&
                    suscripcion is not null &&
                    suscripcion.FechaInicio.Date <= hoy &&
                    (suscripcion.FechaFin == null || suscripcion.FechaFin.Value.Date >= hoy))
                {
                    mrr += suscripcion.MontoMensual;
                }
            }

            var pagosMes = await _context.Pagos
                .Where(p => p.FechaPago >= inicioMes)
                .ToListAsync();

            var cobradoMes = pagosMes.Sum(p => p.Monto);
            var comerciosPagaron = pagosMes.Select(p => p.ComercioId).Distinct().Count();

            var totalDebenPagar = GetCount(EstadoAdministrativo.Activo) + GetCount(EstadoAdministrativo.PendientePago);
            var tasa = totalDebenPagar > 0
                ? (int)Math.Round((double)comerciosPagaron / totalDebenPagar * 100)
                : 0;

            var nuevosMes = await _context.Comercios
                .CountAsync(c => c.Suscripciones.Any(s => s.FechaInicio >= inicioMes));

            var nuevosAno = await _context.Comercios
                .CountAsync(c => c.Suscripciones.Any(s => s.FechaInicio >= inicioAno));

            var datosAno = await _context.Pagos
                .Where(p => p.FechaPago >= inicioAno)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Sum(p => (decimal?)p.Monto) ?? 0,
                    Cantidad = g.Count()
                })
                .FirstOrDefaultAsync();

            var solicitudesPendientes = await _context.SolicitudesComercio
                .CountAsync(s => s.Estado == EstadoSolicitud.Pendiente);

            var solicitudesMes = await _context.SolicitudesComercio
                .CountAsync(s => s.FechaSolicitud >= inicioMes);

            return new EstadisticasViewModel
            {
                TotalComercios = conteos.Values.Sum(),
                ComerciosActivos = GetCount(EstadoAdministrativo.Activo),
                ComerciosInhabilitados = GetCount(EstadoAdministrativo.Inhabilitado),
                ComerciosPendientesPago = GetCount(EstadoAdministrativo.PendientePago),
                ComerciosAlDia = alDia,
                ComerciosVencidos = vencidos,

                MontoMensualRecurrente = mrr,
                IngresosCobradosMes = cobradoMes,
                ComerciosPagaronMes = comerciosPagaron,
                TasaCobranza = tasa,

                NuevosComerciosMes = nuevosMes,
                NuevosComerciosAno = nuevosAno,

                IngresosAnuales = datosAno?.Total ?? 0,
                PagosAnuales = datosAno?.Cantidad ?? 0,

                SolicitudesPendientes = solicitudesPendientes,
                SolicitudesRecibidasMes = solicitudesMes
            };
        }
    }
}
