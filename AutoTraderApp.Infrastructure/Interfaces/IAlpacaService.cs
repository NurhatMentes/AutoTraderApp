using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using Position = AutoTraderApp.Domain.Entities.Position;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface IAlpacaService
    {
        Task<AccountInfo> GetAccountInfoAsync(string apiKey, string apiSecret, bool isPaper);
        Task<OrderResponse> PlaceOrderAsync(OrderRequest orderRequest);
        Task<OrderResponse> CancelOrderAsync(string orderId);
        Task<List<PositionResponse>> GetPositionsAsync();
        Task<List<Portfolio>> GetPortfolioAsync();
        Task<MarketDataResponse> GetMarketDataAsync(string symbol);
        Task<List<MarketDataResponse>> GetAllMarketDataAsync(int page = 1, int pageSize = 30);
        Task<IResult> ClosePositionAsync(string symbol, decimal quantity);
    }
}