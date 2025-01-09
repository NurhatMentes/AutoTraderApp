using Newtonsoft.Json;

namespace AutoTraderApp.Domain.ExternalModels.Polygon
{
    public class PolygonResponse
    {
        [JsonProperty("ticker")]
        public string Ticker { get; set; }

        [JsonProperty("results")]
        public List<PolygonResult> Results { get; set; }
    }
}
