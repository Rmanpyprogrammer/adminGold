using System.Text.Json;
using Core.API.DTOs;
using Core.API.Data;
using Core.API.Models;
namespace Core.API.Services.Digikala;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class AdminService
{
    private readonly AppDbContext _db;
    public AdminService(

        AppDbContext db
    )
    {
        _db = db;
    }
    public async Task<JsonElement>
    CalculateReport(DateTime strt,
                    DateTime end,
                    int panel)
    {

        double weight;
        double paid;
        
        switch (panel)
        {
            case 1:
                weight = await _db.InvoicesProduct
                    .Where(x =>
                        x.CreatedAt > strt &&
                        x.CreatedAt < end
                    )
                    .SumAsync(x => x.Weight);

                paid = await _db.Invoices
                    .Where(x =>
                        x.CreatedAt > strt &&
                        x.CreatedAt < end
                    )
                    .SumAsync(x => x.Amount);

                break;

            case 2:
                weight = await _db.InvoicesProduct2
                    .Where(x =>
                        x.CreatedAt > strt &&
                        x.CreatedAt < end
                    )
                    .SumAsync(x => x.Weight);

                paid = await _db.Invoices2
                    .Where(x =>
                        x.CreatedAt > strt &&
                        x.CreatedAt < end
                    )
                    .SumAsync(x => x.Amount);

                break;

            default:
                throw new Exception($"Invalid panel: {panel}");
        }

        var response = new AdminReportDtoResponse
        {
            Paid = paid,
            Weight = weight
        };

        var json = JsonSerializer.SerializeToElement(response);
        
        return json;
    }                   
}