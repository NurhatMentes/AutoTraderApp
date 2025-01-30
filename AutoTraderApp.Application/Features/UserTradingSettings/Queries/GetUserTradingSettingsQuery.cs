using AutoTraderApp.Application.Features.UserTradingSettings.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.UserTradingSettings.Queries
{
    public class GetUserTradingSettingsQuery : IRequest<UserTradingSettingsDto>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserTradingSettingsQueryHandler : IRequestHandler<GetUserTradingSettingsQuery, UserTradingSettingsDto>
    {
        private readonly IBaseRepository<UserTradingSetting> _userTradingSettingsRepository;

        public GetUserTradingSettingsQueryHandler(IBaseRepository<UserTradingSetting> userTradingSettingsRepository)
        {
            _userTradingSettingsRepository = userTradingSettingsRepository;
        }

        public async Task<UserTradingSettingsDto> Handle(GetUserTradingSettingsQuery request, CancellationToken cancellationToken)
        {
            var settings = await _userTradingSettingsRepository.GetAsync(uts => uts.UserId == request.UserId);
            if (settings == null)
                return null;

            return new UserTradingSettingsDto
            {
                UserName = settings.User.FirstName + " " + settings.User.LastName,
                BrokerType = settings.BrokerType,
                RiskPercentage = settings.RiskPercentage,
                MaxRiskLimit = settings.MaxRiskLimit,
                MinBuyQuantity = settings.MinBuyQuantity,
                MaxBuyQuantity = settings.MaxBuyQuantity,
                BuyPricePercentage = settings.BuyPricePercentage,
                SellPricePercentage = settings.SellPricePercentage
            };
        }
    }
}
