namespace AutoTraderApp.Application.Features.BrokerAccounts.DTOs;

public class AccountInfoDto
{
    public string AccountId { get; set; } = "N/A";
    public decimal Cash { get; set; }
    public decimal BuyingPower { get; set; }
    public decimal PortfolioValue { get; set; }
    public bool IsPaper { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "UNKNOWN";
}