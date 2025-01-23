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
using Position = AutoTraderApp.Domain.Entities.Position;


namespace AutoTraderApp.Infrastructure.Services.Alpaca
{
    public class AlpacaService : IAlpacaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly IBaseRepository<BrokerLog> _brokerLog;

        private readonly ConcurrentDictionary<Guid, HttpClient> _httpClientCache = new();

        public AlpacaService(
            IHttpClientFactory httpClientFactory,
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IBaseRepository<BrokerLog> brokerLog)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _brokerLog = brokerLog; _brokerAccountRepository = brokerAccountRepository ?? throw new ArgumentNullException(nameof(brokerAccountRepository));
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



        public async Task<bool> AlpacaLog(Guid brokerAccountId, string symbol, decimal? price, int? quantity, string msg)
        {
            await _brokerLog.AddAsync(new BrokerLog
            {
                BrokerAccountId = brokerAccountId,
                Message = msg,
                Symbol = symbol,
                Price = price,
                Quantity = quantity
            });

            return true;
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

                // Log işlemi
                await _brokerLog.AddAsync(new BrokerLog
                {
                    BrokerAccountId = brokerAccountId,
                    Symbol=orderResponse.Symbol,
                    Price = Convert.ToDecimal(orderResponse.LimitPrice),
                    Quantity = Convert.ToInt16(orderResponse.Quantity),
                    Message = $"Yeni emir oluşturuldu: {orderRequest.Side}"

                });


                return orderResponse;
            }
            catch (Exception ex)
            {
                var orderResponse = JsonSerializer.Deserialize<OrderResponse>(responseContent);
                // Log işlemi
                await _brokerLog.AddAsync(new BrokerLog
                {
                    BrokerAccountId = brokerAccountId,
                    Symbol = orderResponse.Symbol,
                    Price = Convert.ToDecimal(orderResponse.LimitPrice),
                    Quantity = Convert.ToInt16(orderResponse.Quantity),
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

        public async Task<IResult> CloseAllPositionAsync(Guid brokerAccountId)
        {
            Console.WriteLine($"Butuün açık pozisyonlar kapatma işlemi başlıyor. BrokerAccountId: {brokerAccountId}");

            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);

            try
            {
                var response = await httpClient.DeleteAsync($"/v2/positions?cancel_orders=true");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Alpaca API hatası: {response.StatusCode} - {responseContent}");
                    return new ErrorResult($"Alpaca API hatası: {response.StatusCode} - {responseContent}");
                }

                Console.WriteLine("Pozisyon başarıyla kapatıldı");
                return new SuccessResult("Bütün açık pozisyon başarıyla kapatıldı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pozisyonları kapatma sırasında hata oluştu: {ex.Message}");
                return new ErrorResult($"Pozisyonları kapatma sırasında hata oluştu: {ex.Message}");
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

                var closePositions = await CloseAllPositionAsync(brokerAccountId);
                Console.WriteLine("Tüm pozisyonlar başarıyla kapatıldı.");

                if (closePositions.Success!)
                {
                    Console.WriteLine($"Pozisyonlar kapatılırken hata oluştu. Hata: {closePositions.Message}");
                    return new ErrorResult("Tüm pozisyonları kapatma sırasında hata.");
                }
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

        public async Task<AssetDetails> GetMoversAsync(string symbol, Guid brokerAccountId)
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

        public async Task<string> GenerateDailyTradeReportAsync(Guid brokerAccountId, DateTime tradeDate)
        {
            var orders = await GetFilledOrdersAsync(brokerAccountId, tradeDate.Date, tradeDate.Date.AddDays(1));
            if (orders == null || !orders.Any())
                return "Günlük işlem bulunamadı.";

            var tradeDetails = new List<DailyTradeDetails>();

            foreach (var group in orders.GroupBy(o => o.Symbol))
            {
                var symbol = group.Key;
                var buyOrders = group.Where(o => o.Side.Equals("buy", StringComparison.OrdinalIgnoreCase)).ToList();
                var sellOrders = group.Where(o => o.Side.Equals("sell", StringComparison.OrdinalIgnoreCase)).ToList();

                // Toplam Alım ve Satış Miktarları ve Maliyetleri
                decimal totalBuyAmount = buyOrders.Sum(o => Convert.ToDecimal(o.FilledAvgPrice) * Convert.ToInt32(o.FilledQuantity));
                decimal totalSellAmount = sellOrders.Sum(o => Convert.ToDecimal(o.FilledAvgPrice) * Convert.ToInt32(o.FilledQuantity));

                decimal totalBuyQuantity = buyOrders.Sum(o => Convert.ToInt16(o.FilledQuantity));
                decimal totalSellQuantity = sellOrders.Sum(o => Convert.ToInt32(o.FilledQuantity));

                // Kar/Zarar Hesaplama
                decimal pnl = totalSellAmount - totalBuyAmount;
                decimal pnlPercentage = totalBuyAmount > 0 ? (pnl / totalBuyAmount) * 100 : 0;

                // StopLoss Kontrolü
                bool stopLossSales = sellOrders.Any(o => o.OrderClass == "bracket" && o.StopLoss != null);

                tradeDetails.Add(new DailyTradeDetails
                {
                    Symbol = symbol,
                    TotalBuyAmount = Math.Round(totalBuyAmount, 2),
                    TotalSellAmount = Math.Round(totalSellAmount, 2),
                    TotalBuyQuantity = totalBuyQuantity,
                    TotalSellQuantity = totalSellQuantity,
                    PnL = Math.Round(pnl, 2),
                    PnLPercentage = Math.Round(pnlPercentage, 2),
                    StopLossSale = stopLossSales
                });
            }

            // Genel analiz
            var totalBuyAmountOverall = tradeDetails.Sum(t => t.TotalBuyAmount);
            var totalSellAmountOverall = tradeDetails.Sum(t => t.TotalSellAmount);
            var totalPnL = totalSellAmountOverall - totalBuyAmountOverall;
            var totalPnLPercentage = totalBuyAmountOverall > 0 ? (totalPnL / totalBuyAmountOverall) * 100 : 0;

            var mostProfitable = tradeDetails.OrderByDescending(t => t.PnLPercentage).FirstOrDefault();
            var mostLoss = tradeDetails.OrderBy(t => t.PnLPercentage).FirstOrDefault();

            // Çıktı formatı
            var reportBuilder = new StringBuilder();
            reportBuilder.AppendLine("Her Hisse için Analiz");
            for (var i = 0; i < tradeDetails.Count; i++)
            {
                var trade = tradeDetails[i];
                var result = trade.PnL >= 0 ? "Kar" : "Zarar";
                reportBuilder.AppendLine($"{i + 1}. {trade.Symbol}");
                reportBuilder.AppendLine($"Toplam Alım Maliyeti: ${trade.TotalBuyAmount:N2}");
                reportBuilder.AppendLine($"Toplam Satış Geliri: ${trade.TotalSellAmount:N2}");
                reportBuilder.AppendLine($"Net Kar/Zarar: ${trade.PnL:N2} ({result})");
                reportBuilder.AppendLine($"Yüzde Kar/Zarar: {trade.PnLPercentage:+0.00;-0.00}%");
                reportBuilder.AppendLine();
            }

            reportBuilder.AppendLine("Genel Analiz");
            reportBuilder.AppendLine($"Toplam Alım Maliyeti: ${totalBuyAmountOverall:N2}");
            reportBuilder.AppendLine($"Toplam Satış Geliri: ${totalSellAmountOverall:N2}");
            reportBuilder.AppendLine($"Net Kar/Zarar: ${totalPnL:N2} ({(totalPnL >= 0 ? "Kar" : "Zarar")})");
            reportBuilder.AppendLine($"Yüzde Kar/Zarar: {totalPnLPercentage:+0.00;-0.00}%");
            reportBuilder.AppendLine();

            if (mostProfitable != null)
            {
                reportBuilder.AppendLine($"En Karlı Hisse: {mostProfitable.Symbol} (%{mostProfitable.PnLPercentage:+0.00;-0.00} kar)");
            }
            if (mostLoss != null)
            {
                reportBuilder.AppendLine($"En Zararlı Hisse: {mostLoss.Symbol} (%{mostLoss.PnLPercentage:+0.00;-0.00} zarar)");
            }

            return reportBuilder.ToString();
        }



    }
}
