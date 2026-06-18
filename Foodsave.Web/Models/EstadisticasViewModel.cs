namespace Foodsave.Web.Models
{
    public class EstadisticasViewModel
    {
        public int TotalComercios { get; set; }
        public int ComerciosActivos { get; set; }
        public int ComerciosInhabilitados { get; set; }
        public int ComerciosPendientesPago { get; set; }
        public int ComerciosAlDia { get; set; }
        public int ComerciosVencidos { get; set; }
        public decimal IngresosEstimadosMes { get; set; }
        public decimal IngresosRegistradosTotales { get; set; }
        public int CantidadPagosRegistrados { get; set; }
        public int SolicitudesPendientes { get; set; }
    }
}
