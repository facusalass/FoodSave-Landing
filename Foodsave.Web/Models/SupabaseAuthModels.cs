using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foodsave.Web.Models
{
    public sealed class SupabaseSession
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "bearer";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("user")]
        public SupabaseUser User { get; set; } = new();
    }

    public sealed class SupabaseUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("app_metadata")]
        public Dictionary<string, JsonElement> AppMetadata { get; set; } = new();

        [JsonPropertyName("user_metadata")]
        public Dictionary<string, JsonElement> UserMetadata { get; set; } = new();

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
