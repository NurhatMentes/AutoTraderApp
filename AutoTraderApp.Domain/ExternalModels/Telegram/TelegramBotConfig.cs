using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.ExternalModels.Telegram
{
    public class TelegramBotConfig : BaseEntity
    {
        public string BotToken { get; set; }
        public string WebhookUrl { get; set; }
    }
}
