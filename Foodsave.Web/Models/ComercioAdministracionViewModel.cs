namespace Foodsave.Web.Models
{
    public class ComercioAdministracionViewModel
    {
        public Comercio Comercio { get; set; } = null!;
        public Suscripcion? SuscripcionActual { get; set; }
        public EstadoPagoSuscripcion EstadoPagoEfectivo { get; set; } = EstadoPagoSuscripcion.Pendiente;
    }
}
