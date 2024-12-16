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
        Task<IResult> ClosePositionAsync(string symbol, decimal quantity, Guid userId);
    }
}