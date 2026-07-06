using System.Text.RegularExpressions;

namespace Foodsave.Web.Helpers
{
    public static partial class WhatsAppLinkHelper
    {
        private const string ContactMessage =
            "Hola, te contactamos desde FoodSave por tu solicitud para sumar tu comercio.";

        public static string? Create(
            string? telefonoTitular,
            string? telefonoComercio)
        {
            var phone = Normalize(telefonoTitular) ?? Normalize(telefonoComercio);
            if (phone is null)
            {
                return null;
            }

            return $"https://wa.me/{phone}?text={Uri.EscapeDataString(ContactMessage)}";
        }

        private static string? Normalize(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            var digits = NonDigitRegex().Replace(phone, string.Empty);

            if (digits.Length == 10)
            {
                digits = $"549{digits}";
            }

            if (digits.Length is < 11 or > 15 || !digits.StartsWith("54"))
            {
                return null;
            }

            return digits;
        }

        [GeneratedRegex(@"\D")]
        private static partial Regex NonDigitRegex();
    }
}
