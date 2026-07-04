using System.Net.Http.Json;
using System.Text.Json;

namespace Foodsave.Web.Services
{
    public class FoodSaveApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<FoodSaveApiClient> _logger;

        public FoodSaveApiClient(HttpClient http, ILogger<FoodSaveApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<string?> RegistrarComercioAsync(
            string email,
            string password,
            string businessName,
            string? businessAddress,
            string businessCategory,
            string? businessCity,
            string ownerName,
            string? ownerPhone)
        {
            var payload = new
            {
                email,
                password,
                businessName,
                businessAddress = businessAddress ?? "",
                businessCategory,
                businessCity = businessCity ?? "No especificada",
                ownerName,
                ownerPhone = ownerPhone ?? ""
            };

            try
            {
                var response = await _http.PostAsJsonAsync("/auth/register-business", payload);
                var body = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("FoodSave API respondió {Status}: {Body}",
                    (int)response.StatusCode, body);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("FoodSave API error {Status} para {Email}", (int)response.StatusCode, email);
                    return null;
                }

                using var doc = JsonDocument.Parse(body);
                var success = doc.RootElement.TryGetProperty("success", out var prop) && prop.GetBoolean();

                if (!success)
                {
                    var errorMsg = doc.RootElement.TryGetProperty("error", out var err) ? err.GetString() : "sin detalle";
                    _logger.LogWarning("FoodSave API rechazó {Email}: {Error}", email, errorMsg);
                    return null;
                }

                var businessId = doc.RootElement
                    .GetProperty("data")
                    .GetProperty("user")
                    .GetProperty("businessId")
                    .GetString();

                _logger.LogInformation("Cuenta creada en FoodSave: {Email}, businessId={BusinessId}", email, businessId);
                return businessId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar a FoodSave API para {Email}", email);
                return null;
            }
        }

        public async Task<bool> ToggleActiveAsync(string businessId, bool isActive)
        {
            var payload = new { businessId, isActive };

            try
            {
                _logger.LogInformation("Toggle active FoodSave: {BusinessId}, isActive={Active}", businessId, isActive);

                var response = await _http.PatchAsJsonAsync(
                    "/auth/register-business/toggle-active", payload);
                var body = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("FoodSave toggle-active respondió {Status}: {Body}",
                    (int)response.StatusCode, body);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar toggle-active para {BusinessId}", businessId);
                return false;
            }
        }
    }
}
