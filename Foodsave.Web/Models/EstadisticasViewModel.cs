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

        public decimal MontoMensualRecurrente { get; set; }
        public decimal IngresosCobradosMes { get; set; }
        public int ComerciosPagaronMes { get; set; }
        public int TasaCobranza { get; set; }

        public int NuevosComerciosMes { get; set; }
        public int NuevosComerciosAno { get; set; }

        public decimal IngresosAnuales { get; set; }
        public int PagosAnuales { get; set; }

        public int SolicitudesPendientes { get; set; }
        public int SolicitudesRecibidasMes { get; set; }
    }
}
