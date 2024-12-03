namespace AutoTraderApp.Application.Features.BrokerAccounts.DTOs;

public class AddBrokerAccountDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public bool IsPaper { get; set; }
}