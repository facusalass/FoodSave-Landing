using System.ComponentModel.DataAnnotations;

namespace Foodsave.Web.Models
{
    public class Comercio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rubro { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public EstadoAdministrativo EstadoAdministrativo { get; set; } = EstadoAdministrativo.Activo;

        [MaxLength(100)]
        public string? FoodSaveBusinessId { get; set; }

        public int TitularId { get; set; }
        public Titular Titular { get; set; } = new();

        public List<Suscripcion> Suscripciones { get; set; } = new();
        public List<Pago> Pagos { get; set; } = new();
    }
}
