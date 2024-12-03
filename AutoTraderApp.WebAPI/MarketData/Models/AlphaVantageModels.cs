using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoTraderApp.Infrastructure.MarketData.Models
{
    public class AlphaVantageResponse<T>
    {
        [JsonPropertyName("Meta Data")]
        public Dictionary<string, string> MetaData { get; set; }

        [JsonPropertyName("Time Series (Daily)")]
        public Dictionary<string, T> TimeSeries { get; set; }
    }
}
