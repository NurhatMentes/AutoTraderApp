using System.Text.Json;
using AutoTraderApp.Core.CrossCuttingConcerns.Caching;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.AlphaVantage;
using AutoTraderApp.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoTraderApp.Infrastructure.Services.MarketData;

public class AlphaVantageService : IAlphaVantageService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AlphaVantageService> _logger;
    private readonly ICacheManager _cacheManager;
    private readonly string _apiKey;

    public AlphaVantageService(
        IConfiguration configuration,
        ILogger<AlphaVantageService> logger,
        ICacheManager cacheManager)
    {
        _configuration = configuration;
        _logger = logger;
        _cacheManager = cacheManager;

        _apiKey = _configuration["AlphaVantage:ApiKey"]
                  ?? throw new ArgumentException("Alpha Vantage API key is missing");

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://www.alphavantage.co/")
        };
    }

    public async Task<string?> GetCurrentPrice(string symbol)
    {
        try
        {
            var url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var quote = JsonSerializer.Deserialize<JsonElement>(content);

            if (quote.TryGetProperty("Global Quote", out var globalQuote) &&
                globalQuote.TryGetProperty("05. price", out var priceElement))
            {
                var price = priceElement.GetString();
                return price;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching price for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<IEnumerable<Price>> GetHistoricalPrices(string symbol, DateTime startDate, DateTime endDate)
    {
        try
        {
            var url = $"query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_apiKey}&outputsize=full";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<AlphaVantageResponse<Dictionary<string, string>>>(content);

            var prices = new List<Price>();

            foreach (var kvp in data.TimeSeries)
            {
                if (DateTime.TryParse(kvp.Key, out var date) &&
                    date >= startDate && date <= endDate)
                {
                    prices.Add(new Price
                    {
                        Timestamp = date,
                        Open = decimal.Parse(kvp.Value["1. open"]),
                        High = decimal.Parse(kvp.Value["2. high"]),
                        Low = decimal.Parse(kvp.Value["3. low"]),
                        Close = decimal.Parse(kvp.Value["4. close"]),
                        Volume = decimal.Parse(kvp.Value["5. volume"])
                    });
                }
            }

            return prices.OrderByDescending(p => p.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching historical prices for {Symbol}", symbol);
            return Enumerable.Empty<Price>();
        }
    }

    public async Task<IEnumerable<Price>> GetIntraday(string symbol, string interval = "5min")
    {
        var cacheKey = $"intraday_{symbol}_{interval}";

        if (_cacheManager.IsAdd(cacheKey))
            return _cacheManager.Get<IEnumerable<Price>>(cacheKey);

        try
        {
            var url = $"query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval={interval}&apikey={_apiKey}&outputsize=compact";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonData = JsonSerializer.Deserialize<JsonElement>(content);

            if (!jsonData.TryGetProperty($"Time Series ({interval})", out var timeSeries))
            {
                _logger.LogError("Gün içi verileri alınamadı veya hatalı format.");
                return Enumerable.Empty<Price>();
            }

            var prices = new List<Price>();

            foreach (var dataPoint in timeSeries.EnumerateObject())
            {
                if (DateTime.TryParse(dataPoint.Name, out var timestamp))
                {
                    var entry = dataPoint.Value;

                    var price = new Price
                    {
                        Timestamp = timestamp,
                        Open = decimal.Parse(entry.GetProperty("1. open").GetString()),
                        High = decimal.Parse(entry.GetProperty("2. high").GetString()),
                        Low = decimal.Parse(entry.GetProperty("3. low").GetString()),
                        Close = decimal.Parse(entry.GetProperty("4. close").GetString()),
                        Volume = decimal.Parse(entry.GetProperty("5. volume").GetString())
                    };

                    prices.Add(price);
                }
            }

            _cacheManager.Add(cacheKey, prices, 1);
            return prices.OrderByDescending(p => p.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching intraday prices for {Symbol} with interval {Interval}", symbol, interval);
            return Enumerable.Empty<Price>();
        }
    }

    public async Task<List<GainerDto>> GetTopGainersAsync()
    {
        try
        {
            var url = $"query?function=TOP_GAINERS_LOSERS&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var jsonData = JsonSerializer.Deserialize<JsonElement>(content);

            if (jsonData.TryGetProperty("top_gainers", out var gainersArray))
            {
                var gainers = gainersArray.EnumerateArray()
                    .Select(gainer => new GainerDto
                    {
                        Ticker = gainer.GetProperty("ticker").GetString(),
                        Price = gainer.GetProperty("price").GetString(),
                        ChangeAmount = gainer.GetProperty("change_amount").GetString(),
                        ChangePercentage = gainer.GetProperty("change_percentage").GetString(),
                        Volume = long.Parse(gainer.GetProperty("volume").GetString())
                    })
                    .Take(15)
                    .ToList();

                return gainers;
            }

            _logger.LogError("Günlük en çok yükselen hisseler verisi bulunamadı.");
            throw new Exception("Günlük en çok yükselen hisseler verisi alınamadı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Günlük en çok yükselen hisseler verisi alınamadı.");
            throw new Exception("Günlük en çok yükselen hisseler verisi alınamadı.");
        }
    }

    public async Task<List<LoserDto>> GetTopLosersAsync()
    {
        try
        {
            var url = $"query?function=TOP_GAINERS_LOSERS&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var jsonData = JsonSerializer.Deserialize<JsonElement>(content);

            if (jsonData.TryGetProperty("top_losers", out var losersArray))
            {
                var losers = losersArray.EnumerateArray()
                    .Select(loser => new LoserDto
                    {
                        Ticker = loser.GetProperty("ticker").GetString(),
                        Price = loser.GetProperty("price").GetString(),
                        ChangeAmount = loser.GetProperty("change_amount").GetString(),
                        ChangePercentage = loser.GetProperty("change_percentage").GetString(),
                        Volume = long.Parse(loser.GetProperty("volume").GetString())
                    })
                    .Take(15)
                    .ToList();

                return losers;
            }

            _logger.LogError("Günlük en çok düşen hisseler verisi bulunamadı.");
            throw new Exception("Günlük en çok düşen hisseler verisi alınamadı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Günlük en çok düşen hisseler verisi alınamadı.");
            throw new Exception("Günlük en çok düşen hisseler verisi alınamadı.");
        }
    }

    public async Task<List<ActiveStockDto>> GetMostActiveAsync()
    {
        try
        {
            var url = $"query?function=TOP_GAINERS_LOSERS&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var jsonData = JsonSerializer.Deserialize<JsonElement>(content);

            if (jsonData.TryGetProperty("most_actively_traded", out var activeStocksArray))
            {
                var activeStocks = activeStocksArray.EnumerateArray()
                    .Select(activeStock => new ActiveStockDto
                    {
                        Ticker = activeStock.GetProperty("ticker").GetString(),
                        Price = activeStock.GetProperty("price").GetString(),
                        ChangeAmount = activeStock.GetProperty("change_amount").GetString(),
                        ChangePercentage = activeStock.GetProperty("change_percentage").GetString(),
                        Volume = long.Parse(activeStock.GetProperty("volume").GetString())
                    })
                    .Take(15)
                    .ToList();

                return activeStocks;
            }

            _logger.LogError("Hacim açısından en aktif hisseler verisi bulunamadı.");
            throw new Exception("Hacim açısından en aktif hisseler verisi alınamadı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hacim açısından en aktif hisseler verisi alınamadı.");
            throw new Exception("Hacim açısından en aktif hisseler verisi alınamadı.");
        }
    }

}