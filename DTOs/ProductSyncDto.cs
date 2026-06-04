public class ProductSyncDto
{
    public long Dkpc { get; set; }

    public string Title { get; set; }
    public long CashSellingPrice { get; set; }
    public long LiveGoldPrice { get; set; }

    public object SkuConfig { get; set; }

    public long ProductId { get; set; }

    public decimal GoldProfit { get; set; }
    public decimal GoldWage { get; set; }
    public decimal NoneGoldWage { get; set; }
    public decimal NoneGoldCost { get; set; }
}