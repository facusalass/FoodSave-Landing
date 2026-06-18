namespace Foodsave.Web.Models
{
    public class Comercio
    {
        public const string EstadoActivo = "Activo";
        public const string EstadoInhabilitado = "Inhabilitado";
        public const string EstadoPendientePago = "PendientePago";

        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rubro { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string EstadoAdministrativo { get; set; } = EstadoActivo;
        public Titular Titular { get; set; } = new();
        public List<Suscripcion> Suscripciones { get; set; } = new();
        public List<Pago> Pagos { get; set; } = new();
    }
}
