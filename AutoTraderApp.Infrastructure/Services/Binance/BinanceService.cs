using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Binance;
using AutoTraderApp.Infrastructure.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AutoTraderApp.Infrastructure.Services.Binance
{
    public class BinanceService : IBinanceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly ConcurrentDictionary<Guid, HttpClient> _httpClientCache = new();

        public BinanceService(IHttpClientFactory httpClientFactory, IBaseRepository<BrokerAccount> brokerAccountRepository)
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
            if (brokerAccount == null || brokerAccount.BrokerName != "Binance")
            {
                throw new Exception("Binance broker hesabı bulunamadı.");
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(brokerAccount.IsPaper ? "https://testnet.binance.vision/api" : "https://api.binance.com/");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-MBX-APIKEY", brokerAccount.ApiKey);

            _httpClientCache.TryAdd(brokerAccountId, client);
            return client;
        }

        private string GenerateSignature(string queryString, string apiSecret)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(apiSecret));
            byte[] signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
            return BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();
        }

        public async Task<decimal> GetAccountBalanceAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var queryString = $"timestamp={timestamp}&recvWindow=60000";
            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/api/v3/account?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Hata: {response.StatusCode} - {responseContent}");
            }

            var accountData = JsonConvert.DeserializeObject<BinanceAccountResponse>(responseContent);
            if (accountData == null || accountData.Balances == null) return 0m;

            decimal totalBalanceInUSDT = 0m;

            foreach (var balance in accountData.Balances)
            {
                if (balance.Free > 0)
                {
                    if (balance.Asset == "USDT")
                    {
                        totalBalanceInUSDT += balance.Free;
                    }
                    else
                    {
                        try
                        {
                            decimal priceInUSDT = await GetMarketPriceAsync(balance.Asset + "USDT", brokerAccountId);
                            decimal assetValue = balance.Free * priceInUSDT;
                            if (priceInUSDT == 0)
                            {
                                continue;
                            }

                            totalBalanceInUSDT += assetValue;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Fiyat alınamadı: {balance.Asset} - {ex.Message}");
                        }
                    }
                }
            }

            Console.WriteLine($"Toplam USDT Karşılığı: {totalBalanceInUSDT}");
            return totalBalanceInUSDT;
        }


        public async Task<decimal> GetMarketPriceAsync(string symbol, Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var response = await httpClient.GetAsync($"/api/v3/ticker/price?symbol={symbol}");

            if (!response.IsSuccessStatusCode)
            {
                return 0m;  
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var priceData = JsonConvert.DeserializeObject<BinancePriceResponse>(responseContent);
            decimal price = priceData?.Price ?? 0m;

            if (price <= 0 || price > 1_000_000) 
            {
                return 0m;
            }

            return price;
        }


        public async Task<BrokerAccount?> GetBinanceAccountAsync(Guid brokerAccountId)
        {
            return await _brokerAccountRepository.GetAsync(b => b.Id == brokerAccountId && b.BrokerName == "Binance");
        }

        public async Task<object> GetAccountInfoAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account bulunamadı.");

            var queryString = $"timestamp={timestamp}&recvWindow=60000";
            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/api/v3/account?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();


            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Account Error: {response.StatusCode} - {responseContent}");
            }

            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            var balances = root.GetProperty("balances")
                .EnumerateArray()
                .Where(b =>
                    decimal.TryParse(b.GetProperty("free").GetString(), out var free) && free > 0 ||
                    decimal.TryParse(b.GetProperty("locked").GetString(), out var locked) && locked > 0)
                .Select(b => new
                {
                    Asset = b.GetProperty("asset").GetString(),
                    Free = b.GetProperty("free").GetString(),
                    Locked = b.GetProperty("locked").GetString()
                })
                .ToList();


            var filteredAccountData = new
            {
                AccountType = root.GetProperty("accountType").GetString(),
                CanTrade = root.GetProperty("canTrade").GetBoolean(),
                CanWithdraw = root.GetProperty("canWithdraw").GetBoolean(),
                CanDeposit = root.GetProperty("canDeposit").GetBoolean(),
                UID = root.GetProperty("uid").GetInt64(),
                Balances = balances
            };

            return filteredAccountData;
        }

        public async Task<Dictionary<string, decimal>> GetTotalPortfolioValueAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var requestParams = new Dictionary<string, string>
            {
                ["timestamp"] = timestamp,
                ["recvWindow"] = "60000"
            };

            var queryString = string.Join("&", requestParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var accountResponse = await httpClient.GetAsync($"/api/v3/account?{queryString}");
            var accountContent = await accountResponse.Content.ReadAsStringAsync();

            if (!accountResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Hata: {accountResponse.StatusCode} - {accountContent}");
            }

            var accountData = JsonSerializer.Deserialize<JsonElement>(accountContent);
            var balances = accountData.GetProperty("balances").EnumerateArray()
                .Where(b =>
                    decimal.Parse(b.GetProperty("free").GetString(), CultureInfo.InvariantCulture) > 0 ||
                    decimal.Parse(b.GetProperty("locked").GetString(), CultureInfo.InvariantCulture) > 0 
                )
                .Select(b => new
                {
                    Asset = b.GetProperty("asset").GetString(),
                    FreeAmount = decimal.Parse(b.GetProperty("free").GetString(), CultureInfo.InvariantCulture)
                })
                .ToList();

            var portfolioValue = new Dictionary<string, decimal>();
            decimal totalValue = 0;

            var symbols = balances.Where(b => b.Asset != "USDT")
                                  .Select(b => b.Asset + "USDT")
                                  .ToList();

            var priceResponse = await httpClient.GetAsync($"/api/v3/ticker/price");
            if (!priceResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Fiyat Hata: {priceResponse.StatusCode}");
            }

            var priceContent = await priceResponse.Content.ReadAsStringAsync();
            var prices = JsonSerializer.Deserialize<JsonElement>(priceContent)
                                       .EnumerateArray()
                                       .ToDictionary(x => x.GetProperty("symbol").GetString(),
                                                     x => decimal.Parse(x.GetProperty("price").GetString(), CultureInfo.InvariantCulture));

            foreach (var balance in balances)
            {
                if (balance.Asset == "USDT")
                {
                    portfolioValue["USDT"] = Math.Round(balance.FreeAmount, 2);
                    totalValue += balance.FreeAmount;
                    continue;
                }

                var symbol = balance.Asset + "USDT";

                if (prices.TryGetValue(symbol, out decimal price))
                {
                    decimal assetValue = balance.FreeAmount * price;
                    portfolioValue[balance.Asset] = Math.Round(assetValue, 2);
                    totalValue += assetValue;
                }
                else
                {
                    Console.WriteLine($"⚠️ {symbol} için fiyat bulunamadı, atlanıyor...");
                }
            }

            portfolioValue["TotalValue"] = Math.Round(totalValue, 2); 

            return portfolioValue;
        }

        public async Task<decimal> GetSpotBalanceAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var queryString = $"timestamp={timestamp}&recvWindow=60000";
            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/api/v3/account?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Hata: {response.StatusCode} - {responseContent}");
            }

            var accountData = JsonConvert.DeserializeObject<BinanceAccountResponse>(responseContent);

            return accountData?.Balances.FirstOrDefault(b => b.Asset == "USDT")?.Free ?? 0m;
        }

        public async Task<decimal> GetFundingBalanceAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var queryString = $"timestamp={timestamp}&recvWindow=60000";
            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/sapi/v1/asset/get-funding-asset?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Hata: {response.StatusCode} - {responseContent}");
            }

            var fundingData = JsonConvert.DeserializeObject<BinanceFundingResponse>(responseContent);

            if (fundingData?.Balances == null)
                return 0m;

            var fundingUsdt = fundingData.Balances.FirstOrDefault(b => b.Asset == "USDT")?.Free ?? 0m;
            return fundingUsdt;
        }

        public async Task<decimal> GetCrossMarginBalanceAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var queryString = $"timestamp={timestamp}&recvWindow=60000";
            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/sapi/v1/margin/account?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Hata: {response.StatusCode} - {responseContent}");
            }

            var marginData = JsonConvert.DeserializeObject<BinanceMarginResponse>(responseContent);

            if (marginData?.Assets == null)
                return 0m;

            var marginUsdt = marginData.Assets.FirstOrDefault(b => b.Asset == "USDT")?.Free ?? 0m;
            return marginUsdt;
        }

        public async Task<decimal> GetIsolatedMarginBalanceAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var queryString = $"timestamp={timestamp}&recvWindow=60000";
            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/sapi/v1/margin/isolated/account?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Hata: {response.StatusCode} - {responseContent}");
            }

            var isolatedData = JsonConvert.DeserializeObject<BinanceIsolatedMarginResponse>(responseContent);

            if (isolatedData?.Assets == null)
                return 0m;

            var isolatedUsdt = isolatedData.Assets.FirstOrDefault(b => b.QuoteAsset.Asset == "USDT")?.QuoteAsset.Free ?? 0m;
            return isolatedUsdt;
        }

        public async Task<decimal> GetTotalBalanceAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var queryString = $"timestamp={timestamp}&recvWindow=60000";
            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/api/v3/account?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Hata: {response.StatusCode} - {responseContent}");
            }

            var accountData = JsonConvert.DeserializeObject<BinanceAccountResponse>(responseContent);
            if (accountData?.Balances == null)
                return 0m;

            var assetsWithBalance = accountData.Balances.Where(b => b.Free > 0).ToList();

            decimal totalBalance = 0m;
            var priceDictionary = new Dictionary<string, decimal>();

            foreach (var asset in assetsWithBalance)
            {
                if (asset.Asset == "USDT")
                {
                    totalBalance += asset.Free;
                    continue;
                }

                try
                {
                    if (!priceDictionary.ContainsKey(asset.Asset))
                    {
                        var priceResponse = await httpClient.GetAsync($"/api/v3/ticker/price?symbol={asset.Asset}USDT");
                        var priceContent = await priceResponse.Content.ReadAsStringAsync();
                        var priceData = JsonConvert.DeserializeObject<BinancePriceResponse>(priceContent);

                        if (priceResponse.IsSuccessStatusCode && priceData != null)
                        {
                            priceDictionary[asset.Asset] = priceData.Price;
                        }
                        else
                        {
                            Console.WriteLine($"UYARI: {asset.Asset}/USDT için fiyat bulunamadı, hesaba dahil edilmedi.");
                            continue;
                        }
                    }

                    totalBalance += asset.Free * priceDictionary[asset.Asset];
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"HATA: {asset.Asset}/USDT fiyatı çekilirken hata oluştu: {ex.Message}");
                }
            }

            return totalBalance;
        }

        public async Task<bool> PlaceOrderAsync(Guid brokerAccountId, string symbol, decimal quantity, string action, bool isMarginTrade = false)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var formattedQuantity = quantity.ToString("F8", System.Globalization.CultureInfo.InvariantCulture);

            var requestBody = new Dictionary<string, string>
            {
                ["symbol"] = symbol.ToUpper().Trim(),
                ["side"] = action.ToUpper().Trim(),
                ["type"] = "MARKET",
                ["quantity"] = formattedQuantity,
                ["timestamp"] = timestamp,
                ["recvWindow"] = "60000"
            };

            var queryString = string.Join("&", requestBody.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            Console.WriteLine($"Request Parameters: {queryString}");

            string endpoint = isMarginTrade ? "/sapi/v1/margin/order" : "/api/v3/order"; 
            var response = await httpClient.PostAsync(endpoint + "?" + queryString, null);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Binance API Response: {response.StatusCode} - {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Order Error: {response.StatusCode} - {responseContent}");
            }

            return true;
        }

        public async Task<bool> PlaceStopLossOrderAsync(Guid brokerAccountId, string symbol, decimal quantity, decimal stopLossPrice)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // Get current market price
            var currentPrice = await GetMarketPriceAsync(symbol, brokerAccountId);
            if (currentPrice <= 0)
                throw new Exception($"Could not get current market price for {symbol}");

            var symbolInfo = await GetExchangeInfoAsync(brokerAccountId, symbol);
            if (symbolInfo == null)
                throw new Exception($"Could not get exchange info for {symbol}");

            // Extract all required filters
            var priceFilter = symbolInfo.Filters.FirstOrDefault(f => f.FilterType == "PRICE_FILTER");
            var minNotionalFilter = symbolInfo.Filters.FirstOrDefault(f => f.FilterType == "NOTIONAL");
            var percentPriceFilter = symbolInfo.Filters.FirstOrDefault(f => f.FilterType == "PERCENT_PRICE_BY_SIDE");

            if (priceFilter == null || minNotionalFilter == null || percentPriceFilter == null)
                throw new Exception($"Missing price filters for {symbol}");

            // Parse filter values with proper decimal culture
            decimal tickSize = decimal.Parse(priceFilter.TickSize, CultureInfo.InvariantCulture);
            decimal minPrice = decimal.Parse(priceFilter.MinPrice, CultureInfo.InvariantCulture);
            decimal maxPrice = decimal.Parse(priceFilter.MaxPrice, CultureInfo.InvariantCulture);
            decimal minNotional = decimal.Parse(minNotionalFilter.MinNotional, CultureInfo.InvariantCulture);

            // Adjust stop loss price to proper tick size
            var tickSizeDecimals = BitConverter.GetBytes(decimal.GetBits(tickSize)[3])[2];
            stopLossPrice = Math.Round(stopLossPrice / tickSize, 0, MidpointRounding.ToPositiveInfinity) * tickSize;
            stopLossPrice = decimal.Round(stopLossPrice, tickSizeDecimals);

            // Ensure price is within allowed range
            stopLossPrice = Math.Max(minPrice, Math.Min(maxPrice, stopLossPrice));

            // Check if price would trigger immediately
            if (stopLossPrice >= currentPrice)
            {
                stopLossPrice = decimal.Round(currentPrice * 0.99m / tickSize, 0, MidpointRounding.ToPositiveInfinity) * tickSize;
            }

            // Calculate and check notional value
            decimal orderValue = stopLossPrice * quantity;
            if (orderValue < minNotional)
            {
                throw new Exception($"Order value ({orderValue}) is below minimum notional value ({minNotional})");
            }

            // Format values with proper precision
            var formattedQuantity = quantity.ToString($"F{tickSizeDecimals}", CultureInfo.InvariantCulture);
            var formattedStopLossPrice = stopLossPrice.ToString($"F{tickSizeDecimals}", CultureInfo.InvariantCulture);
            var formattedLimitPrice = stopLossPrice.ToString($"F{tickSizeDecimals}", CultureInfo.InvariantCulture);

            var requestBody = new Dictionary<string, string>
            {
                ["symbol"] = symbol.ToUpper().Trim(),
                ["side"] = "SELL",
                ["type"] = "STOP_LOSS_LIMIT",
                ["quantity"] = formattedQuantity,
                ["price"] = formattedLimitPrice,
                ["stopPrice"] = formattedStopLossPrice,
                ["timeInForce"] = "GTC",
                ["timestamp"] = timestamp,
                ["recvWindow"] = "60000"
            };

            var queryString = string.Join("&", requestBody.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null)
                throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            try
            {
                var response = await httpClient.PostAsync("/api/v3/order?" + queryString, null);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Binance API Stop-Loss Order Error: {response.StatusCode} - {responseContent}");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to place stop-loss order: {ex.Message}");
            }
        }

        public async Task<decimal> AdjustPriceForBinance(string symbol, decimal price, decimal currentPrice, Guid brokerAccountId)
        {
            var symbolInfo = await GetExchangeInfoAsync(brokerAccountId, symbol);
            if (symbolInfo == null)
            {
                throw new Exception($"Binance price filter info not found: {symbol}");
            }

            var priceFilter = symbolInfo.Filters.FirstOrDefault(f => f.FilterType == "PRICE_FILTER");
            if (priceFilter == null)
            {
                throw new Exception($"Price filter not found for: {symbol}");
            }

            decimal tickSize = decimal.Parse(priceFilter.TickSize, CultureInfo.InvariantCulture);
            decimal minPrice = decimal.Parse(priceFilter.MinPrice, CultureInfo.InvariantCulture);
            decimal maxPrice = decimal.Parse(priceFilter.MaxPrice, CultureInfo.InvariantCulture);

            // Get decimal places from tick size
            var tickSizeDecimals = BitConverter.GetBytes(decimal.GetBits(tickSize)[3])[2];

            // Round to valid tick size
            var adjustedPrice = Math.Round(price / tickSize, 0, MidpointRounding.ToPositiveInfinity) * tickSize;
            adjustedPrice = decimal.Round(adjustedPrice, tickSizeDecimals);

            // Ensure price is within allowed range
            adjustedPrice = Math.Max(minPrice, Math.Min(maxPrice, adjustedPrice));

            return adjustedPrice;
        }

        public async Task<bool> CheckExistingStopLossOrderAsync(Guid brokerAccountId, string symbol)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var requestBody = new Dictionary<string, string>
            {
                ["symbol"] = symbol.ToUpper().Trim(),
                ["timestamp"] = timestamp,
                ["recvWindow"] = "60000"
            };

            var queryString = string.Join("&", requestBody.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null)
                throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/api/v3/openOrders?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Binance API Error: {response.StatusCode} - {responseContent}");

            var orders = JsonConvert.DeserializeObject<List<BinanceOrder>>(responseContent);

            return orders.Any(o => o.Symbol == symbol && o.Type == "STOP_LOSS_LIMIT");
        }

        public async Task<decimal> GetMinOrderSizeAsync(Guid brokerAccountId, string symbol)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var response = await httpClient.GetAsync($"/api/v3/exchangeInfo?symbol={symbol.ToUpper()}");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Binance API Hata: {content}");

            var json = JObject.Parse(content);
            var filters = json["symbols"]?.FirstOrDefault()?["filters"];
            var lotSizeFilter = filters?.FirstOrDefault(f => f["filterType"]?.ToString() == "LOT_SIZE");

            if (lotSizeFilter != null)
            {
                return decimal.Parse(lotSizeFilter["minQty"]?.ToString() ?? "0", CultureInfo.InvariantCulture);
            }

            throw new Exception("Binance LOT_SIZE filtresi bulunamadı.");
        }

        public async Task<string> GetAllOrdersBySymbolAsync(Guid brokerAccountId, string symbol)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var requestParams = new Dictionary<string, string>
            {
                ["symbol"] = symbol.ToUpper().Trim(),
                ["timestamp"] = timestamp,
                ["recvWindow"] = "60000"
            };

            var queryString = string.Join("&", requestParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/api/v3/allOrders?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Order History Error: {response.StatusCode} - {responseContent}");
            }

            return responseContent;
        }
    
        public async Task<string> GetTradeHistoryAsync(Guid brokerAccountId, string symbol)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var requestParams = new Dictionary<string, string>
            {
                ["symbol"] = symbol.ToUpper().Trim(),
                ["timestamp"] = timestamp,
                ["recvWindow"] = "60000"
            };

            var queryString = string.Join("&", requestParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/api/v3/myTrades?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Binance API Response (Trade History): {response.StatusCode} - {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Trade History Error: {response.StatusCode} - {responseContent}");
            }

            return responseContent;
        }

        public async Task<string> PlaceMarginBuyOrderAsync(Guid brokerAccountId, string symbol, decimal quantity)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var requestParams = new Dictionary<string, string>
            {
                ["symbol"] = symbol, 
                ["side"] = "BUY",
                ["type"] = "MARKET",
                ["quantity"] = quantity.ToString(CultureInfo.InvariantCulture),
                ["isIsolated"] = "FALSE", 
                ["timestamp"] = timestamp
            };

            var queryString = string.Join("&", requestParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.PostAsync($"/sapi/v1/margin/order?{queryString}", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance Margin API Hata: {response.StatusCode} - {responseContent}");
            }

            return responseContent;
        }

        public async Task<string> BorrowAssetAsync(Guid brokerAccountId, string asset, decimal amount)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var requestParams = new Dictionary<string, string>
            {
                ["asset"] = asset, 
                ["amount"] = amount.ToString(CultureInfo.InvariantCulture),
                ["timestamp"] = timestamp
            };

            var queryString = string.Join("&", requestParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.PostAsync($"/sapi/v1/margin/loan?{queryString}", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance Margin Borrow Hata: {response.StatusCode} - {responseContent}");
            }

            return responseContent;
        }

        public async Task<string> PlaceMarginSellOrderAsync(Guid brokerAccountId, string symbol, decimal quantity)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var requestParams = new Dictionary<string, string>
            {
                ["symbol"] = symbol, 
                ["side"] = "SELL",
                ["type"] = "MARKET",
                ["quantity"] = quantity.ToString(CultureInfo.InvariantCulture),
                ["isIsolated"] = "FALSE",
                ["timestamp"] = timestamp
            };

            var queryString = string.Join("&", requestParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.PostAsync($"/sapi/v1/margin/order?{queryString}", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance Margin API Hata: {response.StatusCode} - {responseContent}");
            }

            return responseContent;
        }

        public async Task<string> RepayBorrowedAssetAsync(Guid brokerAccountId, string asset, decimal amount)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var requestParams = new Dictionary<string, string>
            {
                ["asset"] = asset, 
                ["amount"] = amount.ToString(CultureInfo.InvariantCulture),
                ["timestamp"] = timestamp
            };

            var queryString = string.Join("&", requestParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.PostAsync($"/sapi/v1/margin/repay?{queryString}", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance Margin Repay Hata: {response.StatusCode} - {responseContent}");
            }

            return responseContent;
        }

        public async Task<List<MarginOrderResponse>> GetMarginOrdersAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var requestParams = new Dictionary<string, string>
            {
                ["timestamp"] = timestamp
            };

            var queryString = string.Join("&", requestParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/sapi/v1/margin/openOrders?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Hata: {response.StatusCode} - {responseContent}");
            }

            return JsonSerializer.Deserialize<List<MarginOrderResponse>>(responseContent);
        }

        public async Task<MarginBalanceResponse> GetMarginBalanceAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var requestParams = new Dictionary<string, string>
            {
                ["timestamp"] = timestamp
            };

            var queryString = string.Join("&", requestParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/sapi/v1/margin/account?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Hata: {response.StatusCode} - {responseContent}");
            }

            return JsonSerializer.Deserialize<MarginBalanceResponse>(responseContent);
        }

        public async Task<long?> GetBinanceUIDAsync(Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("❌ Broker account bulunamadı.");

            var queryString = $"timestamp={timestamp}&recvWindow=60000";
            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/api/v3/account?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Account Error: {response.StatusCode} - {responseContent}");
            }

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return null;
            }

            try
            {
                var accountData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                if (accountData.TryGetProperty("uid", out var uidProperty))
                {
                    long uid = uidProperty.GetInt64();
                    return uid;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
                
        public async Task<bool> ValidateUserByUIDAsync(Guid brokerAccountId, long expectedUID)
        {
            var uid = await GetBinanceUIDAsync(brokerAccountId);
            if (uid == null) return false;

            return uid == expectedUID;
        }

        public async Task<BinanceSymbolInfo> GetExchangeInfoAsync(Guid brokerAccountId,string symbol)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var response = await httpClient.GetAsync($"api/v3/exchangeInfo?symbol={symbol}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Binance Exchange Info alınamadı.");
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Binance API Yanıtı:");
            Console.WriteLine(json);

            var exchangeInfo = JsonConvert.DeserializeObject<BinanceExchangeInfo>(json);

            return exchangeInfo.Symbols.FirstOrDefault(s => s.Symbol == symbol);
        }

        public async Task<decimal> AdjustQuantityForBinance(string symbol, decimal requestedQuantity, decimal price, Guid brokerAccountId, bool isSellOrder)
        {
            var symbolInfo = await GetExchangeInfoAsync(brokerAccountId, symbol);
            if (symbolInfo == null)
            {
                throw new Exception($"Binance LOT_SIZE bilgisi alınamadı: {symbol}");
            }

            var lotSizeFilter = symbolInfo.Filters.FirstOrDefault(f => f.FilterType == "LOT_SIZE");
            var minNotionalFilter = symbolInfo.Filters.FirstOrDefault(f => f.FilterType == "NOTIONAL");

            if (lotSizeFilter == null || (minNotionalFilter == null && !isSellOrder))
            {
                throw new Exception($"Binance LOT_SIZE veya MIN_NOTIONAL bilgisi eksik: {symbol}");
            }

            decimal minQty = decimal.Parse(lotSizeFilter.MinQty, CultureInfo.InvariantCulture);
            decimal stepSize = decimal.Parse(lotSizeFilter.StepSize, CultureInfo.InvariantCulture);
            decimal minNotional = minNotionalFilter != null ? decimal.Parse(minNotionalFilter.MinNotional, CultureInfo.InvariantCulture) : 0;

            decimal adjustedQuantity = requestedQuantity;

            // **SELL İşlemi: Elde ne varsa sat, min LOT_SIZE şartını sağla**
            if (isSellOrder)
            {
                adjustedQuantity = Math.Floor(requestedQuantity / stepSize) * stepSize;

                // **Eğer hesaplanan miktar minQty'den küçükse hata döndür**
                if (adjustedQuantity < minQty)
                {
                    throw new Exception($"[UYARI] Yetersiz bakiye: {requestedQuantity}. Minimum LOT_SIZE: {minQty}. Satış yapılamaz.");
                }

                return adjustedQuantity;
            }

            // **BUY İşlemi: LOT_SIZE ve NOTIONAL kontrolü yap**
            adjustedQuantity = Math.Floor(requestedQuantity / stepSize) * stepSize;
            decimal totalValue = adjustedQuantity * price;

            if (totalValue < minNotional)
            {
                adjustedQuantity = Math.Ceiling(minNotional / price / stepSize) * stepSize;
            }

            // **Eğer hesaplanan miktar 0 veya negatifse hata döndür**
            if (adjustedQuantity <= 0)
            {
                throw new Exception($"[UYARI] Geçersiz miktar: {adjustedQuantity}. LOT_SIZE: {stepSize}, MinNotional: {minNotional}");
            }

            return adjustedQuantity;
        }



        public async Task<BinancePosition?> GetCryptoPositionAsync(string symbol, Guid brokerAccountId)
        {
            var httpClient = await ConfigureHttpClientAsync(brokerAccountId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var queryString = $"timestamp={timestamp}&recvWindow=60000";
            var brokerAccount = await GetBinanceAccountAsync(brokerAccountId);
            if (brokerAccount == null) throw new Exception("Broker account not found");

            var signature = GenerateSignature(queryString, brokerAccount.ApiSecret);
            queryString += $"&signature={signature}";

            var response = await httpClient.GetAsync($"/api/v3/account?{queryString}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Binance API Hata: {response.StatusCode} - {responseContent}");
            }

            var accountData = JsonConvert.DeserializeObject<BinanceAccountResponse>(responseContent);
            if (accountData?.Balances == null)
                return null;

            var asset = accountData.Balances.FirstOrDefault(b => b.Asset == symbol.Replace("USDT", ""));
            if (asset == null || asset.Free <= 0)
                return null;

            return new BinancePosition
            {
                Symbol = symbol,
                Quantity = asset.Free
            };
        }
    }
}