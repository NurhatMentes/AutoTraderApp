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
    public class CreateUserTradingSettingsCommand : IRequest<IResult>
    {
        public Guid UserId { get; set; }
        public Guid BrokerAccountId { get; set; } 
        public UserTradingSettingsCreateDto Settings { get; set; }
    }

    public class CreateUserTradingSettingsCommandHandler : IRequestHandler<CreateUserTradingSettingsCommand, IResult>
    {
        private readonly IBaseRepository<UserTradingSetting> _userTradingSettingsRepository;
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;

        public CreateUserTradingSettingsCommandHandler(
            IBaseRepository<UserTradingSetting> userTradingSettingsRepository,
            IBaseRepository<BrokerAccount> brokerAccountRepository)
        {
            _userTradingSettingsRepository = userTradingSettingsRepository;
            _brokerAccountRepository = brokerAccountRepository;
        }

        public async Task<IResult> Handle(CreateUserTradingSettingsCommand request, CancellationToken cancellationToken)
        {
            var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == request.BrokerAccountId && b.UserId == request.UserId);
            if (brokerAccount == null)
                return new ErrorResult("Seçilen broker hesabı bulunamadı.");

            var existingSettings = await _userTradingSettingsRepository.GetAsync(uts => uts.UserId == request.UserId && uts.BrokerAccountId == request.BrokerAccountId);
            if (existingSettings != null)
                return new ErrorResult($"Bu broker hesabı için zaten ayarlar mevcut.");

            var settings = new UserTradingSetting
            {
                UserId = request.UserId,
                BrokerAccountId = request.BrokerAccountId,
                BrokerType = brokerAccount.BrokerType,
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
