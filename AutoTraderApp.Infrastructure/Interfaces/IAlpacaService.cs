using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface IAlpacaService
    {
        Task<AccountInfo> GetAccountInfoAsync(Guid brokerAccountId);
        Task<OrderResponse> PlaceOrderAsync(Guid brokerAccountId, OrderRequest orderReques);
        Task<OrderResponse> CancelOrderAsync(string orderId, Guid brokerAccountId);
        Task<List<PositionResponse>> GetPositionsAsync(Guid brokerAccountId);
        Task<List<Portfolio>> GetPortfolioAsync(Guid brokerAccountId);
        Task<IResult> ClosePositionAsync(string symbol, decimal? quantity, Guid brokerAccountId);
        Task<List<OrderResponse>> GetFilledOrdersAsync(Guid brokerAccountId, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, string>> CalculateDailyPnL(List<OrderResponse> orders);
        Task<IResult> SellLossMakingPositionsAsync(Guid brokerAccountId, decimal lossThresholdPercentage = -5);
        Task<IResult> SellAllPositionsAtEndOfDayAsync(Guid brokerAccountId);
        Task<PositionResponse> GetPositionBySymbolAsync(string symbol, Guid brokerAccountId);
        Task<IResult> ClosePartialPositionAsync(string symbol, decimal quantity, Guid brokerAccountId);
        Task<AssetDetails> GetAssetDetailsAsync(string symbol, Guid brokerAccountId);
        Task<OrderResponse[]> GetAllOrdersAsync(Guid brokerAccountId);
        Task<decimal> GetLatestPriceAsync(string symbol, Guid brokerAccountId);
        public Task<bool> AlpacaLog(Guid brokerAccountId, string symbol, string? Action, decimal? price, int? quantity, string msg);
        Task<IResult> CloseAllPositionAsync(Guid brokerAccountId);
        Task<string> GenerateDailyTradeReportAsync(Guid brokerAccountId, DateTime tradeDate);
        Task<IResult> ClosePositionAsync(Guid brokerAccountId, string symbol);
    }
}