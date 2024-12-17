using Microsoft.Playwright;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface ITradingViewAutomationService
    {
        Task<bool> CreateStrategyAsync(string strategyName, string symbol, string script, string webhookUrl, Guid userId);
        Task<bool> CreateAlertAsync(string strategyName, string webhookUrl, string action, string symbol, int quantity, decimal price, Guid brokerAccountId, Guid userId);
        Task<bool> LoginAsync(Guid userId, string password);
    }
}
