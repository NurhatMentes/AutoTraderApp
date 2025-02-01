using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Binance;
using System;

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
        Task<bool> PlaceStopLossOrderAsync(Guid brokerAccountId, string symbol, decimal quantity, decimal stopLossPrice);
        Task<decimal> AdjustPriceForBinance(string symbol, decimal stopLossPrice, decimal currentPrice, Guid brokerAccountId);
        Task<decimal> GetMinOrderSizeAsync(Guid brokerAccountId, string symbol);
        Task<bool> CheckExistingStopLossOrderAsync(Guid brokerAccountId, string symbol);
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
        Task<BinanceSymbolInfo> GetExchangeInfoAsync(Guid brokerAccountId, string symbol);
        Task<decimal> AdjustQuantityForBinance(string symbol, decimal requestedQuantity, decimal price, Guid brokerAccountId, bool isSellOrder);
        Task<BinancePosition?> GetCryptoPositionAsync(string symbol, Guid brokerAccountId);
        Task<bool> BinanceLog(Guid brokerAccountId, string symbol, string? Action, decimal? price, int? quantity, string msg);
    }

}
