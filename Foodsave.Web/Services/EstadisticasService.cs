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

            var comercios = await _context.Comercios
                .AsNoTracking()
                .Include(c => c.Suscripciones)
                .ToListAsync();

            var pagos = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= inicioMes)
                .ToListAsync();

            var pagosAno = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= inicioAno)
                .ToListAsync();

            var solicitudes = await _context.SolicitudesComercio
                .AsNoTracking()
                .ToListAsync();

            // Conteos por estado administrativo
            var conteos = comercios
                .GroupBy(c => c.EstadoAdministrativo)
                .ToDictionary(g => g.Key, g => g.Count());

            int GetCount(EstadoAdministrativo estado) =>
                conteos.TryGetValue(estado, out var count) ? count : 0;

            int alDia = 0, vencidos = 0;
            decimal mrr = 0;

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
                    (suscripcion.FechaFin == null || suscripcion.FechaFin.Value.Date >= hoy))
                {
                    mrr += suscripcion.MontoMensual;
                }
            }

            var cobradoMes = pagos.Sum(p => p.Monto);
            var comerciosPagaron = pagos.Select(p => p.ComercioId).Distinct().Count();

            var totalDebenPagar = GetCount(EstadoAdministrativo.Activo) +
                                  GetCount(EstadoAdministrativo.PendientePago);

            var tasa = totalDebenPagar > 0
                ? (int)Math.Round((double)comerciosPagaron / totalDebenPagar * 100)
                : 0;

            var activas = comercios
                .SelectMany(c => c.Suscripciones)
                .Where(s => s.Estado == EstadoSuscripcion.Activa)
                .ToList();

            var nuevosMes = activas.Count(s => s.FechaInicio >= inicioMes);
            var nuevosAno = activas.Count(s => s.FechaInicio >= inicioAno);

            var ingresosAnuales = pagosAno.Sum(p => p.Monto);
            var pagosAnualesCount = pagosAno.Count;

            var solicitudesPendientes = solicitudes
                .Count(s => s.Estado == EstadoSolicitud.Pendiente);

            var solicitudesMes = solicitudes
                .Count(s => s.FechaSolicitud >= inicioMes);

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

                IngresosAnuales = ingresosAnuales,
                PagosAnuales = pagosAnualesCount,

                SolicitudesPendientes = solicitudesPendientes,
                SolicitudesRecibidasMes = solicitudesMes
            };
        }
    }
}
