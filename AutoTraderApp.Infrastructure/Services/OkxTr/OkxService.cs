using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AutoTraderApp.Infrastructure.Services.OkxTr
{
    public class OkxService : IOkxService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly IBaseRepository<BrokerLog> _brokerLog;

        public OkxService(IHttpClientFactory httpClientFactory, IBaseRepository<BrokerAccount> brokerAccountRepository, IBaseRepository<BrokerLog> brokerLog)
        {
            _httpClientFactory = httpClientFactory;
            _brokerAccountRepository = brokerAccountRepository;
            _brokerLog = brokerLog;
        }

        private async Task<HttpClient> ConfigureHttpClientAsync(Guid brokerAccountId, string method, string requestPath, string body = "")
        {
            var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == brokerAccountId);
            if (brokerAccount == null) throw new Exception("OKX Broker account not found");

            var baseUrl = "https://www.okx.com"; 
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ").Trim();
            var signature = GenerateSignature(timestamp, brokerAccount.ApiSecret, method, requestPath, body);

            client.DefaultRequestHeaders.Add("OK-ACCESS-KEY", brokerAccount.ApiKey.Trim());
            client.DefaultRequestHeaders.Add("OK-ACCESS-SIGN", signature.Trim());
            client.DefaultRequestHeaders.Add("OK-ACCESS-PASSPHRASE", brokerAccount.ApiPassphrase.Trim());
            client.DefaultRequestHeaders.Add("OK-ACCESS-TIMESTAMP", timestamp);

            if (brokerAccount.IsPaper)
            {
                client.DefaultRequestHeaders.Add("x-simulated-trading", "1");
            }

            return client;
        }

        private string GenerateSignature(string timestamp, string apiSecret, string method, string requestPath, string body)
        {
            var payload = $"{timestamp}{method.ToUpper()}{requestPath}{body}".Trim();
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret.Trim())))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
            }
        }

        public async Task<bool> OkxLog(Guid brokerAccountId, string symbol, string? Action, decimal? price, int? quantity, string msg)
        {
            await _brokerLog.AddAsync(new BrokerLog
            {
                BrokerAccountId = brokerAccountId,
                Message = "(OKX TR) " + msg,
                Symbol = symbol,
                Price = price,
                Quantity = quantity,
                Action = Action
            });

            return true;
        }

        public async Task<decimal> GetMarketPriceAsync(string symbol, Guid brokerAccountId)
        {
            var client = await ConfigureHttpClientAsync(brokerAccountId, "GET", $"/api/v5/market/ticker?instId={symbol}", "");
            var response = await client.GetAsync($"/api/v5/market/ticker?instId={symbol}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OKX API Hatası: {response.StatusCode} - {responseContent}");
            }

            try
            {
                dynamic data = JsonConvert.DeserializeObject(responseContent);

                if (data == null || data.data == null || data.data.Count == 0)
                {
                    throw new Exception($"OKX API beklenmedik formatta yanıt döndürdü: {responseContent}");
                }

                return Convert.ToDecimal(data.data[0].last, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new Exception($"OKX API yanıtı işlenirken hata oluştu: {ex.Message} \n Yanıt: {responseContent}");
            }
        }

        public async Task<bool> PlaceOrderAsync(Guid brokerAccountId, string symbol, decimal quantity, string orderType, bool isMarginTrade)
        {
            var requestPath = "/api/v5/trade/order";
            var body = JsonConvert.SerializeObject(new
            {
                instId = symbol,
                tdMode = isMarginTrade ? "cross" : "cash",
                side = orderType.ToLower(),
                ordType = "market",
                sz = quantity.ToString(CultureInfo.InvariantCulture)
            });

            var client = await ConfigureHttpClientAsync(brokerAccountId, "POST", requestPath, body);
            var response = await client.PostAsync(requestPath, new StringContent(body, Encoding.UTF8, "application/json"));
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OKX Order API Hatası: {response.StatusCode} - {responseContent}");
            }

            try
            {
                dynamic data = JsonConvert.DeserializeObject(responseContent);

                if (data.code != "0" && data.data != null && data.data.Count > 0)
                {
                    string errorCode = data.data[0].sCode;
                    string errorMessage = data.data[0].sMsg;

                    if (errorCode == "51008")
                    {
                        throw new Exception("OKX işlem hatası: Yetersiz BTC bakiyesi nedeniyle emir başarısız oldu.");
                    }
                    else if (errorCode == "51004")
                    {
                        throw new Exception("OKX işlem hatası: Geçersiz işlem miktarı.");
                    }
                    else if (errorCode == "50000")
                    {
                        throw new Exception("OKX işlem hatası: Genel bir hata oluştu, lütfen tekrar deneyin.");
                    }
                    else
                    {
                        throw new Exception($"OKX işlem hatası: {errorMessage}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"OKX Order API yanıtı işlenirken hata oluştu: {ex.Message} \n Yanıt: {responseContent}");
            }
        }


        public async Task<bool> CancelOrderAsync(Guid brokerAccountId, string orderId)
        {
            var requestPath = "/api/v5/trade/cancel-order";
            var body = JsonConvert.SerializeObject(new { ordId = orderId });

            var client = await ConfigureHttpClientAsync(brokerAccountId, "POST", requestPath, body);
            var response = await client.PostAsync(requestPath, new StringContent(body, Encoding.UTF8, "application/json"));
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OKX API Hatası: {response.StatusCode} - {responseContent}");
            }

            return true;
        }

        public async Task<decimal> GetAccountBalanceAsync(Guid brokerAccountId, string currency = "USDT")
        {
            var requestPath = "/api/v5/account/balance";
            var client = await ConfigureHttpClientAsync(brokerAccountId, "GET", requestPath);

            var response = await client.GetAsync(requestPath);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OKX API Hatası: {response.StatusCode} - {responseContent}");
            }

            try
            {
                dynamic data = JsonConvert.DeserializeObject(responseContent);
                if (data == null || data.data == null || data.data.Count == 0 || data.data[0].details == null)
                {
                    throw new Exception("OKX API yanıtı beklenen formatta değil.");
                }

                foreach (var detail in data.data[0].details)
                {
                    if (detail.ccy == currency)  
                    {
                        if (decimal.TryParse(detail.availBal.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal balance))
                        {
                            return balance;
                        }
                    }
                }

                return 0; 
            }
            catch (Exception ex)
            {
                throw new Exception($"OKX API yanıtı işlenirken hata oluştu: {ex.Message} \n Yanıt: {responseContent}");
            }
        }

        public async Task<object> GetAccountInfoAsync(Guid brokerAccountId)
        {
            var requestPath = "/api/v5/account/balance";
            var client = await ConfigureHttpClientAsync(brokerAccountId, "GET", requestPath);

            var request = new HttpRequestMessage(HttpMethod.Get, requestPath);
            request.Headers.Add("Accept", "application/json");

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OKX API Hatası: {response.StatusCode} - {responseContent}");
            }

            try
            {
                dynamic data = JsonConvert.DeserializeObject(responseContent);
                if (data == null || data.data == null || data.data.Count == 0 || data.data[0].details == null || data.data[0].details.Count == 0)
                {
                    throw new Exception("OKX API yanıtı beklenen formatta değil veya hesap bilgileri bulunamadı.");
                }

                var details = data.data[0].details[0];

                static decimal SafeDecimalParse(string value)
                {
                    return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result) ? result : 0m;
                }

                decimal balance = SafeDecimalParse(details.availEq);
                decimal balanceUsd = SafeDecimalParse(details.eqUsd);
                decimal cashBalance = SafeDecimalParse(details.cashBal);
                decimal frozenBalance = SafeDecimalParse(details.frozenBal);
                decimal totalPnl = SafeDecimalParse(details.totalPnl);
                decimal unrealizedPnl = SafeDecimalParse(details.upl);
                decimal leverage = SafeDecimalParse(details.notionalLever);
                decimal initialMargin = SafeDecimalParse(details.imr);
                decimal maintenanceMargin = SafeDecimalParse(details.mmr);
                decimal marginRatio = SafeDecimalParse(details.mgnRatio);

                var openOrders = await GetActiveOrdersAsync(brokerAccountId);

                return new
                {
                    Balance = balance,
                    BalanceUsd = balanceUsd,
                    CashBalance = cashBalance,
                    FrozenBalance = frozenBalance,
                    TotalPnl = totalPnl,
                    UnrealizedPnl = unrealizedPnl,
                    Leverage = leverage,
                    InitialMargin = initialMargin,
                    MaintenanceMargin = maintenanceMargin,
                    MarginRatio = marginRatio,
                    OpenOrders = openOrders
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"OKX API yanıtı işlenirken hata oluştu: {ex.Message} \n Yanıt: {responseContent}");
            }
        }



        public async Task<List<object>> GetActiveOrdersAsync(Guid brokerAccountId)
        {
            var client = await ConfigureHttpClientAsync(brokerAccountId, "GET", "/api/v5/trade/orders-pending");
            var response = await client.GetAsync("/api/v5/trade/orders-pending");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OKX API Hatası (Açık Emirler): {response.StatusCode} - {responseContent}");
            }

            try
            {
                dynamic data = JsonConvert.DeserializeObject(responseContent);
                var orders = new List<object>();

                foreach (var order in data.data)
                {
                    orders.Add(new
                    {
                        OrderId = order.ordId,
                        Symbol = order.instId,
                        Side = order.side,
                        OrderType = order.ordType,
                        Quantity = Convert.ToDecimal(order.sz),
                        Price = Convert.ToDecimal(order.px),
                        Status = order.state
                    });
                }

                return orders;
            }
            catch (Exception ex)
            {
                throw new Exception($"OKX Açık Emirler Yanıtı İşlenirken Hata: {ex.Message} \n Yanıt: {responseContent}");
            }
        }

        public async Task<decimal> GetCryptoPositionAsync(string symbol, Guid brokerAccountId)
        {
            var requestPath = "/api/v5/account/balance";
            var client = await ConfigureHttpClientAsync(brokerAccountId, "GET", requestPath);

            var response = await client.GetAsync(requestPath);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OKX API Hatası: {response.StatusCode} - {responseContent}");
            }

            try
            {
                dynamic data = JsonConvert.DeserializeObject(responseContent);

                if (data == null || data.data == null || data.data.Count == 0 || data.data[0].details == null)
                {
                    throw new Exception("OKX API beklenmedik formatta yanıt döndürdü.");
                }

                // ✅ **Sembolü bul ve sadece "available" (kullanılabilir) bakiyeyi al**
                foreach (var asset in data.data[0].details)
                {
                    if (asset.ccy == symbol.Split('-')[0])  // Örn: "ALGO-USDT" -> "ALGO"
                    {
                        decimal availableBalance = Convert.ToDecimal(asset.availBal, CultureInfo.InvariantCulture);
                        return availableBalance;
                    }
                }

                return 0; // ✅ **Eğer sembol hiç yoksa sıfır döndür**
            }
            catch (Exception ex)
            {
                throw new Exception($"OKX GetCryptoPositionAsync Hatası: {ex.Message} \n Yanıt: {responseContent}");
            }
        }


        public async Task<decimal> AdjustQuantityForOkx(string symbol, decimal quantity, Guid brokerAccountId)
        {
            var requestPath = $"/api/v5/public/instruments?instType=SPOT&instId={symbol}";
            var client = await ConfigureHttpClientAsync(brokerAccountId, "GET", requestPath);

            var response = await client.GetAsync(requestPath);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OKX API Hatası: {response.StatusCode} - {responseContent}");
            }

            try
            {
                dynamic data = JsonConvert.DeserializeObject(responseContent);

                if (data == null || data.data == null || data.data.Count == 0)
                {
                    throw new Exception($"OKX Lot Bilgisi Bulunamadı: {responseContent}");
                }

                decimal minSize = Convert.ToDecimal(data.data[0].minSz, CultureInfo.InvariantCulture);
                decimal tickSize = Convert.ToDecimal(data.data[0].lotSz, CultureInfo.InvariantCulture);

                // ✅ **Lot büyüklüğüne göre yuvarlama**
                decimal adjustedQuantity = Math.Floor(quantity / tickSize) * tickSize;

                // ✅ **Min lot büyüklüğünün altına düşmesini engelle**
                if (adjustedQuantity < minSize)
                {
                    throw new Exception($"OKX lot büyüklüğü çok düşük: Minimum {minSize} işlem yapılabilir.");
                }

                return adjustedQuantity;
            }
            catch (Exception ex)
            {
                throw new Exception($"OKX Lot Ayarlama Hatası: {ex.Message} \n Yanıt: {responseContent}");
            }
        }


    }
}
