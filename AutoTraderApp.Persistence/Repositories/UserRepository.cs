using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AutoTraderApp.Persistence.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AutoTraderAppDbContext context) : base(context)
    {
    }

    public async Task<List<OperationClaim>> GetClaimsAsync(User user)
    {
        var result = from operationClaim in _context.OperationClaims
            join userOperationClaim in _context.UserOperationClaims
                on operationClaim.Id equals userOperationClaim.OperationClaimId
            where userOperationClaim.UserId == user.Id
            select new OperationClaim { Id = operationClaim.Id, Name = operationClaim.Name };
        return await result.ToListAsync();
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.Address == email);
    }

    public async Task<bool> AddUserClaimAsync(UserOperationClaim userOperationClaim)
    {
        await _context.UserOperationClaims.AddAsync(userOperationClaim);
        return await _context.SaveChangesAsync() > 0;
    }
}