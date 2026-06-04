namespace Core.API.Configurations;

public class DigikalaSettings
{
    public string BaseUrl { get; set; } = string.Empty;

    public string RefreshUrl { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public int RefreshBeforeMinutes { get; set; }
}