using System.Text.Json;
using Microsoft.JSInterop.Implementation;

public class ProductSyncDto
{
    public long Dkpc { get; set; }

    public string Title { get; set; }
    public long CashSellingPrice { get; set; }
    public long LiveGoldPrice { get; set; }

    public JsonElement SkuConfig { get; set; }

    public long ProductId { get; set; }

    public decimal GoldProfit { get; set; }
    public decimal GoldWage { get; set; }
    public decimal NoneGoldWage { get; set; }
    public decimal NoneGoldCost { get; set; }
}

public class ProductSyncResDto
{
    public JsonElement Pager { get; set; }
    public List<ProductSyncDto> ProductList { get; set; }

}