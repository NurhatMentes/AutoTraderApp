using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Utilities.Calculators;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using AutoTraderApp.Infrastructure.Services.Binance;
using MediatR;

namespace AutoTraderApp.Application.Features.TradingView.Commands.OkxTrProcessTradingViewSignal
{
    public class OkxTrProcessTradingViewSignalCommand : IRequest<IResult>
    {
        public TradingViewCryptoSignalDto Signal { get; set; } = null!;
    }

    public class OkxTrProcessTradingViewSignalCommandHandler : IRequestHandler<OkxTrProcessTradingViewSignalCommand, IResult>
    {
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly IOkxService _okxService;
        private readonly ITelegramBotService _telegramBotService;
        private readonly IBaseRepository<UserTradingSetting> _userTradingSetting;

        public OkxTrProcessTradingViewSignalCommandHandler(
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IOkxService okxService,
            ITelegramBotService telegramBotService,
            IBaseRepository<UserTradingSetting> userTradingSetting)
        {
            _brokerAccountRepository = brokerAccountRepository;
            _okxService = okxService;
            _telegramBotService = telegramBotService;
            _userTradingSetting = userTradingSetting;
        }

        public async Task<IResult> Handle(OkxTrProcessTradingViewSignalCommand request, CancellationToken cancellationToken)
        {
            var signal = request.Signal;

            try
            {
                var brokerAccount = await _brokerAccountRepository.GetAsync(b =>
                    b.Id == signal.BrokerAccountId &&
                    (b.BrokerName == "OKX (TR)" || b.BrokerName == "OKX (TR) Test"));

                if (brokerAccount == null)
                    return new ErrorResult("OKX hesabı bulunamadı.");

                decimal accountBalance = await _okxService.GetAccountBalanceAsync(brokerAccount.Id, "USDT");
                if (accountBalance <= 0)
                {
                    return new ErrorResult("Hesap bakiyesi yetersiz.");
                }

                var userTradingSettings = await _userTradingSetting.GetAsync(uts => uts.UserId == signal.UserId);
                if (userTradingSettings == null)
                {
                    return new ErrorResult("Kullanıcı ticaret ayarları bulunamadı.");
                }

                decimal cryptoPrice = await _okxService.GetMarketPriceAsync(signal.Symbol, brokerAccount.Id);

                decimal quantity;
                if (signal.Action.ToUpper() == "SELL")
                {
                    quantity = await _okxService.GetCryptoPositionAsync(signal.Symbol, brokerAccount.Id);

                    if (quantity <= 0)
                    {
                        await _okxService.OkxLog(signal.BrokerAccountId, signal.Symbol, signal.Action, null, null, "OKX (TR) " + Messages.Trading.InsufficientBalance);
                        return new ErrorResult($"Satış için yeterli {signal.Symbol} bakiyesi yok.");
                    }

                    quantity = await _okxService.AdjustQuantityForOkx(signal.Symbol, quantity, brokerAccount.Id);
                }

                else
                {
                    quantity = QuantityCalculator.CalculateCryptoQuantity(
                        accountBalance,
                        userTradingSettings.RiskPercentage,
                        cryptoPrice,
                        userTradingSettings.MaxRiskLimit,
                        userTradingSettings.MaxBuyQuantity,
                        userTradingSettings.MinBuyPrice
                    );
                }

                // **Telegram botuna log gönder**
                await _telegramBotService.SendMessageAsync(signal.UserId.ToString(),
                    $"OKX (TR) {signal.Action.ToUpper()} SİNYALİ {signal.Symbol}, Miktar: {quantity}, Fiyat: {cryptoPrice}");

                // **Doğru `side` parametresini kullanarak işlemi gerçekleştir**
                bool orderResult = await _okxService.PlaceOrderAsync(brokerAccount.Id, signal.Symbol, quantity,
                    signal.Action.ToUpper() == "SELL" ? "sell" : "buy", signal.IsMarginTrade);

                if (!orderResult)
                {
                    await _okxService.OkxLog(signal.BrokerAccountId, signal.Symbol, signal.Action, cryptoPrice, Convert.ToInt32(quantity), "OKX (TR) " + Messages.Trading.OrderFailed);
                    return new ErrorResult($"OKX (TR) - {signal.Action.ToUpper()} emri başarısız.");
                }

                await _okxService.OkxLog(signal.BrokerAccountId, signal.Symbol, signal.Action, cryptoPrice, Convert.ToInt32(quantity), "OKX (TR) " + Messages.General.Success);
                return new SuccessResult($"OKX (TR) - {signal.Action.ToUpper()} işlemi başarıyla tamamlandı.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"OKX işlem hatası: {ex.Message}");
            }
        }

    }
}
