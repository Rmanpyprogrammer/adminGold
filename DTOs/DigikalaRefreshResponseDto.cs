using System.Text.Json.Serialization;

namespace Core.API.DTOs;

public class DigikalaRefreshResponseDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
}