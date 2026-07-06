using System.ComponentModel.DataAnnotations;

namespace Foodsave.Web.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public int ComercioId { get; set; }
        public int SuscripcionId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }

        [MaxLength(500)]
        public string? Observacion { get; set; }

        public Comercio Comercio { get; set; } = null!;
        public Suscripcion Suscripcion { get; set; } = null!;
    }
}
