using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Utilities.Calculators;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.TradingView.Commands.CryptoProcessTradingViewSignal
{
    public class CryptoProcessTradingViewSignalCommand : IRequest<IResult>
    {
        public TradingViewCryptoSignalDto Signal { get; set; } = null!;
    }

    public class CryptoProcessTradingViewSignalCommandHandler : IRequestHandler<CryptoProcessTradingViewSignalCommand, IResult>
    {
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly IBinanceService _binanceService;
        private readonly ITelegramBotService _telegramBotService;
        IBaseRepository<UserTradingSetting> _userTradingSetting;

        public CryptoProcessTradingViewSignalCommandHandler(
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IBinanceService binanceService,
            ITelegramBotService telegramBotService,
            IBaseRepository<UserTradingSetting> userTradingSetting)
        {
            _brokerAccountRepository = brokerAccountRepository;
            _binanceService = binanceService;
            _telegramBotService = telegramBotService;
            _userTradingSetting = userTradingSetting;
        }

        public async Task<IResult> Handle(CryptoProcessTradingViewSignalCommand request, CancellationToken cancellationToken)
        {
            var signal = request.Signal;

            try
            {
                // Broker hesabını kontrol et
                var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == signal.BrokerAccountId && b.UserId == signal.UserId);
                if (brokerAccount == null || brokerAccount.BrokerName != "Binance")
                    return new ErrorResult(Messages.Trading.InvalidBrokerAccount);

                // Kullanıcının trading ayarlarını getir
                var userTradingSettings = await _userTradingSetting.GetAsync(uts => uts.UserId == signal.UserId);
                if (userTradingSettings == null)
                    return new ErrorResult(Messages.General.DataNotFound);

                // Binance fiyatını getir**
                var latestPrice = await _binanceService.GetMarketPriceAsync(signal.Symbol, brokerAccount.Id);
                if (latestPrice <= 0)
                    return new ErrorResult(Messages.Trading.PriceNotFound);

                // Kullanıcının Binance hesap bakiyesini getir*
                var accountBalance = await _binanceService.GetAccountBalanceAsync(brokerAccount.Id);
                if (accountBalance <= 0)
                    return new ErrorResult(Messages.Trading.InsufficientBalance);

                // Kullanıcının risk oranına göre otomatik işlem miktarını belirle**
                var calculatedQuantity = QuantityCalculator.CalculateCryptoQuantity(
                    accountBalance,
                    userTradingSettings.RiskPercentage,
                    latestPrice,
                    userTradingSettings.MaxRiskLimit
                );

                // Eğer hesaplanan miktar sıfır veya geçersizse hata döndür
                if (calculatedQuantity <= 0)
                    return new ErrorResult(Messages.Trading.InvalidAmount);

                var orderResult = await _binanceService.PlaceOrderAsync(
                    brokerAccount.Id,
                    signal.Symbol,
                    calculatedQuantity, 
                    signal.Action,
                    signal.IsMarginTrade
                );

                if (!orderResult)
                    return new ErrorResult(Messages.Trading.OrderFailed);

                // Telegram bildirimi gönder
                await _telegramBotService.SendMessageAsync(signal.UserId.ToString(),
                    $"Başarılı işlem: {signal.Action} {signal.Symbol}, Miktar: {calculatedQuantity} {(signal.IsMarginTrade ? "(Margin)" : "(Spot)")}");

                return new SuccessResult(Messages.General.Success);
            }
            catch (Exception ex)
            {
                return new ErrorResult($"{Messages.General.SystemError}: {ex.Message}");
            }
        }

    }
}
