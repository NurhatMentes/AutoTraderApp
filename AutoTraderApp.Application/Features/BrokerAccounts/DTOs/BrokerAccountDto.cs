namespace AutoTraderApp.Application.Features.BrokerAccounts.DTOs;

public class BrokerAccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public bool IsPaper { get; set; }
}