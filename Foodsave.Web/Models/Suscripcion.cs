using System.ComponentModel.DataAnnotations;

namespace Foodsave.Web.Models
{
    public class Suscripcion
    {
        public const string EstadoActiva = "Activa";
        public const string EstadoPendiente = "Pendiente";
        public const string EstadoVencida = "Vencida";

        public const string EstadoPagoAlDia = "AlDia";
        public const string EstadoPagoPendiente = "Pendiente";
        public const string EstadoPagoVencido = "Vencido";

        public int Id { get; set; }

        [MaxLength(20)]
        public string Plan { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Estado { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public decimal MontoMensual { get; set; }
        public DateTime? FechaUltimoPago { get; set; }
        public DateTime FechaProximoVencimiento { get; set; }

        [MaxLength(20)]
        public string EstadoPago { get; set; } = EstadoPagoPendiente;

        public int? ComercioId { get; set; }
        public Comercio? Comercio { get; set; }
        public List<Pago> Pagos { get; set; } = new();
    }
}
