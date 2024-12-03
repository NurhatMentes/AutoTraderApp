using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Core.Utilities.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<List<OperationClaim>> GetClaimsAsync(User user);
    Task<User> GetByEmailAsync(string email);
    Task<bool> AddUserClaimAsync(UserOperationClaim userOperationClaim);
}