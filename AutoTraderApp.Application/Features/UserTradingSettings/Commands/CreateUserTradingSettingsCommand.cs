using AutoTraderApp.Application.Features.UserTradingSettings.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.UserTradingSettings.Commands
{
    public class CreateUserTradingSettingsCommand : IRequest<IResult>
    {
        public Guid UserId { get; set; }
        public UserTradingSettingsDto Settings { get; set; }
    }

    public class CreateUserTradingSettingsCommandHandler : IRequestHandler<CreateUserTradingSettingsCommand, IResult>
    {
        private readonly IBaseRepository<UserTradingSetting> _userTradingSettingsRepository;

        public CreateUserTradingSettingsCommandHandler(IBaseRepository<UserTradingSetting> userTradingSettingsRepository)
        {
            _userTradingSettingsRepository = userTradingSettingsRepository;
        }

        public async Task<IResult> Handle(CreateUserTradingSettingsCommand request, CancellationToken cancellationToken)
        {
            var existingSettings = await _userTradingSettingsRepository.GetAsync(uts => uts.UserId == request.UserId);
            if (existingSettings != null)
                return new ErrorResult("Bu kullanıcı için zaten ayarlar mevcut.");

            var settings = new UserTradingSetting
            {
                UserId = request.UserId,
                RiskPercentage = request.Settings.RiskPercentage,
                MaxRiskLimit = request.Settings.MaxRiskLimit,
                MinBuyQuantity = request.Settings.MinBuyQuantity,
                MaxBuyQuantity = request.Settings.MaxBuyQuantity,
                BuyPricePercentage = request.Settings.BuyPricePercentage,
                SellPricePercentage = request.Settings.SellPricePercentage
            };

            await _userTradingSettingsRepository.AddAsync(settings);
            return new SuccessResult("Kullanıcı ayarları başarıyla oluşturuldu.");
        }
    }
}
