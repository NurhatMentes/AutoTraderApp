using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.ExternalModels.Telegram
{
    public class TelegramUser : BaseEntity
    {
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string? ChatId { get; set; } 
        public string? ChannelId { get; set; }
    }
}
                                                                                      