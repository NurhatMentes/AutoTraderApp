using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface IOkxService
    {
        Task<decimal> GetMarketPriceAsync(string symbol, Guid brokerAccountId);
        Task<bool> PlaceOrderAsync(Guid brokerAccountId, string symbol, decimal quantity, string orderType, bool isMarginTrade);
        Task<bool> CancelOrderAsync(Guid brokerAccountId, string orderId, string symbol);
        Task<decimal> GetAccountBalanceAsync(Guid brokerAccountId, string currency);
        Task<object> GetAccountInfoAsync(Guid brokerAccountId);
        Task<List<object>> GetActiveOrdersAsync(Guid brokerAccountId);
        Task<decimal> GetCryptoPositionAsync(string symbol, Guid brokerAccountId);
        Task<decimal> AdjustQuantityForOkx(string symbol, decimal quantity, Guid brokerAccountId);
        Task<bool> OkxLog(Guid brokerAccountId, string symbol, string? Action, decimal? price, int? quantity, string msg);
        Task<bool> PlaceTrailingStopOrderAsync(Guid brokerAccountId, string symbol, decimal quantity, decimal callbackRate);
        Task<List<string>> GetOpenOrdersAsync(Guid brokerAccountId, string symbol);
    }
}
