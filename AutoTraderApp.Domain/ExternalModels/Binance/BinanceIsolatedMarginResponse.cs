using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.ExternalModels.Binance
{
    public class BinanceIsolatedMarginResponse
    {
        [JsonProperty("assets")]
        public List<BinanceIsolatedMarginAsset> Assets { get; set; }
    }

    public class BinanceIsolatedMarginAsset
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("baseAsset")]
        public BinanceIsolatedAsset BaseAsset { get; set; }

        [JsonProperty("quoteAsset")]
        public BinanceIsolatedAsset QuoteAsset { get; set; }
    }

    public class BinanceIsolatedAsset
    {
        [JsonProperty("asset")]
        public string Asset { get; set; }

        [JsonProperty("free")]
        public decimal Free { get; set; }

        [JsonProperty("locked")]
        public decimal Locked { get; set; }

        [JsonProperty("borrowed")]
        public decimal Borrowed { get; set; }

        [JsonProperty("interest")]
        public decimal Interest { get; set; }

        [JsonProperty("netAsset")]
        public decimal NetAsset { get; set; }
    }

}
