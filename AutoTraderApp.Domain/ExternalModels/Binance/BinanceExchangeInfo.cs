using Newtonsoft.Json;

namespace AutoTraderApp.Domain.ExternalModels.Binance
{
    public class BinanceExchangeInfo
    {
        [JsonProperty("symbols")]
        public List<BinanceSymbolInfo> Symbols { get; set; }
    }
}
