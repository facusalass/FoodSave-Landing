namespace Foodsave.Web.Models
{
    public enum EstadoAdministrativo
    {
        Activo,
        Inhabilitado,
        PendientePago
    }

    public enum EstadoSuscripcion
    {
        Activa,
        Pendiente,
        Vencida
    }

    public enum EstadoPagoSuscripcion
    {
        AlDia,
        Pendiente,
        Vencido
    }

    public enum PlanSuscripcion
    {
        Estandar,
        Pro
    }

    public enum EstadoSolicitud
    {
        Pendiente,
        Aceptada,
        Rechazada
    }
}
