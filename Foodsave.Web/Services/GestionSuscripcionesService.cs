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

        public string ObtenerEstadoPagoEfectivo(
            Suscripcion? suscripcion,
            DateTime fecha)
        {
            if (suscripcion is null)
            {
                return Suscripcion.EstadoPagoPendiente;
            }

            if (suscripcion.FechaProximoVencimiento.Date < fecha.Date)
            {
                return Suscripcion.EstadoPagoVencido;
            }

            return suscripcion.EstadoPago switch
            {
                Suscripcion.EstadoPagoAlDia => Suscripcion.EstadoPagoAlDia,
                Suscripcion.EstadoPagoVencido => Suscripcion.EstadoPagoVencido,
                _ => Suscripcion.EstadoPagoPendiente
            };
        }

        public string ObtenerEstadoAlReactivar(
            Suscripcion? suscripcion,
            DateTime fecha)
        {
            return ObtenerEstadoPagoEfectivo(suscripcion, fecha) ==
                   Suscripcion.EstadoPagoAlDia
                ? Comercio.EstadoActivo
                : Comercio.EstadoPendientePago;
        }
    }
}
