using Newtonsoft.Json;

namespace AutoTraderApp.Domain.ExternalModels.Binance
{
    public class BinancePriceResponse
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("price")]
        public decimal Price { get; set; }
    }
}
