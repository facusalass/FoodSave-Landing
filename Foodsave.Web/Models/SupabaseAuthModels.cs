using System.Text.Json;
using System.Text.Json.Serialization;

// Modelos que mapean la respuesta JSON de Supabase Auth (/auth/v1/token).
// Los nombres con snake_case vienen de Supabase; JsonPropertyName los mapea a PascalCase de C#.
namespace Foodsave.Web.Models
{
    // Lo que devuelve Supabase al hacer login exitoso.
    public sealed class SupabaseSession
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;  // El JWT que el cliente debe enviar como Bearer.

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;  // Para obtener un nuevo token cuando expire.

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "bearer";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }  // Segundos hasta que expire el access_token.

        [JsonPropertyName("user")]
        public SupabaseUser User { get; set; } = new();
    }

    // Datos del usuario autenticado que vienen dentro de la sesión.
    public sealed class SupabaseUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;  // UUID de Supabase Auth.

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        // Metadatos que vienen del JWT/ Supabase, no de nuestra BD.
        [JsonPropertyName("app_metadata")]
        public Dictionary<string, JsonElement> AppMetadata { get; set; } = new();  // role, etc.

        [JsonPropertyName("user_metadata")]
        public Dictionary<string, JsonElement> UserMetadata { get; set; } = new();  // name, etc.

        // Helper para extraer strings de los diccionarios de metadata.
        public string GetMetadataString(
            Dictionary<string, JsonElement> metadata,
            string key)
        {
            return metadata.TryGetValue(key, out var value) &&
                   value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }
    }
}
