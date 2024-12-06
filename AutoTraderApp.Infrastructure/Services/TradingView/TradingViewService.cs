using AutoTraderApp.Infrastructure.Interfaces;
using System.Net.Http.Json;

namespace AutoTraderApp.c.Services.TradingView
{
    public class TradingViewService : ITradingViewService
    {
        private readonly HttpClient _httpClient;

        public TradingViewService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> UploadStrategyAsync(string strategyName, string pineScriptCode)
        {
            var payload = new
            {
                name = strategyName,
                script = pineScriptCode
            };

            var response = await _httpClient.PostAsJsonAsync("tradingview/strategies", payload);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteStrategyAsync(string strategyName)
        {
            var response = await _httpClient.DeleteAsync($"tradingview/strategies/{strategyName}");
            return response.IsSuccessStatusCode;
        }
    }
}
