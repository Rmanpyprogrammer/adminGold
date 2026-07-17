using System.Text.Json;
using Microsoft.JSInterop.Implementation;
using System.Text.Json.Serialization;

namespace Core.API.DTOs;


public class AdminReportDtoRequest
{
    [JsonPropertyName("start")]
    public DateTime Start { get; set; }

    [JsonPropertyName("end")]
    public DateTime End { get; set; }

    [JsonPropertyName("admin_id")]
    public int AdminId { get; set; }
}


public class AdminReportDtoResponse
{
    public double Paid {get; set; }
    public double Weight {get; set; }
}
