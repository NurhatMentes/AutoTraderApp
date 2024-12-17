using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoTraderApp.Infrastructure.Services.Alpaca
{
    public class AlpacaService : IAlpacaService
    {
        private readonly HttpClient _httpClient;
        private readonly AlpacaSettings _alpacaSettings;

        public AlpacaService(HttpClient httpClient, IOptions<AlpacaSettings> alpacaSettings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (_httpClient.BaseAddress == null)
            {
                _alpacaSettings = alpacaSettings.Value;
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.BaseAddress = new Uri(_alpacaSettings.IsPaper ? "https://paper-api.alpaca.markets/" : "https://api.alpaca.markets/");
                _httpClient.DefaultRequestHeaders.Add("APCA-API-KEY-ID", _alpacaSettings.ApiKey);
                _httpClient.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", _alpacaSettings.ApiSecret);
            }
        }

        public async Task<AccountInfo> GetAccountInfoAsync(string apiKey, string apiSecret, bool isPaper)
        {
            var response = await _httpClient.GetAsync("v2/account");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Alpaca API hatası: {response.StatusCode} - {responseContent}");
            }

            try
            {
                var accountInfo = JsonSerializer.Deserialize<AccountInfo>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals
                });

                if (accountInfo == null)
                {
                    throw new Exception("Hesap bilgileri boş geldi.");
                }

                return accountInfo;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Hesap bilgileri alınırken bir hata oluştu: {ex.Message}");
            }
        }


        public async Task<OrderResponse> PlaceOrderAsync(OrderRequest orderRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("v2/orders", orderRequest);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Alpaca API hatası: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(); 
            Console.WriteLine($"Alpaca Yanıtı: {responseContent}");

            try
            {
                var orderResponse = JsonSerializer.Deserialize<OrderResponse>(responseContent); 
                if (orderResponse == null)
                    throw new Exception("Alpaca API yanıtı boş döndü.");

                return orderResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"JSON deserialize sırasında bir hata oluştu: {ex.Message} - Yanıt: {responseContent}");
            }

        }

        public async Task<List<Portfolio>> GetPortfolioAsync()
        {
            var response = await _httpClient.GetAsync("v2/positions");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Portföy bilgisi alınamadı: {response.StatusCode} - {errorContent}");
            }

            var portfolio = await response.Content.ReadFromJsonAsync<List<PortfolioResponse>>();

            if (portfolio == null)
                return new List<Portfolio>();

            return portfolio.Select(position => new Portfolio
            {
                Symbol = position.Symbol,
                Quantity = position.Qty,
                MarketValue = position.MarketValue,
                CostBasis = position.CostBasis,
                UnrealizedPnL = position.UnrealizedPnL,
                CurrentPrice = position.CurrentPrice,
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow     
            }).ToList();
        }

        public async Task<List<PositionResponse>> GetPositionsAsync()
        {
            var response = await _httpClient.GetAsync("/v2/positions");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Alpaca API hatası: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PositionResponse>>(responseContent);
        }

        public async Task<IResult> ClosePositionAsync(string symbol, decimal quantity)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/v2/positions/{symbol}/close",
                    new StringContent(JsonSerializer.Serialize(new { qty = quantity }), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ErrorResult($"Alpaca API hatası: {response.StatusCode} - {errorContent}");
                }

                return new SuccessResult("Pozisyon Alpaca üzerinde kapatıldı.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Alpaca API ile iletişim sırasında bir hata oluştu: {ex.Message}");
            }
        }



        public async Task<OrderResponse> CancelOrderAsync(string orderId)
        {
            var response = await _httpClient.DeleteAsync($"v2/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderResponse>();
        }

        public async Task<MarketDataResponse> GetMarketDataAsync(string symbol)
        {
            var response = await _httpClient.GetAsync($"/v2/stocks/{symbol}/trades/latest?feed=sip");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Alpaca API hatası: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<MarketDataResponse>();
        }

        public async Task<List<MarketDataResponse>> GetAllMarketDataAsync(int page = 1, int pageSize = 30)
        {
            var assetsResponse = await _httpClient.GetAsync("/v2/assets");
            if (!assetsResponse.IsSuccessStatusCode)
            {
                var errorContent = await assetsResponse.Content.ReadAsStringAsync();
                throw new Exception($"Alpaca API hatası: {errorContent}");
            }

            var assets = await assetsResponse.Content.ReadFromJsonAsync<List<AssetResponse>>();

            var marketDataResponses = new List<MarketDataResponse>();
            foreach (var asset in assets)
            {
                var marketDataResponse = await GetMarketDataAsync(asset.Symbol);
                marketDataResponses.Add(marketDataResponse);
            }

            return marketDataResponses;
        }


        public async Task<OrderResponse[]> GetAllOrdersAsync()
        {
            var response = await _httpClient.GetAsync("v2/orders");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderResponse[]>();
        }

        public async Task<OrderResponse> GetOrderByIdAsync(string orderId)
        {
            var response = await _httpClient.GetAsync($"v2/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderResponse>();
        }

        public async Task<Position[]> GetOpenPositionsAsync()
        {
            var response = await _httpClient.GetAsync("v2/positions");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API isteği başarısız oldu: {response.StatusCode} - {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<Position[]>();
        }

        public async Task<LastPrice> GetLastPriceAsync(string symbol)
        {
            var response = await _httpClient.GetAsync($"v2/stocks/{symbol}/last");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LastPrice>();
        }
    }
}
