using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.Telegram
{
    public class TelegramMessage
    {
        public TelegramChat Chat { get; set; }
        public string Text { get; set; }
        public TelegramContact Contact { get; set; }
    }
}
