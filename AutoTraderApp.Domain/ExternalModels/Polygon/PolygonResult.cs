using Newtonsoft.Json;

namespace AutoTraderApp.Domain.ExternalModels.Polygon
{
    public class PolygonResult
    {
        [JsonProperty("o")]
        public decimal Open { get; set; }

        [JsonProperty("c")]
        public decimal Close { get; set; }

        [JsonProperty("h")]
        public decimal High { get; set; }

        [JsonProperty("l")]
        public decimal Low { get; set; }
    }
}
