using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Domain.Entities;

public class BrokerAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public BrokerType Type { get; set; }
    public string Name { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string? ApiPassphrase { get; set; }  
    public bool IsPaperTrading { get; set; }    
    public AccountStatus Status { get; set; }
    public decimal Balance { get; set; }

  
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Position> Positions { get; set; } = new List<Position>();
}