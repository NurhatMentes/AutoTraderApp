using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.Telegram
{
    public class TelegramUpdate
    {
         public TelegramMessage Message { get; set; }

    }
}
