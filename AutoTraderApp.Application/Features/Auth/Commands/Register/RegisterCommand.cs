using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Common.AutoTraderApp.Application.Features.Common;
using AutoTraderApp.Core.Aspects.Autofac.Validation;
using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Security.Hashing;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ValueObjects;
using MediatR;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<IResult>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class RegisterCommandHandler : BaseRequestHandler<RegisterCommand, IResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IOperationClaimRepository _operationClaimRepository;
        private readonly IUserOperationClaimRepository _userOperationClaimRepository;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IOperationClaimRepository operationClaimRepository,
            IUserOperationClaimRepository userOperationClaimRepository)
        {
            _userRepository = userRepository;
            _operationClaimRepository = operationClaimRepository;
            _userOperationClaimRepository = userOperationClaimRepository;
        }

        [ValidationAspect(typeof(RegisterCommandValidator))]
        public override async Task<IResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var userExists = await _userRepository.GetAsync(u => u.Email.Address == request.Email);
            if (userExists != null)
                return new ErrorResult(Messages.Auth.UserAlreadyExists);

            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(request.Password, out passwordHash, out passwordSalt);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = new Email(request.Email),
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = AccountStatus.Inactive,
                UserName = request.Email 
            };

            await _userRepository.AddAsync(user);

            var defaultClaim = await _operationClaimRepository.GetAsync(oc => oc.Name == "User");
            if (defaultClaim == null)
            {
                defaultClaim = new OperationClaim { Name = "User" };
                await _operationClaimRepository.AddAsync(defaultClaim);
            }

            var userOperationClaim = new UserOperationClaim
            {
                UserId = user.Id,
                OperationClaimId = defaultClaim.Id
            };

            await _userOperationClaimRepository.AddAsync(userOperationClaim);

            return new SuccessResult(Messages.Auth.UserRegistered);
        }
    }
}
