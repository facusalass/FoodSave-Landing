using System.ComponentModel.DataAnnotations;

namespace Foodsave.Web.Models
{
    public class ComercioEditarInputModel
    {
        [Required(ErrorMessage = "Ingresá el nombre del comercio.")]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá el rubro.")]
        [MaxLength(100)]
        public string Rubro { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá la dirección.")]
        [MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá el teléfono.")]
        [MaxLength(40)]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá el nombre del titular.")]
        [MaxLength(100)]
        public string TitularNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá el apellido del titular.")]
        [MaxLength(100)]
        public string TitularApellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá el email del titular.")]
        [EmailAddress(ErrorMessage = "Ingresá un email válido.")]
        [MaxLength(200)]
        public string TitularEmail { get; set; } = string.Empty;

        [MaxLength(40)]
        public string TitularTelefono { get; set; } = string.Empty;
    }
}
