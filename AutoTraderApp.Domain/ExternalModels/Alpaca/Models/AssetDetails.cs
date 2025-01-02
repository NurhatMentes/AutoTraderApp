namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

public class AssetDetails
{
    public string Symbol { get; set; }
    public bool Tradable { get; set; }
    public bool Shortable { get; set; }
}

