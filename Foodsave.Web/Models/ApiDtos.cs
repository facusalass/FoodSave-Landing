// DTOs = objetos planos para la API.
// No exponen las entidades de EF Core ni navegan propiedades de BD.
namespace Foodsave.Web.Models
{
    // Cada DTO tiene solo los campos necesarios para el cliente.
    // Los valores enum se serializan como string (ToString()), no como int.
    public class ComercioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rubro { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string EstadoAdministrativo { get; set; } = string.Empty;
        public string? FoodSaveBusinessId { get; set; }

        public TitularDto Titular { get; set; } = new();
        public List<SuscripcionDto> Suscripciones { get; set; } = new();
    }

    public class TitularDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }

    public class SuscripcionDto
    {
        public int Id { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
        public decimal MontoMensual { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime? FechaUltimoPago { get; set; }
        public DateTime FechaProximoVencimiento { get; set; }
    }

    public class SolicitudDto
    {
        public int Id { get; set; }
        public string NombreComercio { get; set; } = string.Empty;
        public string Rubro { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string TelefonoComercio { get; set; } = string.Empty;
        public string NombreTitular { get; set; } = string.Empty;
        public string? ApellidoTitular { get; set; }
        public string? TelefonoTitular { get; set; }
        public string EmailTitular { get; set; } = string.Empty;
        public string? Ciudad { get; set; }
        public string? Mensaje { get; set; }
        public string? PlanInteres { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRevision { get; set; }
        public string? ObservacionAdmin { get; set; }
    }

    public class PagoDto
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string? Observacion { get; set; }
        public string? Plan { get; set; }
    }

    // Métodos de extensión para mapear entidades → DTOs.
    // Se llaman desde los controladores: comercio.ToDto()
    public static class DtoMapper
    {
        public static ComercioDto ToDto(this Comercio comercio)
        {
            // Si se olvida el Include(), explota con un mensaje claro en lugar de NullReference.
            if (comercio.Titular is null)
                throw new InvalidOperationException(
                    $"Titular no cargado para Comercio {comercio.Id}. Usar .Include(c => c.Titular).");

            return new ComercioDto
            {
                Id = comercio.Id,
                Nombre = comercio.Nombre,
                Rubro = comercio.Rubro,
                Direccion = comercio.Direccion,
                Telefono = comercio.Telefono,
                Plan = comercio.Plan.ToString(),
                EstadoAdministrativo = comercio.EstadoAdministrativo.ToString(),
                FoodSaveBusinessId = comercio.FoodSaveBusinessId,
                Titular = new TitularDto
                {
                    Id = comercio.Titular.Id,
                    Nombre = comercio.Titular.Nombre,
                    Apellido = comercio.Titular.Apellido,
                    Email = comercio.Titular.Email,
                    Telefono = comercio.Titular.Telefono
                },
                Suscripciones = comercio.Suscripciones
                    .Select(s => s.ToDto())
                    .ToList()
            };
        }

        public static SuscripcionDto ToDto(this Suscripcion suscripcion)
        {
            return new SuscripcionDto
            {
                Id = suscripcion.Id,
                Plan = suscripcion.Plan.ToString(),
                Estado = suscripcion.Estado.ToString(),
                EstadoPago = suscripcion.EstadoPago.ToString(),
                MontoMensual = suscripcion.MontoMensual,
                FechaInicio = suscripcion.FechaInicio,
                FechaFin = suscripcion.FechaFin,
                FechaUltimoPago = suscripcion.FechaUltimoPago,
                FechaProximoVencimiento = suscripcion.FechaProximoVencimiento
            };
        }

        public static SolicitudDto ToDto(this SolicitudComercio solicitud)
        {
            return new SolicitudDto
            {
                Id = solicitud.Id,
                NombreComercio = solicitud.NombreComercio,
                Rubro = solicitud.Rubro,
                Direccion = solicitud.Direccion,
                TelefonoComercio = solicitud.TelefonoComercio,
                NombreTitular = solicitud.NombreTitular,
                ApellidoTitular = solicitud.ApellidoTitular,
                TelefonoTitular = solicitud.TelefonoTitular,
                EmailTitular = solicitud.EmailTitular,
                Ciudad = solicitud.Ciudad,
                Mensaje = solicitud.Mensaje,
                PlanInteres = solicitud.PlanInteres?.ToString(),
                Estado = solicitud.Estado.ToString(),
                FechaSolicitud = solicitud.FechaSolicitud,
                FechaRevision = solicitud.FechaRevision,
                ObservacionAdmin = solicitud.ObservacionAdmin
            };
        }

        public static PagoDto ToDto(this Pago pago)
        {
            return new PagoDto
            {
                Id = pago.Id,
                Monto = pago.Monto,
                FechaPago = pago.FechaPago,
                Observacion = pago.Observacion,
                Plan = pago.Suscripcion?.Plan.ToString()
            };
        }
    }
}
