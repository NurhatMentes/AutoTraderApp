using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities;

public class UserOperationClaim : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid OperationClaimId { get; set; }
    public OperationClaim OperationClaim { get; set; }

}