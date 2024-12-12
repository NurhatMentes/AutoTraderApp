using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.ExternalModels.TradingView;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface ITradingViewService
    {
        Task<bool> UploadStrategyAsync(string strategyName, string pineScriptCode);
        Task<bool> DeleteStrategyAsync(string strategyName);
    }
}
