using System.Net.Http.Headers;
using System.Text.Json;

namespace Core.API.Services.Digikala;

public class DigikalaClient
{
    private readonly HttpClient _http;
    private readonly DigikalaAuthService _auth;

    public DigikalaClient(
        HttpClient http,
        DigikalaAuthService auth
    )
    {
        _http = http;
        _auth = auth;
    }

    public async Task<JsonElement>
        GetPackageAsync(long packageId)
    {
        var token =
            await _auth.GetValidTokenAsync();

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token
            );

        var response =
            await _http.GetAsync(
                $"https://seller.digikala.com/open-api/v1/packages/{packageId}"
            );

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content
                .ReadAsStringAsync();

        return JsonSerializer
            .Deserialize<JsonElement>(json);
    }

    public async Task<JsonElement>
        GetVariantAsync(long dkpc)
    {
        var token =
            await _auth.GetValidTokenAsync();

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token
            );

        var response =
            await _http.GetAsync(
                $"https://seller.digikala.com/open-api/v1/variants/{dkpc}"
            );

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content
                .ReadAsStringAsync();

        return JsonSerializer
            .Deserialize<JsonElement>(json);
    }
}