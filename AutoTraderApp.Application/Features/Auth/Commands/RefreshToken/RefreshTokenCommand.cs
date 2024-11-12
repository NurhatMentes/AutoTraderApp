using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Auth.Models;
using AutoTraderApp.Application.Features.Common.AutoTraderApp.Application.Features.Common;
using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Security.JWT;
using AutoTraderApp.Core.Utilities.Results;
using MediatR;
using System.Security.Claims;

namespace AutoTraderApp.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<IDataResult<AccessToken>>
    {
        public string AccessToken { get; set; }
    }

    public class RefreshTokenCommandHandler : BaseRequestHandler<RefreshTokenCommand, IDataResult<AccessToken>>
    {
        private readonly ITokenHelper _tokenHelper;
        private readonly IUserRepository _userRepository;

        public RefreshTokenCommandHandler(
            ITokenHelper tokenHelper,
            IUserRepository userRepository)
        {
            _tokenHelper = tokenHelper;
            _userRepository = userRepository;
        }

        public override async Task<IDataResult<AccessToken>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _tokenHelper.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
                return new ErrorDataResult<AccessToken>(Messages.Auth.InvalidToken);

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return new ErrorDataResult<AccessToken>(Messages.Auth.InvalidToken);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new ErrorDataResult<AccessToken>(Messages.Auth.UserNotFound);

            var claims = await _userRepository.GetClaimsAsync(user);
            var newAccessToken = _tokenHelper.CreateToken(user, claims);

            return new SuccessDataResult<AccessToken>(newAccessToken, Messages.Auth.TokenCreated);
        }
    }
}
