namespace AutoTraderApp.Domain.Enums;

public enum OrderStatus
{
    Created,
    Pending,
    PartiallyFilled,
    Filled,
    Cancelled,
    Rejected,
    Expired
}