using Foodsave.Web.Models;

namespace Foodsave.Web.Helpers
{
    public static class PlanHelper
    {
        private static readonly string[] PlanesPermitidos = ["Estandar", "Pro"];

        public static string? NormalizarPlan(string? plan)
        {
            return PlanesPermitidos.FirstOrDefault(
                permitido => permitido.Equals(
                    plan?.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        public static PlanSuscripcion? ParsePlan(string? plan)
        {
            var normalizado = NormalizarPlan(plan);
            return normalizado is not null
                ? Enum.Parse<PlanSuscripcion>(normalizado)
                : null;
        }
    }
}
