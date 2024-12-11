using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.TradingView;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface ITradingViewService
    {                           
        Task<bool> UploadStrategyAsync(string strategyName, string pineScriptCode);
        void UploadStrategyToTradingView(string script, string username, string password);
        Task<bool> DeleteStrategyAsync(string strategyName);
        Task<bool> SendStrategyAsync(TradingViewStrategy strategy);
    }
}
