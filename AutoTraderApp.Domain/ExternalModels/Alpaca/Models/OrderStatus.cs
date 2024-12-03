namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

public enum OrderStatus
{
    New,
    PartiallyFilled,
    Filled,
    DoneForDay,
    Canceled,
    Expired,
    Replaced,
    PendingCancel,
    PendingReplace,
    Rejected,
    PendingNew,
    Unknown
}