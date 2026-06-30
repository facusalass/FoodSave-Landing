namespace Foodsave.Web.Helpers
{
    public static class TextHelper
    {
        public static string? NormalizarOpcional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
