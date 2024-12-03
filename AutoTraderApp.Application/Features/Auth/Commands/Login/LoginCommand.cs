using AutoMapper;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Application.Features.Auth.Models;
using AutoTraderApp.Application.Features.Common.AutoTraderApp.Application.Features.Common;
using AutoTraderApp.Core.Aspects.Autofac.Caching;
using AutoTraderApp.Core.Aspects.Autofac.Validation;
using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Security.Hashing;
using AutoTraderApp.Core.Security.JWT;
using AutoTraderApp.Core.Utilities.Results;             
using MediatR;

namespace AutoTraderApp.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<IDataResult<LoginResponse>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginCommandHandler : BaseRequestHandler<LoginCommand, IDataResult<LoginResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenHelper _tokenHelper;
        private readonly IMapper _mapper;

        public LoginCommandHandler(
            IUserRepository userRepository,
            ITokenHelper tokenHelper,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _tokenHelper = tokenHelper;
            _mapper = mapper;
        }

        [ValidationAspect(typeof(LoginCommandValidator))]
        [CacheAspect]
        public override async Task<IDataResult<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAsync(u => u.Email.Address == request.Email);
            if (user == null)
                return new ErrorDataResult<LoginResponse>(Messages.Auth.UserNotFound);

            if (!HashingHelper.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                return new ErrorDataResult<LoginResponse>(Messages.Auth.PasswordError);

            var claims = await _userRepository.GetClaimsAsync(user);

            var accessToken = _tokenHelper.CreateToken(user, claims);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                User = _mapper.Map<UserDto>(user)
            };

            return new SuccessDataResult<LoginResponse>(response, Messages.Auth.SuccessfulLogin);
        }
    }
}

