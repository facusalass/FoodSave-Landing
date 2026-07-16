using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Foodsave.Web.Models;

namespace Foodsave.Web.Services
{
    public sealed class SupabaseAuthClient
    {
        private readonly HttpClient _httpClient;

        public SupabaseAuthClient(HttpClient httpClient, IConfiguration configuration)
        {
            var url = configuration["Supabase:Url"]
                ?? throw new InvalidOperationException(
                    "Supabase:Url no está configurado.");
            var publishableKey = configuration["Supabase:PublishableKey"]
                ?? throw new InvalidOperationException(
                    "Supabase:PublishableKey no está configurado.");

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(url.TrimEnd('/') + "/");
            _httpClient.DefaultRequestHeaders.Add("apikey", publishableKey);
        }

        public async Task<SupabaseSession?> SignInAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "auth/v1/token?grant_type=password",
                new { email, password },
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.BadRequest or
                HttpStatusCode.Unauthorized)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SupabaseSession>(
                cancellationToken: cancellationToken);
        }

        public async Task SignOutAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "auth/v1/logout");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
