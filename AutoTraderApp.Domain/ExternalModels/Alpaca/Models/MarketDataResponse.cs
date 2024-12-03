using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models
{
    public class MarketDataResponse
    {
        public string Symbol { get; set; }
        public decimal LastPrice { get; set; }
        public decimal ChangePercent { get; set; }
        public int Volume { get; set; }
    }
}
