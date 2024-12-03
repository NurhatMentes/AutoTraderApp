using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface IMarketDataService
    {
        Task<decimal?> GetCurrentPrice(string symbol);
        Task<IEnumerable<Price>> GetHistoricalPrices(string symbol, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Price>> GetIntraday(string symbol, string interval = "1min");
    }
}
