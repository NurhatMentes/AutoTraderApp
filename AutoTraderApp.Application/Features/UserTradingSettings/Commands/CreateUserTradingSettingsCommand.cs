using AutoTraderApp.Application.Features.UserTradingSettings.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace AutoTraderApp.Application.Features.UserTradingSettings.Commands
{
    public class CreateUserTradingSettingsCommand : IRequest<IResult>
    {
        public Guid UserId { get; set; }
        public string BrokerType { get; set; }
        public UserTradingSettingsDto Settings { get; set; }
    }

    public class CreateUserTradingSettingsCommandHandler : IRequestHandler<CreateUserTradingSettingsCommand, IResult>
    {
        private readonly IBaseRepository<UserTradingSetting> _userTradingSettingsRepository;
        private readonly IBaseRepository<BrokerAccount> _brokerAccount;

        public CreateUserTradingSettingsCommandHandler(IBaseRepository<UserTradingSetting> userTradingSettingsRepository, IBaseRepository<BrokerAccount> brokerAccount)
        {
            _userTradingSettingsRepository = userTradingSettingsRepository;
            _brokerAccount = brokerAccount;
        }

        public async Task<IResult> Handle(CreateUserTradingSettingsCommand request, CancellationToken cancellationToken)
        {
            var brokerAccount = await _brokerAccount.GetAsync(b => b.UserId == request.UserId);
            var existingSettings = await _userTradingSettingsRepository.GetAsync(uts => uts.UserId == request.UserId && uts.BrokerType == request.BrokerType);
            if (existingSettings != null)
                return new ErrorResult($"Bu kullanıcı için {request.BrokerType.ToLower()} hesap türünde zaten ayarlar mevcut.");

            var settings = new UserTradingSetting
            {
                UserId = request.UserId,
                BrokerType = request.BrokerType,
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
