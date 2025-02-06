using AutoTraderApp.Application.Features.UserTradingSettings.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.UserTradingSettings.Commands
{
    public class UpdateUserTradingSettingsCommand : IRequest<IResult>
    {
        public Guid UserId { get; set; }
        public Guid BrokerAccountId { get; set; }
        public UserTradingSettingsCreateDto Settings { get; set; }
    }

    public class UpdateUserTradingSettingsCommandHandler : IRequestHandler<UpdateUserTradingSettingsCommand, IResult>
    {
        private readonly IBaseRepository<UserTradingSetting> _userTradingSettingsRepository;

        public UpdateUserTradingSettingsCommandHandler(IBaseRepository<UserTradingSetting> userTradingSettingsRepository)
        {
            _userTradingSettingsRepository = userTradingSettingsRepository;
        }

        public async Task<IResult> Handle(UpdateUserTradingSettingsCommand request, CancellationToken cancellationToken)
        {
            var settings = await _userTradingSettingsRepository.GetAsync(uts => uts.UserId == request.UserId && uts.BrokerAccountId == request.BrokerAccountId);
            if (settings == null)
                return new ErrorResult("Kullanıcı ayarları bulunamadı.");

            settings.RiskPercentage = request.Settings.RiskPercentage;
            settings.MaxRiskLimit = request.Settings.MaxRiskLimit;
            settings.MinBuyQuantity = request.Settings.MinBuyQuantity;
            settings.MaxBuyQuantity = request.Settings.MaxBuyQuantity;
            settings.BuyPricePercentage = request.Settings.BuyPricePercentage;
            settings.SellPricePercentage = request.Settings.SellPricePercentage;

            await _userTradingSettingsRepository.UpdateAsync(settings);
            return new SuccessResult("Kullanıcı ayarları başarıyla güncellendi.");
        }
    }
}
