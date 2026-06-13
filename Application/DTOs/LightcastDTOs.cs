using System.Text.Json.Serialization;

namespace Application.DTOs
{
    public class LightcastTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    public class LightcastSkillResponse
    {
        [JsonPropertyName("data")]
        public List<LightcastSkillDto> Data { get; set; } = new List<LightcastSkillDto>();
    }

    public class LightcastSkillDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public LightcastSkillTypeDto? Type { get; set; }
    }

    public class LightcastSkillTypeDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
