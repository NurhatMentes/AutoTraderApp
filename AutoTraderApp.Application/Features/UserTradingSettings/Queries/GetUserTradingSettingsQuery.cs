using AutoTraderApp.Application.Features.UserTradingSettings.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.UserTradingSettings.Queries
{
    public class GetUserTradingSettingsQuery : IRequest<IEnumerable<UserTradingSettingsDto>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserTradingSettingsQueryHandler : IRequestHandler<GetUserTradingSettingsQuery, IEnumerable<UserTradingSettingsDto>>
    {
        private readonly IBaseRepository<UserTradingSetting> _userTradingSettingsRepository;

        public GetUserTradingSettingsQueryHandler(IBaseRepository<UserTradingSetting> userTradingSettingsRepository)
        {
            _userTradingSettingsRepository = userTradingSettingsRepository;
        }

        public async Task<IEnumerable<UserTradingSettingsDto>> Handle(GetUserTradingSettingsQuery request, CancellationToken cancellationToken)
        {
            var settingsList = await _userTradingSettingsRepository.GetListWithExpressionIncludeAsync(
                uts => uts.UserId == request.UserId,
                includes: new List<Expression<Func<UserTradingSetting, object>>> { uts => uts.BrokerAccount, uts => uts.User });

            return settingsList.Select(settings => new UserTradingSettingsDto
            {
                UserName = settings.User.FirstName + " " + settings.User.LastName,
                BrokerName = settings.BrokerAccount != null ? settings.BrokerAccount.BrokerName : "Broker mevcut değil", 
                RiskPercentage = settings.RiskPercentage,
                MaxRiskLimit = settings.MaxRiskLimit,
                MinBuyQuantity = settings.MinBuyQuantity,
                MaxBuyQuantity = settings.MaxBuyQuantity,
                BuyPricePercentage = settings.BuyPricePercentage,
                SellPricePercentage = settings.SellPricePercentage
            }).ToList();
        }
    }
}
