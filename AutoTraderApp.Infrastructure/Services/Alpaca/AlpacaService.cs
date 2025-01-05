using Alpaca.Markets;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using System.Collections.Concurrent;
using System.Net;
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
        private readonly IAlpacaApiLogService _alpacaApiLogService;
        private readonly IBaseRepository<BrokerLog> _brokerLog;

        private readonly ConcurrentDictionary<Guid, HttpClient> _httpClientCache = new();

        public AlpacaService(
            IHttpClientFactory httpClientFactory,
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaApiLogService alpacaApiLogService,
            IBaseRepository<BrokerLog> brokerLog)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _brokerAccountRepository = brokerAccountRepository ?? throw new ArgumentNullException(nameof(brokerAccountRepository));
            _alpacaApiLogService = alpacaApiLogService;
            _brokerLog = brokerLog;
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


        private async Task<HttpClient> ConfigureDataApiHttpClientAsync(Guid brokerAccountId)
        {

            var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == brokerAccountId);
            if (brokerAccount == null)
            {
                throw new Exception("Broker hesabı bulunamadı.");
            }

            var dataApiUrl = "https://data.alpaca.markets";
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(dataApiUrl);
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

                // Log işlemi
                await _alpacaApiLogService.LogAsync(new AlpacaApiLog
                {
                    BrokerAccountId = brokerAccountId,
                    RequestUrl = "v2/account",
                    HttpMethod = "GET",
                    ResponseBody = responseContent,
                    ResponseStatusCode = (int)response.StatusCode,
                    CreatedAt = DateTime.UtcNow,
                    LogType = response.IsSuccessStatusCode ? "Info" : "Error"
                });

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

                // Log işlemi
                await   _brokerLog.AddAsync(new BrokerLog
                {
                    BrokerAccountId = brokerAccountId,
                    Message = $"Yeni emir oluşturuldu: {orderResponse.Symbol} - {orderResponse.Quantity} adet",

                });

                await _alpacaApiLogService.LogAsync(new AlpacaApiLog
                {
                    BrokerAccountId = brokerAccountId,
                    RequestUrl = "v2/orders",
                    HttpMethod = "POST",
                    RequestBody = JsonSerializer.Serialize(orderRequest),
                    ResponseBody = responseContent,
                    ResponseStatusCode = (int)response.StatusCode,
                    CreatedAt = DateTime.UtcNow,
                    LogType = response.IsSuccessStatusCode ? "Info" : "Error",
                    ErrorMessage = !response.IsSuccessStatusCode ? $"Hata: {responseContent}" : null
                });


                return orderResponse;
            }
            catch (Exception ex)
            {
                // Log işlemi
                await _brokerLog.AddAsync(new BrokerLog
                {
                    BrokerAccountId = brokerAccountId,
                    Message = $"Emir Oluştulurken hata Oldu:{ex.Message} - Yanıt: {responseContent}",

                });
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

        public async Task<IResult> ClosePositionAsync(string symbol, decimal? quantity, Guid brokerAccountId)
        {
            Console.WriteLine($"Pozisyon kapatma işlemi başlıyor. Symbol: {symbol}, BrokerAccountId: {brokerAccountId}");

            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);

            try
            {
                var response = await httpClient.DeleteAsync($"/v2/positions/{symbol}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Alpaca API hatası: {response.StatusCode} - {responseContent}");
                    return new ErrorResult($"Alpaca API hatası: {response.StatusCode} - {responseContent}");
                }

                Console.WriteLine($"Pozisyon başarıyla kapatıldı: {symbol}");
                return new SuccessResult($"Pozisyon başarıyla kapatıldı. Symbol: {symbol}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pozisyon kapatma sırasında hata: {ex.Message}");
                return new ErrorResult($"Pozisyon kapatma sırasında hata: {ex.Message}");
            }
        }


        public async Task<PositionResponse> GetPositionBySymbolAsync(string symbol, Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var response = await httpClient.GetAsync($"/v2/positions/{symbol}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Pozisyon bilgisi alınamadı: {response.StatusCode} - {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<PositionResponse>();
        }

        public async Task<IResult> ClosePartialPositionAsync(string symbol, decimal quantity, Guid brokerAccountId)
        {
            var position = await GetPositionBySymbolAsync(symbol, brokerAccountId);

            if (Convert.ToDecimal(position.AvailableQuantity) == 0)
                return new ErrorResult($"Hisse {symbol} için kullanılabilir miktar sıfır.");

            var closeQuantity = Math.Min(quantity, Convert.ToDecimal(position.AvailableQuantity));
            return await ClosePositionAsync(symbol, closeQuantity, brokerAccountId);
        }


        public async Task<List<OrderResponse>> GetRecentOrders(Guid brokerAccountId)
        {
            var orders = await GetFilledOrdersAsync(brokerAccountId, DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow);
            Console.WriteLine($"Son 5 dakika içinde gerçekleşen emirler: {JsonSerializer.Serialize(orders)}");
            return orders;
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

        public async Task<IResult> SellLossMakingPositionsAsync(Guid brokerAccountId, decimal lossThresholdPercentage = -5)
        {
            Console.WriteLine($"Zarara uğrayan hisseler satılmaya başlıyor. BrokerAccountId: {brokerAccountId}, Zarar Eşiği: %{lossThresholdPercentage}");

            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);

            var positions = await GetPositionsAsync(brokerAccountId);

            if (positions == null || !positions.Any())
            {
                Console.WriteLine("Hiçbir açık pozisyon bulunamadı.");
                return new ErrorResult("Hiçbir açık pozisyon bulunamadı.");
            }

            foreach (var position in positions)
            {
                if (Convert.ToDecimal(position.UnrealizedPnLPercentage) <= lossThresholdPercentage)
                {
                    Console.WriteLine($"Zarara düşen pozisyon tespit edildi. Symbol: {position.Symbol}, Zarar Yüzdesi: {position.UnrealizedPnLPercentage}%");

                    var closeResult = await ClosePositionAsync(position.Symbol, null, brokerAccountId);

                    if (closeResult.Success)
                    {
                        Console.WriteLine($"Pozisyon başarıyla kapatıldı: {position.Symbol}, Miktar: {position.Quantity}");
                    }
                    else
                    {
                        Console.WriteLine($"Pozisyon kapatılırken hata oluştu: {position.Symbol}, Hata: {closeResult.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Pozisyon zarar eşiğinin üzerinde: {position.Symbol}, Zarar Yüzdesi: {position.UnrealizedPnLPercentage}%");
                }
            }

            return new SuccessResult("Zarara düşen pozisyonlar başarıyla elden çıkarıldı.");
        }


        public async Task<IResult> SellAllPositionsAtEndOfDayAsync(Guid brokerAccountId)
        {
            Console.WriteLine($"Tüm pozisyonları kapatma işlemi başlıyor. BrokerAccountId: {brokerAccountId}");

            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);

            try
            {
                var response = await httpClient.DeleteAsync("/v2/positions");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Alpaca API hatası: {response.StatusCode} - {responseContent}");
                    return new ErrorResult($"Alpaca API hatası: {response.StatusCode} - {responseContent}");
                }

                Console.WriteLine("Tüm pozisyonlar başarıyla kapatıldı.");
                return new SuccessResult("Tüm pozisyonlar başarıyla kapatıldı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tüm pozisyonları kapatma sırasında hata: {ex.Message}");
                return new ErrorResult($"Tüm pozisyonları kapatma sırasında hata: {ex.Message}");
            }
        }

        public async Task<AssetDetails> GetAssetDetailsAsync(string symbol, Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var response = await httpClient.GetAsync($"/v2/assets/{symbol}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Varlık bilgisi alınamadı: {response.StatusCode} - {errorContent}");
            }
            return await response.Content.ReadFromJsonAsync<AssetDetails>();
        }
        public async Task<decimal> GetLatestPriceAsync(string symbol, Guid brokerAccountId)
        {
            var httpClient = await ConfigureDataApiHttpClientAsync(brokerAccountId);

            // Quotes endpoint'ini kullan
            var url = $"/v2/stocks/{symbol}/trades/latest";
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Fiyat verisi alınamadı: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();


            var tradeResponse = JsonSerializer.Deserialize<TradeResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tradeResponse?.Trade == null)
            {
                throw new Exception($"No trade data available for {symbol}");
            }

            return tradeResponse.Trade.Price;
        }
    

        public async Task<bool> IsSymbolValidAsync(string symbol, Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var response = await httpClient.GetAsync($"/v2/assets/{symbol}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Symbol bulunamadı: {symbol}");
                return false;
            }

            return response.IsSuccessStatusCode;
        }

    }
}
