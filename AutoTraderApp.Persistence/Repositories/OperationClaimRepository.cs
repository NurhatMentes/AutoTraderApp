using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Persistence.Context;

namespace AutoTraderApp.Persistence.Repositories;

public class OperationClaimRepository : BaseRepository<OperationClaim>, IOperationClaimRepository
{
    public OperationClaimRepository(AutoTraderAppDbContext context) : base(context)
    {
    }
}