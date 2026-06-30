using System.ComponentModel.DataAnnotations;

namespace Foodsave.Web.Models
{
    public class SolicitudComercioInputModel
    {
        [Required(ErrorMessage = "Ingresá el nombre del comercio.")]
        [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        [Display(Name = "Nombre del comercio")]
        public string NombreComercio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá el rubro.")]
        [MaxLength(100, ErrorMessage = "El rubro no puede superar los 100 caracteres.")]
        public string Rubro { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "Ingresá el teléfono del comercio.")]
        [MaxLength(40, ErrorMessage = "El teléfono no puede superar los 40 caracteres.")]
        [Phone(ErrorMessage = "Ingresá un teléfono válido.")]
        [Display(Name = "Teléfono del comercio")]
        public string TelefonoComercio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá el nombre del titular o responsable.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        [Display(Name = "Nombre del titular o responsable")]
        public string NombreTitular { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
        [Display(Name = "Apellido del titular o responsable")]
        public string? ApellidoTitular { get; set; }

        [MaxLength(40, ErrorMessage = "El teléfono no puede superar los 40 caracteres.")]
        [Phone(ErrorMessage = "Ingresá un teléfono válido.")]
        [Display(Name = "Teléfono del titular")]
        public string? TelefonoTitular { get; set; }

        [Required(ErrorMessage = "Ingresá el email del titular.")]
        [EmailAddress(ErrorMessage = "Ingresá un email válido.")]
        [MaxLength(200, ErrorMessage = "El email no puede superar los 200 caracteres.")]
        [Display(Name = "Email del titular")]
        public string EmailTitular { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "El mensaje no puede superar los 2000 caracteres.")]
        [Display(Name = "Mensaje o comentario")]
        public string? Mensaje { get; set; }

        [RegularExpression(
            "^(Estandar|Pro)$",
            ErrorMessage = "Seleccioná un plan válido.")]
        [Display(Name = "Plan de interés")]
        public string? PlanInteres { get; set; }

        public SolicitudComercio ToEntity()
        {
            return new SolicitudComercio
            {
                NombreComercio = NombreComercio.Trim(),
                Rubro = Rubro.Trim(),
                Direccion = Helpers.TextHelper.NormalizarOpcional(Direccion),
                TelefonoComercio = TelefonoComercio.Trim(),
                NombreTitular = NombreTitular.Trim(),
                ApellidoTitular = Helpers.TextHelper.NormalizarOpcional(ApellidoTitular),
                TelefonoTitular = Helpers.TextHelper.NormalizarOpcional(TelefonoTitular),
                EmailTitular = EmailTitular.Trim().ToLowerInvariant(),
                Mensaje = Helpers.TextHelper.NormalizarOpcional(Mensaje),
                PlanInteres = Helpers.PlanHelper.ParsePlan(PlanInteres),
                Estado = EstadoSolicitud.Pendiente,
                FechaSolicitud = DateTime.UtcNow
            };
        }
    }
}
