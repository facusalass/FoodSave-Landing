using System.ComponentModel.DataAnnotations;

namespace Foodsave.Web.Models
{
    public class ActualizarSuscripcionInputModel
    {
        [Required]
        public int ComercioId { get; set; }

        [Required]
        public int SuscripcionId { get; set; }

        [Required(ErrorMessage = "Seleccioná un plan.")]
        [MaxLength(20)]
        public string Plan { get; set; } = string.Empty;

        [Range(
            0,
            9999999999.99,
            ErrorMessage = "El monto mensual debe ser cero o mayor.")]
        public decimal MontoMensual { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaProximoVencimiento { get; set; }
    }
}
