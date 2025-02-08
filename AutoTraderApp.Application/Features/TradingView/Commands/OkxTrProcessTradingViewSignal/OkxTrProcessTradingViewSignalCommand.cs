using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Utilities.Calculators;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
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

                var userTradingSettings = await _userTradingSetting.GetAsync(uts => uts.UserId == signal.UserId && uts.BrokerAccountId == signal.BrokerAccountId);
                if (userTradingSettings == null)
                {
                    return new ErrorResult("Kullanıcı ticaret ayarları bulunamadı.");
                }

                decimal cryptoPrice = await _okxService.GetMarketPriceAsync(signal.Symbol, brokerAccount.Id);
                decimal quantity;
                bool twoSell = false;
                if (signal.Action.ToUpper() == "SELL")
                {
                    var openOrders = await _okxService.GetOpenOrdersAsync(brokerAccount.Id, signal.Symbol);
                    if (openOrders.Count > 0)
                    {
                        foreach (var orderId in openOrders)
                        {
                            bool isCancelled = await _okxService.CancelOrderAsync(brokerAccount.Id, orderId, signal.Symbol);
                            if (isCancelled)
                            {
                                await _telegramBotService.SendMessageAsync(signal.UserId.ToString(), $"🛑 OKX Açık Emir İptal Edildi: {orderId}");
                            }
                            else
                            {
                                await _telegramBotService.SendMessageAsync(signal.UserId.ToString(), $"⚠️ OKX Emir İptali Başarısız: {orderId}");
                            }
                        }
                    }

                    await Task.Delay(1*00); 

                    quantity = await _okxService.GetCryptoPositionAsync(signal.Symbol, brokerAccount.Id);
                    if (quantity <= 0)
                    {
                        return new ErrorResult($"Satış için yeterli {signal.Symbol} bakiyesi yok.");
                    }

                    quantity = await _okxService.AdjustQuantityForOkx(signal.Symbol, quantity, brokerAccount.Id);

                    // **Büyük Satışları Bölme**
                    decimal firstSellQuantity = quantity / 2;
                    decimal secondSellQuantity = quantity - firstSellQuantity;

                    bool firstSellResult = await _okxService.PlaceOrderAsync(brokerAccount.Id, signal.Symbol, firstSellQuantity, "sell", signal.IsMarginTrade);
                    if (!firstSellResult)
                    {
                        return new ErrorResult($"OKX (TR) - {signal.Symbol} için ilk satış emri başarısız.");
                    }

                    await Task.Delay(2-00);

                    // ** Güncellenmiş Pozisyonu Kontrol Et**
                    decimal remainingQuantity = await _okxService.GetCryptoPositionAsync(signal.Symbol, brokerAccount.Id);
                    if (remainingQuantity > 0)
                    {
                        bool secondSellResult = await _okxService.PlaceOrderAsync(brokerAccount.Id, signal.Symbol, secondSellQuantity, "sell", signal.IsMarginTrade);
                        if (!secondSellResult)
                        {
                            return new ErrorResult($"OKX (TR) - {signal.Symbol} için ikinci satış emri başarısız.");
                        }
                        twoSell = true;
                    }
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

                await _telegramBotService.SendMessageAsync(signal.UserId.ToString(),
                    $"OKX (TR) {signal.Action.ToUpper()} SİNYALİ {signal.Symbol}, Miktar: {quantity}, Fiyat: {cryptoPrice}");

               
                if (twoSell == false)
                {
                    bool orderResult = await _okxService.PlaceOrderAsync(brokerAccount.Id, signal.Symbol, quantity,
                    signal.Action.ToUpper() == "SELL" ? "sell" : "buy", signal.IsMarginTrade);
                    if (!orderResult)
                    {
                        await _okxService.OkxLog(signal.BrokerAccountId, signal.Symbol, signal.Action, cryptoPrice, Convert.ToInt32(quantity), "OKX (TR) " + Messages.Trading.OrderFailed);
                        return new ErrorResult($"OKX (TR) - {signal.Action.ToUpper()} emri başarısız.");
                    }
                }

                await _okxService.OkxLog(signal.BrokerAccountId, signal.Symbol, signal.Action, cryptoPrice, Convert.ToInt32(quantity), "OKX (TR) " + Messages.General.Success);

                // **Trailing Stop**
                if (signal.Action.ToUpper() == "BUY")
                {
                    await Task.Delay(500);
                    decimal trailingStopRate = userTradingSettings.SellPricePercentage;

                    bool trailingStopResult = await _okxService.PlaceTrailingStopOrderAsync(
                        brokerAccount.Id,
                        signal.Symbol,
                        quantity,
                        trailingStopRate
                    );

                    if (!trailingStopResult)
                    {
                        await _okxService.OkxLog(signal.BrokerAccountId, signal.Symbol, "Trailing Stop", cryptoPrice, Convert.ToInt32(quantity), "OKX (TR) Trailing Stop başarısız.");
                        return new ErrorResult($"OKX (TR) - {signal.Symbol} için Trailing Stop emri başarısız.");
                    }

                    await _telegramBotService.SendMessageAsync(signal.UserId.ToString(),
                        $"📉 OKX (TR) {signal.Symbol} için Trailing Stop emri başarıyla oluşturuldu! 🎯 Stop Loss: %{trailingStopRate}");
                }

                return new SuccessResult($"OKX (TR) - {signal.Action.ToUpper()} işlemi başarıyla tamamlandı.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"OKX işlem hatası: {ex.Message}");
            }
        }

    }
}
