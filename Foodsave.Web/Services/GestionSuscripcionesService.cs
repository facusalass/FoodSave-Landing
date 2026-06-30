using Foodsave.Web.Models;

namespace Foodsave.Web.Services
{
    public class GestionSuscripcionesService
    {
        public Suscripcion? ObtenerSuscripcionActual(
            IEnumerable<Suscripcion> suscripciones,
            DateTime fecha)
        {
            var dia = fecha.Date;
            var lista = suscripciones.ToList();

            return lista
                .Where(s => s.FechaInicio.Date <= dia && s.FechaFin.Date >= dia)
                .OrderByDescending(s => s.FechaInicio)
                .FirstOrDefault()
                ?? lista
                    .Where(s => s.FechaInicio.Date <= dia)
                    .OrderByDescending(s => s.FechaInicio)
                    .FirstOrDefault()
                ?? lista
                    .OrderBy(s => s.FechaInicio)
                    .FirstOrDefault();
        }

        public (Suscripcion? Suscripcion, EstadoPagoSuscripcion EstadoPago) ObtenerEstadoCompleto(
            IEnumerable<Suscripcion> suscripciones,
            DateTime fecha)
        {
            var suscripcion = ObtenerSuscripcionActual(suscripciones, fecha);
            var estadoPago = ObtenerEstadoPagoEfectivo(suscripcion, fecha);
            return (suscripcion, estadoPago);
        }

        public EstadoPagoSuscripcion ObtenerEstadoPagoEfectivo(
            Suscripcion? suscripcion,
            DateTime fecha)
        {
            if (suscripcion is null)
                return EstadoPagoSuscripcion.Pendiente;

            if (suscripcion.FechaProximoVencimiento.Date < fecha.Date)
                return EstadoPagoSuscripcion.Vencido;

            return suscripcion.EstadoPago;
        }

        public EstadoAdministrativo ObtenerEstadoAlReactivar(
            Suscripcion? suscripcion,
            DateTime fecha)
        {
            return ObtenerEstadoPagoEfectivo(suscripcion, fecha) ==
                   EstadoPagoSuscripcion.AlDia
                ? EstadoAdministrativo.Activo
                : EstadoAdministrativo.PendientePago;
        }
    }
}
