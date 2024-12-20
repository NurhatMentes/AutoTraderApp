using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.AlphaVantage;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface IAlphaVantageService
    {
        Task<decimal?> GetCurrentPrice(string symbol);
        Task<IEnumerable<Price>> GetHistoricalPrices(string symbol, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Price>> GetIntraday(string symbol, string interval = "1min");
        Task<List<GainerDto>> GetTopGainersAsync();
        Task<List<LoserDto>> GetTopLosersAsync();
        Task<List<ActiveStockDto>> GetMostActiveAsync();

    }
}
