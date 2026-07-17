using System.Text.Json;
using Core.API.DTOs;
using Core.API.Data;
using Core.API.Models;
namespace Core.API.Services.Digikala;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class DigikalaSyncService
{
    private readonly DigikalaClient _client;
    private readonly AppDbContext _db;
    public DigikalaSyncService(
        DigikalaClient client,
        AppDbContext db
    )
    {
        _client = client;
        _db = db;
    }

    public async Task<ProductSyncResDto>
        SyncPackageAsync(long packageId, int page, int panel)
    {
        var package =
            await _client
                .GetPackageAsync(packageId, page, panel);

        var products =
            package
            .GetProperty("data")
            .GetProperty("items")
            .GetProperty("package_products");

        var pager = 
            package
            .GetProperty("data")
            .GetProperty("pager");

        

        var result =
            new List<ProductSyncDto>();

        foreach (var item in
                 products.EnumerateArray())
        {
            var dkpc =
                item
                .GetProperty("dkpc")
                .GetInt64();

            var variant =
                await _client
                    .GetVariantAsync(dkpc, panel);

            var data =
                variant
                .GetProperty("data");

            var gold =
                data.GetProperty(
                    "gold_price_parameters"
                );

            result.Add(
                new ProductSyncDto
                {
                    Dkpc = dkpc,

                    Title =
                        data
                        .GetProperty("title")
                        .GetString() ?? "",

                    CashSellingPrice =
                        data
                        .GetProperty(
                            "cash_selling_price"
                        )
                        .GetInt64(),

                    LiveGoldPrice =
                        gold


                        .GetProperty(
                            "liveGoldPrice"
                        )
                        .GetInt64(),

                    SkuConfig =
                        data
                        .GetProperty(
                            "sku_config"
                        ),

                    ProductId =
                        data
                        .GetProperty(
                            "product_id"
                        )
                        .GetInt64(),

                    GoldProfit =
                        gold
                        .GetProperty(
                            "goldProfit"
                        )
                        .GetDecimal(),

                    GoldWage =
                        gold
                        .GetProperty(
                            "goldWage"
                        )
                        .GetDecimal(),

                    NoneGoldWage =
                        gold
                        .GetProperty(
                            "noneGoldWage"
                        )
                        .GetDecimal(),

                    NoneGoldCost =
                        gold
                        .GetProperty(
                            "noneGoldCost"
                        )
                        .GetDecimal()
                }
            );
        }
        return new ProductSyncResDto {
            Pager = pager,
            ProductList = result
        }
         ;
    }


    public async Task InvoiceSync(int panel)
    {
        var tehranOffset = TimeSpan.FromHours(3.5);

        var nowIran = DateTimeOffset.UtcNow.ToOffset(tehranOffset);

        var todayStart = new DateTimeOffset(
            nowIran.Year,
            nowIran.Month,
            nowIran.Day,
            0,
            0,
            0,
            tehranOffset
        );

        var start = todayStart.AddDays(-1);
        var end = todayStart;

        var strt_digi = start.ToString(
            "yyyy-MM-dd'T'HH:mm:ss.ffffffzzz",
            CultureInfo.InvariantCulture
        );

        var end_digi = end.ToString(
            "yyyy-MM-dd'T'HH:mm:ss.ffffffzzz",
            CultureInfo.InvariantCulture
        );

        var firstResponse = await _client.GetInvoicesList(strt_digi, end_digi, 1, panel);

        var pager = firstResponse
            .GetProperty("data")
            .GetProperty("pager");

        var totalPages = pager
            .GetProperty("total_pages")
            .GetInt32();

        var invoices = new List<Invoice>();
        var invoices2 = new List<Invoice2>();

        var invoiceTypes = new List<string>
        {
            "cash",
            "credit",
            "reverse_cash",
            "reverse_credit"
        };

        for (int pageIndex = 1; pageIndex <= totalPages; pageIndex++)
        {
            var response = pageIndex == 1
                ? firstResponse
                : await _client.GetInvoicesList(strt_digi, end_digi, pageIndex, panel);

            var items = response
                .GetProperty("data")
                .GetProperty("items");

            foreach (var item in items.EnumerateArray())
            {
                var digi_id = item
                    .GetProperty("id")
                    .GetInt64();

                bool exists;

                switch (panel)
                {
                    case 1:
                        exists = await _db.Invoices
                            .AnyAsync(x => x.DigikalaId == digi_id);
                        break;

                    case 2:
                        exists = await _db.Invoices2
                            .AnyAsync(x => x.DigikalaId == digi_id);
                        break;

                    default:
                        throw new Exception($"Invalid panel: {panel}");
                }

                if (exists)
                {
                    Console.WriteLine($"Invoice already exists. DigikalaId: {digi_id}");
                    continue;
                }

                var invoiceProductIds = new HashSet<long>();

                switch (panel)
                {
                    case 1:
                    {
                        var invoice = new Invoice
                        {
                            DigikalaId = digi_id,
                            Amount = item.GetProperty("invoice_total_amount").GetDouble()
                        };

                        foreach (var type in invoiceTypes)
                        {
                            var firstDetailResponse = await _client.GetInvoicesDet(1, digi_id, type, panel);

                            var pager_det = firstDetailResponse
                                .GetProperty("data")
                                .GetProperty("pager");

                            var detailTotalPages = pager_det
                                .GetProperty("total_pages")
                                .GetInt32();

                            for (int detailPage = 1; detailPage <= detailTotalPages; detailPage++)
                            {
                                var response_det = detailPage == 1
                                    ? firstDetailResponse
                                    : await _client.GetInvoicesDet(detailPage, digi_id, type, panel);

                                var items_detail = response_det
                                    .GetProperty("data")
                                    .GetProperty("items");

                                foreach (var items_det in items_detail.EnumerateArray())
                                {
                                    var invoiceProductId = items_det
                                        .GetProperty("id")
                                        .GetInt64();

                                    if (invoiceProductIds.Contains(invoiceProductId))
                                    {
                                        continue;
                                    }

                                    invoiceProductIds.Add(invoiceProductId);

                                    var productExists = await _db.InvoicesProduct
                                        .AnyAsync(x => x.DigikalaId == invoiceProductId);

                                    if (productExists)
                                    {
                                        Console.WriteLine($"Invoice product already exists. DigikalaId: {invoiceProductId}");
                                        continue;
                                    }

                                    var variantCode = items_det
                                        .GetProperty("variant_code")
                                        .GetString();

                                    int DKPC = int.TryParse(
                                        variantCode != null && variantCode.Length > 5
                                            ? variantCode.Substring(5)
                                            : null,
                                        out var dkpc
                                    ) ? dkpc : 0;

                                    var title = items_det
                                        .GetProperty("variant_title")
                                        .GetString() ?? " ";

                                    var match = Regex.Match(title, @"([\d.]+)\s*گرم");

                                    double weight = match.Success
                                        ? double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
                                        : 0;

                                    invoice.Products.Add(new InvoiceProduct
                                    {
                                        DigikalaId = invoiceProductId,
                                        DKPC = DKPC,
                                        desc = items_det.GetProperty("description").GetString(),
                                        Weight = weight,
                                        OrderId = items_det.GetProperty("order_id").GetInt64(),
                                        Serial = items_det.GetProperty("item_serial").GetString(),
                                        Title = title,
                                        PayMethod = type,
                                    });
                                }
                            }
                        }

                        invoices.Add(invoice);
                        break;
                    }

                    case 2:
                    {
                        var invoice2 = new Invoice2
                        {
                            DigikalaId = digi_id,
                            Amount = item.GetProperty("invoice_total_amount").GetDouble()
                        };

                        foreach (var type in invoiceTypes)
                        {
                            var firstDetailResponse = await _client.GetInvoicesDet(1, digi_id, type, panel);

                            var pager_det = firstDetailResponse
                                .GetProperty("data")
                                .GetProperty("pager");

                            var detailTotalPages = pager_det
                                .GetProperty("total_pages")
                                .GetInt32();

                            for (int detailPage = 1; detailPage <= detailTotalPages; detailPage++)
                            {
                                var response_det = detailPage == 1
                                    ? firstDetailResponse
                                    : await _client.GetInvoicesDet(detailPage, digi_id, type, panel);

                                var items_detail = response_det
                                    .GetProperty("data")
                                    .GetProperty("items");

                                foreach (var items_det in items_detail.EnumerateArray())
                                {
                                    var invoiceProductId = items_det
                                        .GetProperty("id")
                                        .GetInt64();

                                    if (invoiceProductIds.Contains(invoiceProductId))
                                    {
                                        continue;
                                    }

                                    invoiceProductIds.Add(invoiceProductId);

                                    var productExists = await _db.InvoicesProduct2
                                        .AnyAsync(x => x.DigikalaId == invoiceProductId);

                                    if (productExists)
                                    {
                                        Console.WriteLine($"Invoice product already exists. DigikalaId: {invoiceProductId}");
                                        continue;
                                    }

                                    var variantCode = items_det
                                        .GetProperty("variant_code")
                                        .GetString();

                                    int DKPC = int.TryParse(
                                        variantCode != null && variantCode.Length > 5
                                            ? variantCode.Substring(5)
                                            : null,
                                        out var dkpc
                                    ) ? dkpc : 0;

                                    var title = items_det
                                        .GetProperty("variant_title")
                                        .GetString() ?? " ";

                                    var match = Regex.Match(title, @"([\d.]+)\s*گرم");

                                    double weight = match.Success
                                        ? double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
                                        : 0;

                                    invoice2.Products.Add(new InvoiceProduct2
                                    {
                                        DigikalaId = invoiceProductId,
                                        DKPC = DKPC,
                                        desc = items_det.GetProperty("description").GetString(),
                                        Weight = weight,
                                        OrderId = items_det.GetProperty("order_id").GetInt64(),
                                        Serial = items_det.GetProperty("item_serial").GetString(),
                                        Title = title,
                                        PayMethod = type,
                                    });
                                }
                            }
                        }

                        invoices2.Add(invoice2);
                        break;
                    }

                    default:
                        throw new Exception($"Invalid panel: {panel}");
                }
            }
        }

        switch (panel)
        {
            case 1:
                _db.Invoices.AddRange(invoices);
                break;

            case 2:
                _db.Invoices2.AddRange(invoices2);
                break;

            default:
                throw new Exception($"Invalid panel: {panel}");
        }

        var savedCount = await _db.SaveChangesAsync();

        var totalInvoices = panel == 1
            ? invoices.Count
            : invoices2.Count;

        Console.WriteLine($"INVOICE SYNC DONE. Invoices: {totalInvoices}, Saved changes: {savedCount}");
    }
    public async Task PackagesSync(int panel)
    {
        var tehranOffset = TimeSpan.FromHours(3.5);

        var nowIran = DateTimeOffset.UtcNow.ToOffset(tehranOffset);

        var todayStart = new DateTimeOffset(
            nowIran.Year,
            nowIran.Month,
            nowIran.Day,
            0,
            0,
            0,
            tehranOffset
        );

        var start = todayStart.AddDays(-1);
        var end = todayStart;

        var strt_digi = start.ToString(
            "yyyy-MM-dd'T'HH:mm:ss.ffffffzzz",
            CultureInfo.InvariantCulture
        );

        var end_digi = end.ToString(
            "yyyy-MM-dd'T'HH:mm:ss.ffffffzzz",
            CultureInfo.InvariantCulture
        );

        var firstResponse = await _client.GetPackageList(strt_digi, end_digi, 1, panel);

        var pager = firstResponse
            .GetProperty("data")
            .GetProperty("pager");

        var totalPages = pager
            .GetProperty("total_pages")
            .GetInt32();

        var packages = new List<Packages>();
        var packages2 = new List<Packages2>();

        for (int pageIndex = 1; pageIndex <= totalPages; pageIndex++)
        {
            var response = pageIndex == 1
                ? firstResponse
                : await _client.GetPackageList(strt_digi, end_digi, pageIndex, panel);

            var items = response
                .GetProperty("data")
                .GetProperty("items");

            foreach (var item in items.EnumerateArray())
            {
                var digi_id = item
                    .GetProperty("package_id")
                    .GetInt64();

                bool exists;

                switch (panel)
                {
                    case 1:
                        exists = await _db.Packages
                            .AnyAsync(x => x.PackageId == digi_id);
                        break;

                    case 2:
                        exists = await _db.Packages2
                            .AnyAsync(x => x.PackageId == digi_id);
                        break;

                    default:
                        throw new Exception($"Invalid panel: {panel}");
                }

                if (exists)
                {
                    Console.WriteLine($"package already exists. DigikalaId: {digi_id}");
                    continue;
                }

                var PackageProductIds = new HashSet<string>();

                var firstDetailResponse = await _client.GetPackageAsync(digi_id, 1, panel);

                var pager_det = firstDetailResponse
                    .GetProperty("data")
                    .GetProperty("pager");

                var detailTotalPages = pager_det
                    .GetProperty("total_pages")
                    .GetInt32();

                for (int detailPage = 1; detailPage <= detailTotalPages; detailPage++)
                {
                    var response_det = detailPage == 1
                        ? firstDetailResponse
                        : await _client.GetPackageAsync(digi_id, detailPage, panel);

                    var items_detail = response_det
                        .GetProperty("data")
                        .GetProperty("items")
                        .GetProperty("package_products");

                    foreach (var items_det in items_detail.EnumerateArray())
                    {
                        var ProductArray = items_det.GetProperty("serials");

                        foreach (var serials in ProductArray.EnumerateArray())
                        {
                            var status = 0;

                            try
                            {
                                status = serials.GetProperty("status")
                                    .GetProperty("received")
                                    .GetString() == "دریافت شده" ? 1 : 0;
                            }
                            catch
                            {
                                Console.WriteLine("SERIAL WITHOUT STATUS:");
                                Console.WriteLine(serials.ToString());
                            }

                            var packageProductId = serials.GetProperty("serial").GetString();

                            if (string.IsNullOrWhiteSpace(packageProductId))
                            {
                                continue;
                            }

                            if (PackageProductIds.Contains(packageProductId))
                            {
                                continue;
                            }

                            PackageProductIds.Add(packageProductId);

                            bool productExists;

                            switch (panel)
                            {
                                case 1:
                                    productExists = await _db.Packages
                                        .AnyAsync(x => x.Serial == packageProductId);
                                    break;

                                case 2:
                                    productExists = await _db.Packages2
                                        .AnyAsync(x => x.Serial == packageProductId);
                                    break;

                                default:
                                    throw new Exception($"Invalid panel: {panel}");
                            }

                            if (productExists)
                            {
                                Console.WriteLine($"Invoice product already exists. DigikalaId: {packageProductId}");
                                continue;
                            }

                            switch (panel)
                            {
                                case 1:
                                    packages.Add(new Packages
                                    {
                                        Serial = packageProductId,
                                        PackageId = digi_id,
                                        RecievedAt = item.GetProperty("received_at")
                                            .GetDateTime()
                                            .ToUniversalTime(),
                                        Warehouse = item.GetProperty("warehouse")
                                            .GetProperty("title")
                                            .GetString(),
                                        dkpc = items_det.GetProperty("dkpc").GetInt64(),
                                        Status = status
                                    });

                                    break;

                                case 2:
                                    packages2.Add(new Packages2
                                    {
                                        Serial = packageProductId,
                                        PackageId = digi_id,
                                        RecievedAt = item.GetProperty("received_at")
                                            .GetDateTime()
                                            .ToUniversalTime(),
                                        Warehouse = item.GetProperty("warehouse")
                                            .GetProperty("title")
                                            .GetString(),
                                        dkpc = items_det.GetProperty("dkpc").GetInt64(),
                                        Status = status
                                    });

                                    break;

                                default:
                                    throw new Exception($"Invalid panel: {panel}");
                            }
                        }
                    }
                }
            }
        }

        switch (panel)
        {
            case 1:
                _db.Packages.AddRange(packages);
                break;

            case 2:
                _db.Packages2.AddRange(packages2);
                break;

            default:
                throw new Exception($"Invalid panel: {panel}");
        }

        var savedCount = await _db.SaveChangesAsync();

        var totalPackages = panel == 1
            ? packages.Count
            : packages2.Count;

        Console.WriteLine($"packages SYNC DONE. packages: {totalPackages}, Saved changes: {savedCount}");
    }
    
    
    public async Task<List<string>> 
    ReconciliationService(DateTime start,DateTime end, int panel)
    {
        start = start.Kind == DateTimeKind.Utc ? start : start.ToUniversalTime();
        end = end.Kind == DateTimeKind.Utc ? end : end.ToUniversalTime();
        List<string?> Packages;
        List<string?> invoicement;

        switch (panel)
        {
            case 1:
                Packages = await _db.Packages
                    .Where(x =>
                        x.RecievedAt > start &&
                        x.RecievedAt < end &&
                        x.Status == 1
                    )
                    .Select(x => x.Serial)
                    .ToListAsync();

                invoicement = await _db.InvoicesProduct
                    .Where(x =>
                        x.CreatedAt > start &&
                        x.CreatedAt < end
                    )
                    .Select(x => x.Serial)
                    .ToListAsync();

                break;

            case 2:
                Packages = await _db.Packages2
                    .Where(x =>
                        x.RecievedAt > start &&
                        x.RecievedAt < end &&
                        x.Status == 1
                    )
                    .Select(x => x.Serial)
                    .ToListAsync();

                invoicement = await _db.InvoicesProduct2
                    .Where(x =>
                        x.CreatedAt > start &&
                        x.CreatedAt < end
                    )
                    .Select(x => x.Serial)
                    .ToListAsync();

                break;

            default:
                throw new Exception($"Invalid panel: {panel}");
        }

        List<string> inventories = new List<string> ();
        var firstDetailResponse = await _client.GetInventories(1, panel);
        var pager_det = firstDetailResponse
            .GetProperty("data")
            .GetProperty("pager");
        var detailTotalPages = pager_det
            .GetProperty("total_pages")
            .GetInt32();
        for (int detailPage = 1; detailPage <= detailTotalPages; detailPage++)
        {
            var response_det = detailPage == 1
                ? firstDetailResponse
                : await _client.GetInventories(detailPage, panel);
            var items_detail = response_det
                .GetProperty("data")
                .GetProperty("items");


            foreach(var item in items_detail.EnumerateArray())
            {
                var dkpc = item.GetProperty("product_variant_id").GetInt64();
                var response_det2 = await _client.GetInventoriesDet(dkpc, panel);
                var items_detail2 = response_det2
                    .GetProperty("data")
                    .GetProperty("items"); 
                foreach(var item2 in items_detail2.EnumerateArray())
                {
                    string? serial = item2.GetProperty("item_serial").GetString();
                    if (!string.IsNullOrWhiteSpace(serial))
                    {
                        inventories.Add(serial);
                    }
                }
            }
        }
        var invoicementSet = new HashSet<string>(
            invoicement.Where(x => !string.IsNullOrWhiteSpace(x))
        );

        var inventoriesSet = new HashSet<string>(
            inventories.Where(x => !string.IsNullOrWhiteSpace(x))
        );

        var missingSerials = Packages
            .Where(serial =>
                !string.IsNullOrWhiteSpace(serial) &&
                !invoicementSet.Contains(serial) &&
                !inventoriesSet.Contains(serial)
            )
            .ToList();

        return missingSerials;

    }
}




