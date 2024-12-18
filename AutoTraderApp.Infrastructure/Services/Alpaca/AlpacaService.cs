using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace AutoTraderApp.Infrastructure.Services.Alpaca
{
    public class AlpacaService : IAlpacaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;

        private readonly ConcurrentDictionary<Guid, HttpClient> _httpClientCache = new();

        public AlpacaService(
            IHttpClientFactory httpClientFactory,
            IBaseRepository<BrokerAccount> brokerAccountRepository)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _brokerAccountRepository = brokerAccountRepository ?? throw new ArgumentNullException(nameof(brokerAccountRepository));
        }

        private async Task<HttpClient> ConfigureHttpClientAsync(Guid brokerAccountId)
        {
            if (_httpClientCache.TryGetValue(brokerAccountId, out var existingClient))
            {
                return existingClient;
            }

            var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == brokerAccountId);
            if (brokerAccount == null)
            {
                throw new Exception("Broker hesabı bulunamadı.");
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(brokerAccount.IsPaper ? "https://paper-api.alpaca.markets/" : "https://api.alpaca.markets/");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("APCA-API-KEY-ID", brokerAccount.ApiKey);
            client.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", brokerAccount.ApiSecret);

            _httpClientCache.TryAdd(brokerAccountId, client);
            return client;
        }

        public async Task<AccountInfo> GetAccountInfoAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);

            var response = await httpClient.GetAsync("v2/account");
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

        public async Task<OrderResponse> PlaceOrderAsync(Guid brokerAccountId, OrderRequest orderRequest)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);

            var response = await httpClient.PostAsJsonAsync("v2/orders", orderRequest);
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
                {
                    throw new Exception("Alpaca API yanıtı boş döndü.");
                }

                return orderResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"JSON deserialize sırasında bir hata oluştu: {ex.Message} - Yanıt: {responseContent}");
            }
        }

        public async Task<List<Portfolio>> GetPortfolioAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);

            var response = await httpClient.GetAsync("v2/positions");
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
                BrokerAccountId = brokerAccountId,
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

        public async Task<List<OrderResponse>> GetFilledOrdersAsync(Guid brokerAccountId, DateTime startDate, DateTime endDate)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);

            // Tarihleri ISO 8601 formatına dönüştür
            var afterDate = startDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            var untilDate = endDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            // Alpaca endpoint
            var url = $"/v2/orders?status=filled&after={afterDate}&until={untilDate}";
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Alpaca API hatası: {response.StatusCode} - {errorContent}");
            }

            var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();
            return orders ?? new List<OrderResponse>();
        }


        //  Günlük Kar-Zarar Hesapla
        public async Task<Dictionary<string, string>> CalculateDailyPnL(List<OrderResponse> orders)
        {
            var pnlResults = new Dictionary<string, decimal>();

            foreach (var order in orders)
            {
                if (!decimal.TryParse(order.FilledQuantity, out var quantity) || quantity <= 0)
                    continue;

                if (!decimal.TryParse(order.FilledAvgPrice, out var avgPrice) || avgPrice <= 0)
                    continue;

                var totalValue = quantity * avgPrice;
                var sign = order.Side == "sell" ? 1 : -1; // Satış: Pozitif, Alış: Negatif

                if (!pnlResults.ContainsKey(order.Symbol))
                    pnlResults[order.Symbol] = 0;

                pnlResults[order.Symbol] += totalValue * sign;
            }

            // Değerleri formatlı string olarak dönüştür
            var formattedResults = pnlResults.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToString("N2") // Virgüllü format ve iki ondalık basamak
            );

            return formattedResults;
        }


        public async Task<List<PositionResponse>> GetPositionsAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);

            var response = await httpClient.GetAsync("/v2/positions");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Alpaca API hatası: {response.StatusCode} - {errorContent}");
            }

            var position = await response.Content.ReadFromJsonAsync<List<PositionResponse>>();
            if (position == null)
                return new List<PositionResponse>();

            return position.Select(position => new PositionResponse
            {
                BrokerAccountId = brokerAccountId,
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                MarketValue = position.MarketValue,
                CostBasis = position.CostBasis,
                UnrealizedPnL = position.UnrealizedPnL,
                CurrentPrice = position.CurrentPrice,
                AvailableQuantity = position.AvailableQuantity,
                EntryPrice = position.EntryPrice,
                RealizedPnL = position.RealizedPnL,
                TodayChange = position.TodayChange,
                UnrealizedPnLPercentage = position.UnrealizedPnLPercentage
            }).ToList();
        }

        public async Task<IResult> ClosePositionAsync(string symbol, decimal quantity, Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);

            var response = await httpClient.PostAsync($"/v2/positions/{symbol}/close",
                    new StringContent(JsonSerializer.Serialize(new { qty = quantity }), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ErrorResult($"Alpaca API hatası: {response.StatusCode} - {errorContent}");
                }

                return new SuccessResult("Pozisyon Alpaca üzerinde kapatıldı.");
        }



        public async Task<OrderResponse> CancelOrderAsync(string orderId, Guid brokerAccountId)
        {
        var httpClient = await ConfigureHttpClientAsync(brokerAccountId);

        var response = await httpClient.DeleteAsync($"v2/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderResponse>();
        }


        public async Task<OrderResponse[]> GetAllOrdersAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var response = await httpClient.GetAsync("v2/orders");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderResponse[]>();
        }

        public async Task<OrderResponse> GetOrderByIdAsync(string orderId, Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var response = await httpClient.GetAsync($"v2/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderResponse>();
        }

        public async Task<Position[]> GetOpenPositionsAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var response = await httpClient.GetAsync("v2/positions");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API isteği başarısız oldu: {response.StatusCode} - {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<Position[]>();
        }
    }
}
