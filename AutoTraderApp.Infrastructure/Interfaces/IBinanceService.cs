using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Binance;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface IBinanceService
    {
        Task<decimal> GetAccountBalanceAsync(Guid brokerAccountId);
        Task<decimal> GetMarketPriceAsync(string symbol, Guid brokerAccountId);
        Task<BrokerAccount?> GetBinanceAccountAsync(Guid brokerAccountId);
        Task<object> GetAccountInfoAsync(Guid brokerAccountId);
        Task<Dictionary<string, decimal>> GetTotalPortfolioValueAsync(Guid brokerAccountId);
        Task<decimal> GetSpotBalanceAsync(Guid brokerAccountId);
        Task<decimal> GetFundingBalanceAsync(Guid brokerAccountId);
        Task<decimal> GetCrossMarginBalanceAsync(Guid brokerAccountId);
        Task<decimal> GetIsolatedMarginBalanceAsync(Guid brokerAccountId);
        Task<decimal> GetTotalBalanceAsync(Guid brokerAccountId);
        Task<bool> PlaceOrderAsync(Guid brokerAccountId, string symbol, decimal quantity, string action, bool isMarginTrade = false);
        Task<decimal> GetMinOrderSizeAsync(Guid brokerAccountId, string symbol);
        Task<string> GetAllOrdersBySymbolAsync(Guid brokerAccountId, string symbol);
        Task<string> GetTradeHistoryAsync(Guid brokerAccountId, string symbol);
        Task<string> PlaceMarginBuyOrderAsync(Guid brokerAccountId, string symbol, decimal quantity);
        Task<string> BorrowAssetAsync(Guid brokerAccountId, string asset, decimal amount);
        Task<string> PlaceMarginSellOrderAsync(Guid brokerAccountId, string symbol, decimal quantity);
        Task<string> RepayBorrowedAssetAsync(Guid brokerAccountId, string asset, decimal amount);
        Task<long?> GetBinanceUIDAsync(Guid brokerAccountId);
        Task<bool> ValidateUserByUIDAsync(Guid brokerAccountId, long expectedUID);
        Task<List<MarginOrderResponse>> GetMarginOrdersAsync(Guid brokerAccountId);
        Task<MarginBalanceResponse> GetMarginBalanceAsync(Guid brokerAccountId);
    }

}
