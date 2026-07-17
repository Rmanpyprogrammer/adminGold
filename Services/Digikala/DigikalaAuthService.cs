using System.Text;
using System.Text.Json;

using Core.API.Configurations;
using Core.API.Services.Cache;

using Microsoft.Extensions.Options;
using Polly.CircuitBreaker;

namespace Core.API.Services.Digikala;

public class DigikalaAuthService
{
    private readonly HttpClient _http;
    private readonly RedisService _redis;
    private readonly DigikalaSettings _settings;
    private readonly ILogger<DigikalaAuthService> _logger;

    public DigikalaAuthService(
        HttpClient http,
        RedisService redis,
        IOptions<DigikalaSettings> options,
        ILogger<DigikalaAuthService>
            logger
    )
    {
        _http = http;
        _redis = redis;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string>
        GetValidTokenAsync(int panel)
    {
        var cached =
            await _redis
                .GetDigikalaTokenAsync(panel);

        if (!string
            .IsNullOrWhiteSpace(cached))
        {
            return cached;
        }

        return await RefreshTokenAsync(panel);
    }

    public async Task<string>
        RefreshTokenAsync(int panel)
    {
        var body = new
                {
                    access_token =
                        _settings.AccessToken,

                    refresh_token =
                        _settings.RefreshToken
                };
                
        switch (panel)
        {
            case 1:
                body = new
                {
                    access_token =
                        _settings.AccessToken,

                    refresh_token =
                        _settings.RefreshToken
                };
                break;
            case 2:
                body = new
                {
                    access_token =
                        _settings.AccessToken2,

                    refresh_token =
                        _settings.RefreshToken2
                };
                break;
        }


        var json =
            JsonSerializer.Serialize(body);

        var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

        var response =
            await _http.PostAsync(
                _settings.RefreshUrl,
                content
            );

        var responseString =
            await response.Content
                .ReadAsStringAsync();

        _logger.LogInformation(
            "Digikala refresh response: {Response}",
            responseString
        );

        response.EnsureSuccessStatusCode();

        using var document =
            JsonDocument.Parse(
                responseString
            );

        var root =
            document.RootElement;

        if (!root.TryGetProperty(
                "data",
                out var data))
        {
            throw new Exception(
                "Digikala response has no data"
            );
        }

        if (!data.TryGetProperty(
                "access_token",
                out var accessToken))
        {
            throw new Exception(
                "access_token not found"
            );
        }

        var token =
            accessToken.GetString();

        if (string.IsNullOrWhiteSpace(
                token))
        {
            throw new Exception(
                "empty access token"
            );
        }

        await _redis
            .SetDigikalaTokenAsync(
                token,
                TimeSpan.FromMinutes(55),
                panel
            );

        _logger.LogInformation(
            "Digikala token stored in redis"
        );

        return token;
    }
}