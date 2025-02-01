using Newtonsoft.Json;

namespace AutoTraderApp.Domain.ExternalModels.Binance
{
    public class BinanceFilter
    {
        [JsonProperty("filterType")]
        public string FilterType { get; set; }

        [JsonProperty("minQty")]
        public string MinQty { get; set; }

        [JsonProperty("stepSize")]
        public string StepSize { get; set; }

        [JsonProperty("minNotional")]
        public string MinNotional { get; set; }

        [JsonProperty("minPrice")]
        public string MinPrice { get; set; }

        [JsonProperty("maxPrice")]
        public string MaxPrice { get; set; }

        [JsonProperty("tickSize")]
        public string TickSize { get; set; }

        // **Eksik alanlar eklendi**
        [JsonProperty("bidMultiplierUp")]
        public string BidMultiplierUp { get; set; }

        [JsonProperty("bidMultiplierDown")]
        public string BidMultiplierDown { get; set; }

        [JsonProperty("askMultiplierUp")]
        public string AskMultiplierUp { get; set; }

        [JsonProperty("askMultiplierDown")]
        public string AskMultiplierDown { get; set; }
    }
}
