using System.ComponentModel.DataAnnotations;

namespace Foodsave.Web.Models
{
    public class SolicitudComercio
    {
        public const string EstadoPendiente = "Pendiente";
        public const string EstadoAceptada = "Aceptada";
        public const string EstadoRechazada = "Rechazada";

        public int Id { get; set; }

        [MaxLength(150)]
        public string NombreComercio { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Rubro { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Direccion { get; set; }

        [MaxLength(40)]
        public string TelefonoComercio { get; set; } = string.Empty;

        [MaxLength(100)]
        public string NombreTitular { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ApellidoTitular { get; set; }

        [MaxLength(40)]
        public string? TelefonoTitular { get; set; }

        [MaxLength(200)]
        public string EmailTitular { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Mensaje { get; set; }

        [MaxLength(20)]
        public string? PlanInteres { get; set; }

        [MaxLength(20)]
        public string Estado { get; set; } = EstadoPendiente;

        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRevision { get; set; }

        [MaxLength(1000)]
        public string? ObservacionAdmin { get; set; }
    }
}
