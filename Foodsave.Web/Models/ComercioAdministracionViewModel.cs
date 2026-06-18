namespace Foodsave.Web.Models
{
    public class ComercioAdministracionViewModel
    {
        public Comercio Comercio { get; set; } = null!;
        public Suscripcion? SuscripcionActual { get; set; }
        public string EstadoPagoEfectivo { get; set; } = Suscripcion.EstadoPagoPendiente;
    }
}
