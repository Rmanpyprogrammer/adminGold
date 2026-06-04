using Core.API.DTOs;

namespace Core.API.Services.Digikala;

public class DigikalaSyncService
{
    private readonly DigikalaClient _client;

    public DigikalaSyncService(
        DigikalaClient client
    )
    {
        _client = client;
    }

    public async Task<List<ProductSyncDto>>
        SyncPackageAsync(long packageId)
    {
        var package =
            await _client
                .GetPackageAsync(packageId);

        var products =
            package
            .GetProperty("data")
            .GetProperty("items")
            .GetProperty("package_products");

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
                    .GetVariantAsync(dkpc);

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

        return result;
    }
}