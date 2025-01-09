using AutoTraderApp.Domain.ExternalModels.Telegram;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface ITelegramBotService
    {
        Task SetWebhookAsync(string botToken, string webhookUrl);
        Task<string> GetWebhookInfoAsync();
        Task<bool> UpdateBotTokenAsync(string botToken);
        Task<bool> SendMessageAsync(string phoneNumber, string message);
        Task<bool> RegisterUserAsync(string chatId, string phoneNumber);
        Task<TelegramUser> GetUserByIdOrPhoneNumberAsync(Guid? userId, string? phoneNumber);
    }
}
