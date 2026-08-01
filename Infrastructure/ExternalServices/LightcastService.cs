using Application.DTOs;
using Application.Interfaces.Service;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Infrastructure.ExternalServices
{
    public class LightcastService : ILightcastService
    {
        private readonly HttpClient _httpClient;
        private readonly LightcastSettings _settings;
        private readonly IMemoryCache _cache;
        private const string TokenCacheKey = "LightcastAccessToken";

        public LightcastService(HttpClient httpClient, IOptions<LightcastSettings> settings, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _cache = cache;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (_cache.TryGetValue(TokenCacheKey, out string? cachedToken))
            {
                return cachedToken!;
            }

            if (string.IsNullOrWhiteSpace(_settings.ClientId) || 
                string.IsNullOrWhiteSpace(_settings.ClientSecret) || 
                _settings.ClientId == "YOUR_LIGHTCAST_CLIENT_ID" || 
                _settings.ClientSecret == "YOUR_LIGHTCAST_CLIENT_SECRET")
            {
                throw new InvalidOperationException("Lightcast API credentials are missing or unconfigured in appsettings.json. Please set Lightcast:ClientId and Lightcast:ClientSecret.");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://auth.emsicloud.com/connect/token");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _settings.ClientId),
                new KeyValuePair<string, string>("client_secret", _settings.ClientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "emsi_open")
            });
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<LightcastTokenResponse>();
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                throw new Exception("Failed to retrieve Lightcast access token.");

            // Cache the token slightly shorter than the actual expiration time
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 60));

            _cache.Set(TokenCacheKey, tokenResponse.AccessToken, cacheOptions);

            return tokenResponse.AccessToken;
        }

        public async Task<List<LightcastSkillDto>> GetSkillsAsync(int limit, string taxonomyVersion)
        {
            var token = await GetAccessTokenAsync();
            
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.lightcast.io/skills/versions/{taxonomyVersion}/skills?limit={limit}&fields=id,name,type");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var skillsResponse = await response.Content.ReadFromJsonAsync<LightcastSkillResponse>();
            return skillsResponse?.Data ?? new List<LightcastSkillDto>();
        }
    }
}
