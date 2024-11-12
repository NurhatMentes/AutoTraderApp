using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Persistence.Context;

namespace AutoTraderApp.Persistence.Repositories;

public class UserOperationClaimRepository : BaseRepository<UserOperationClaim>, IUserOperationClaimRepository
{
    public UserOperationClaimRepository(AutoTraderAppDbContext context) : base(context)
    {
    }
}