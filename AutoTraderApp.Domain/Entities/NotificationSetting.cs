using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Domain.Entities;

public class NotificationSetting : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string? TelegramChatId { get; set; }
    public bool IsEnabled { get; set; }
    public NotificationPriority Priority { get; set; }
}