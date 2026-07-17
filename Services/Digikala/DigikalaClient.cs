using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
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
        GetPackageAsync(long packageId, int page,int panel)
    {
        var token =
            await _auth.GetValidTokenAsync(panel);

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token
            );

        var response =
            await _http.GetAsync(
                $"https://seller.digikala.com/open-api/v1/packages/{packageId}?page={page}&size=50"
            );

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content
                .ReadAsStringAsync();

        return JsonSerializer
            .Deserialize<JsonElement>(json);
    }

    public async Task<JsonElement>
        GetVariantAsync(long dkpc, int panel)
    {
        var token =
            await _auth.GetValidTokenAsync(panel);

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

    public async Task<JsonElement>
        GetPackageList(string start, string end, int page, int panel)
    {        
        var token =
            await _auth.GetValidTokenAsync(panel);
        
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token
            );

        var startEncoded = Uri.EscapeDataString(start);
        var endEncoded = Uri.EscapeDataString(end);

        var url =
            "https://seller.digikala.com/open-api/v1/packages" +
            $"?page={page}" +
            "&size=50" +
            $"&search[package_received_at_from]={startEncoded}" +
            $"&search[package_received_at_to]={endEncoded}";

        Console.WriteLine("DIGIKALA packages URL:");
        Console.WriteLine(url);
    
        var response = await _http.GetAsync(url);
        var json = await response.Content
                        .ReadAsStringAsync();    
        if (!response.IsSuccessStatusCode)
        {

            Console.WriteLine("DIGIKALA ERROR BODY:");
            Console.WriteLine(json);
    
            throw new HttpRequestException(
                $"Digikala packages failed. Status: {(int)response.StatusCode}, Body: {json}"
            );
        }

        return JsonSerializer.
        Deserialize<JsonElement>(json);
        
    }

    public async Task<JsonElement>
    GetInventories(int page, int panel)
    {
        var token =
            await _auth.GetValidTokenAsync(panel);

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token
            );

        var response =
            await _http.GetAsync(
                $"https://seller.digikala.com/open-api/v1/inventories?page={page}&size=50&sort=id&order=asc&search[stock_status]=has_warehouse_stock"
            );

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content
                .ReadAsStringAsync();

        return JsonSerializer
            .Deserialize<JsonElement>(json);
    }

    public async Task<JsonElement>
    GetInventoriesDet(long dkpc,int panel)
    {
        var token =
            await _auth.GetValidTokenAsync(panel);

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token
            );

        var url =
                $"https://seller.digikala.com/open-api/v1/inventories/{dkpc}";

        const int maxRetries = 30;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var response = await _http.GetAsync(url);

            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<JsonElement>(json);
            }

            if ((int)response.StatusCode == 429)
            {
                Console.WriteLine(
                    $"DIGIKALA 429. Wait 5 seconds. Attempt {attempt}/{maxRetries}"
                );

                await Task.Delay(TimeSpan.FromSeconds(5 * attempt));

                continue;
            }

            Console.WriteLine("DIGIKALA ERROR BODY:");
            Console.WriteLine(json);

            throw new HttpRequestException(
                $"Digikala inventories-det failed. Status: {(int)response.StatusCode}, Body: {json}"
            );
        }
        throw new HttpRequestException(
            $"Digikala inventories-det failed after {maxRetries} retries because of 429 Too Many Requests"
        );
    } 

    public async Task<JsonElement> GetInvoicesList(string start, string end, int page, int panel)
    {
        var token = await _auth.GetValidTokenAsync(panel);
    
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    
        var startEncoded = Uri.EscapeDataString(start);
        var endEncoded = Uri.EscapeDataString(end);

        var url =
            "https://seller.digikala.com/open-api/v1/invoices" +
            $"?page={page}" +
            "&size=50" +
            "&sort=id" +
            "&order=desc" +
            $"&search[invoice_start_date]={startEncoded}" +
            $"&search[invoice_end_date]={endEncoded}";
        Console.WriteLine("DIGIKALA INVOICE URL:");
        Console.WriteLine(url);
    
        var response = await _http.GetAsync(url);
    
        var json = await response.Content.ReadAsStringAsync();
    
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("DIGIKALA ERROR BODY:");
            Console.WriteLine(json);
    
            throw new HttpRequestException(
                $"Digikala invoices failed. Status: {(int)response.StatusCode}, Body: {json}"
            );
        }
    
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    public async Task<JsonElement> GetInvoicesDet(int page, long invoice_id, string type, int panel)
    {
        var token = await _auth.GetValidTokenAsync(panel);

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token
            );

        int type_id;

        switch (type)
        {
            case "cash":
                type_id = 60;
                break;

            case "credit":
                type_id = 120;
                break;

            case "reverse_cash":
                type_id = 61;
                break;

            case "reverse_credit":
                type_id = 121;
                break;

            default:
                type_id = 60;
                break;
        }

        var url =
            $"https://seller.digikala.com/open-api/v1/invoices/{invoice_id}/items/{type_id}/category_based"+
            $"?page={page}" +
            "&size=50" ;

        const int maxRetries = 100;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var response = await _http.GetAsync(url);

            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<JsonElement>(json);
            }

            if ((int)response.StatusCode == 429)
            {
                Console.WriteLine(
                    $"DIGIKALA 429. Wait 5 seconds. Attempt {attempt}/{maxRetries}"
                );

                await Task.Delay(TimeSpan.FromSeconds(5 * attempt));

                continue;
            }

            Console.WriteLine("DIGIKALA ERROR BODY:");
            Console.WriteLine(json);

            throw new HttpRequestException(
                $"Digikala invoices-det failed. Status: {(int)response.StatusCode}, Body: {json}"
            );
        }

        throw new HttpRequestException(
            $"Digikala invoices-det failed after {maxRetries} retries because of 429 Too Many Requests"
        );
    }

}

        