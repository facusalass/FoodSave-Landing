using System.ComponentModel.DataAnnotations;

namespace Foodsave.Web.Models
{
    public class RegistrarPagoInputModel
    {
        [Required]
        public int ComercioId { get; set; }

        [Required]
        public int SuscripcionId { get; set; }

        public string ComercioNombre { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;

        [Range(
            0.01,
            9999999999.99,
            ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Monto { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaPago { get; set; }

        [MaxLength(500, ErrorMessage = "La observación no puede superar los 500 caracteres.")]
        public string? Observacion { get; set; }
    }
}
