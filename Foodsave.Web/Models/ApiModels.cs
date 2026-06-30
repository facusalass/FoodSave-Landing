using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Foodsave.Web.Models
{
    public static class ApiError
    {
        public static object NotFound(string message) =>
            new { error = message };

        public static object BadRequest(string message) =>
            new { error = message };

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
