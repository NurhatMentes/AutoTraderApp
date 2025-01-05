using AutoTraderApp.Domain.ExternalModels.Polygon;
using AutoTraderApp.Infrastructure.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Infrastructure.Services.Polygon
{
    public class PolygonService :IPolygonService
    {
        private readonly PolygonSettings _settings;
        private readonly HttpClient _httpClient;

        public PolygonService(IOptions<PolygonSettings> options, HttpClient httpClient)
        {
            _settings = options.Value;
            _httpClient = httpClient;
        }

        public async Task<decimal> GetStockPriceAsync(string symbol)
        {
            var baseUrl = "https://api.polygon.io";
            var url = $"{baseUrl}/v2/aggs/ticker/{symbol}/prev?apiKey=K2J8OWX43ACpPVoc6WD2AomhgtrPI9Un";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            // Deserialize JSON response
            var polygonResponse = JsonConvert.DeserializeObject<PolygonResponse>(content);

            // Get the close price from the results array
            return polygonResponse?.Results?.FirstOrDefault()?.Close ?? 0;
        }

    }
}
