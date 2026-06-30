namespace Foodsave.Web.Models
{
    public class RegistroPagoCommand
    {
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string? Observacion { get; set; }
    }
}
