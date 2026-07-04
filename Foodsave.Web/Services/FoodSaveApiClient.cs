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

        public async Task<bool> RegistrarComercioAsync(
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
                var json = JsonSerializer.Serialize(payload);
                _logger.LogInformation("Enviando a FoodSave API: {Url}, body: {Body}",
                    _http.BaseAddress + "auth/register-business", json);

                var response = await _http.PostAsJsonAsync("/auth/register-business", payload);
                var body = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("FoodSave API respondió {Status}: {Body}",
                    (int)response.StatusCode, body);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("FoodSave API error {Status} para {Email}: {Body}",
                        (int)response.StatusCode, email, body);
                    return false;
                }

                using var doc = JsonDocument.Parse(body);
                var success = doc.RootElement.TryGetProperty("success", out var prop) && prop.GetBoolean();

                if (success)
                {
                    _logger.LogInformation("Cuenta creada en FoodSave: {Email}", email);
                    return true;
                }

                var errorMsg = doc.RootElement.TryGetProperty("error", out var err) ? err.GetString() : "sin detalle";
                _logger.LogWarning("FoodSave API rechazó {Email}: {Error}", email, errorMsg);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar a FoodSave API para {Email}", email);
                return false;
            }
        }
    }
}
