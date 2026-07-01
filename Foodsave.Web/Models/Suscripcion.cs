using System.ComponentModel.DataAnnotations;

namespace Foodsave.Web.Models
{
    public class Suscripcion
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public PlanSuscripcion Plan { get; set; }

        [MaxLength(20)]
        public EstadoSuscripcion Estado { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public decimal MontoMensual { get; set; }
        public DateTime? FechaUltimoPago { get; set; }
        public DateTime FechaProximoVencimiento { get; set; }

        [MaxLength(20)]
        public EstadoPagoSuscripcion EstadoPago { get; set; } = EstadoPagoSuscripcion.Pendiente;

        public int ComercioId { get; set; }
        public Comercio? Comercio { get; set; }
        public List<Pago> Pagos { get; set; } = new();
    }
}
