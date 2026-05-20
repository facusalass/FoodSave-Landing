namespace Foodsave.Web.Models
{
    public class Suscripcion
    {
        public int Id { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
