using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Core.Utilities.Repositories;
using MediatR;
using AutoTraderApp.Core.Security.Hashing;

namespace AutoTraderApp.Application.Features.UserTradingAccounts.Commands.CreateUserTradingAccount;

public class CreateUserTradingAccountCommand : IRequest<IResult>
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class CreateUserTradingAccountCommandHandler : IRequestHandler<CreateUserTradingAccountCommand, IResult>
{
    private readonly IBaseRepository<UserTradingAccount> _repository;

    public CreateUserTradingAccountCommandHandler(IBaseRepository<UserTradingAccount> repository)
    {
        _repository = repository;
    }

    public async Task<IResult> Handle(CreateUserTradingAccountCommand request, CancellationToken cancellationToken)
    {
        HashingHelper.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var newAccount = new UserTradingAccount
        {
            UserId = request.UserId,
            Email = request.Email,
            EncryptedPassword = Convert.ToBase64String(passwordHash),
            PasswordSalt = Convert.ToBase64String(passwordSalt), 
            TwoFactorExpiry = null
        };

        await _repository.AddAsync(newAccount);
        return new SuccessResult("TradingView hesabı kaydedildi.");
    }
}
