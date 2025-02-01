using Newtonsoft.Json;

namespace AutoTraderApp.Domain.ExternalModels.Binance
{
    public class BinanceSymbolInfo
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("filters")]
        public List<BinanceFilter> Filters { get; set; }
    }
}
