using Microsoft.AspNetCore.Mvc.ModelBinding;

// Helper de errores e input models para la API REST.
namespace Foodsave.Web.Models
{
    // Errores con formato uniforme en toda la API.
    // Los controladores llaman: ApiError.NotFound("mensaje")
    public static class ApiError
    {
        public static object NotFound(string message) =>
            new { error = message };

        public static object BadRequest(string message) =>
            new { error = message };

        // Para errores de validación devuelve los detalles campo por campo.
        public static object Validation(ModelStateDictionary modelState)
        {
            return new
            {
                error = "Error de validación.",
                detalles = modelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value!.Errors.Select(x => x.ErrorMessage))
            };
        }
    }

    // Input models: lo que el cliente envía en el body de POST/PATCH.
    // Se bindean automáticamente desde JSON gracias a [FromBody].
    public class UpdateEstadoInput
    {
        public string Estado { get; set; } = string.Empty;
    }

    public class ReviewSolicitudInput
    {
        public string Estado { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }
}
