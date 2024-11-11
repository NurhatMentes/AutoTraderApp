using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Domain.Entities;

public class Alert : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? InstrumentId { get; set; }
    public Instrument? Instrument { get; set; }
    public AlertType Type { get; set; }
    public string Condition { get; set; }
    public decimal TriggerValue { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? LastTriggered { get; set; }
}