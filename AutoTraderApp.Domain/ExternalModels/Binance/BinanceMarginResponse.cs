using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.ExternalModels.Binance
{
    public class BinanceMarginResponse
    {
        [JsonProperty("userAssets")]
        public List<BinanceMarginAsset> Assets { get; set; }
    }

    public class BinanceMarginAsset
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
