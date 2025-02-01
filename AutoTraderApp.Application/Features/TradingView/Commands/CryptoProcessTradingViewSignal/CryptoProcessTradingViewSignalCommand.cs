using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Utilities.Calculators;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;
using System.Globalization;

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
        private readonly IBaseRepository<UserTradingSetting> _userTradingSetting;

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
                var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == signal.BrokerAccountId && b.UserId == signal.UserId && b.BrokerName == "Binance");
                if (brokerAccount == null)
                    return new ErrorResult(Messages.Trading.InvalidBrokerAccount);

                var userTradingSettings = await _userTradingSetting.GetAsync(uts => uts.UserId == signal.UserId && uts.BrokerType == "Kripto");
                if (userTradingSettings == null)
                    return new ErrorResult(Messages.General.DataNotFound);

                var cryptoPrice = await _binanceService.GetMarketPriceAsync(signal.Symbol, brokerAccount.Id);
                if (cryptoPrice <= 0)
                    return new ErrorResult(Messages.Trading.PriceNotFound);

                var symbolInfo = await _binanceService.GetExchangeInfoAsync(brokerAccount.Id, signal.Symbol);
                if (symbolInfo == null)
                    return new ErrorResult(Messages.Trading.SymbolNotFound);

                var lotSizeFilter = symbolInfo.Filters.FirstOrDefault(f => f.FilterType == "LOT_SIZE");
                var minNotionalFilter = symbolInfo.Filters.FirstOrDefault(f => f.FilterType == "NOTIONAL");

                if (lotSizeFilter == null || minNotionalFilter == null)
                    return new ErrorResult(Messages.Trading.FilterNotFound);

                decimal minQty = decimal.Parse(lotSizeFilter.MinQty, CultureInfo.InvariantCulture);
                decimal minNotional = decimal.Parse(minNotionalFilter.MinNotional, CultureInfo.InvariantCulture);

                decimal calculatedQuantity = 0;

                if (signal.Action.Equals("SELL", StringComparison.OrdinalIgnoreCase))
                {
                    // **Mevcut pozisyonu al**
                    var position = await _binanceService.GetCryptoPositionAsync(signal.Symbol, brokerAccount.Id);
                    if (position == null || position.Quantity <= 0)
                        return new ErrorResult(Messages.Trading.NoPositionToSell);

                    decimal sellQuantity = position.Quantity;

                    // **LOT_SIZE ve MinNotional filtrelerini al**
                    var lotSizeFilterSell = symbolInfo.Filters.FirstOrDefault(f => f.FilterType == "LOT_SIZE");
                    var minNotionalFilterSell = symbolInfo.Filters.FirstOrDefault(f => f.FilterType == "NOTIONAL");

                    if (lotSizeFilterSell == null || minNotionalFilterSell == null)
                        return new ErrorResult("Gerekli Binance filtreleri bulunamadı.");

                    decimal minQtySell = decimal.Parse(lotSizeFilterSell.MinQty, CultureInfo.InvariantCulture);
                    decimal stepSizeSell = decimal.Parse(lotSizeFilterSell.StepSize, CultureInfo.InvariantCulture);
                    decimal minNotionalSell = decimal.Parse(minNotionalFilterSell.MinNotional, CultureInfo.InvariantCulture);

                    // **Mevcut stop loss emri var mı kontrol et**
                    var existingStopLossOrder = await _binanceService.CheckExistingStopLossOrderAsync(brokerAccount.Id, signal.Symbol);
                    if (existingStopLossOrder)
                    {
                        return new ErrorResult($"⚠️ {signal.Symbol} için aktif bir Stop Loss emri var. Satış işlemi gerçekleştirilemez.");
                    }

                    if (sellQuantity < minQtySell)
                    {
                        return new ErrorResult($"⚠️ Yetersiz bakiye: {sellQuantity}. Minimum LOT_SIZE: {minQtySell}. Satış yapılamaz.");
                    }

                    sellQuantity = Math.Floor(sellQuantity / stepSizeSell) * stepSizeSell;

                    if (sellQuantity * cryptoPrice < minNotionalSell)
                        return new ErrorResult($"⚠️ İşlem tutarı çok düşük. Minimum {minNotionalSell} USDT değerinde işlem yapılmalı.");

                    // **MARKET ORDER ile direkt satış yap**
                    var sellOrderResult = await _binanceService.PlaceOrderAsync(
                        brokerAccount.Id,
                        signal.Symbol,
                        sellQuantity,
                        "SELL",
                        signal.IsMarginTrade
                    );

                    if (!sellOrderResult)
                        return new ErrorResult(Messages.Trading.OrderFailed);

                    await _telegramBotService.SendMessageAsync(
                        signal.UserId.ToString(),
                        $"✅ **Satış tamamlandı:** {signal.Symbol}, **Miktar:** {sellQuantity} USDT"
                    );

                    return new SuccessResult(Messages.General.Success);
                }

                else
                {
                    // **BUY işlemi için hesaplamalar**
                    var accountBalance = await _binanceService.GetAccountBalanceAsync(brokerAccount.Id);
                    if (accountBalance <= 0)
                        return new ErrorResult(Messages.Trading.PriceNotFound);

                    calculatedQuantity = QuantityCalculator.CalculateCryptoQuantity(
                        accountBalance,
                        userTradingSettings.RiskPercentage,
                        cryptoPrice,
                        userTradingSettings.MaxRiskLimit,
                        userTradingSettings.MaxBuyQuantity
                    );

                    // **Binance LOT_SIZE uygunluğu kontrolü**
                    calculatedQuantity = await _binanceService.AdjustQuantityForBinance(
                        signal.Symbol,
                        calculatedQuantity,
                        cryptoPrice,
                        brokerAccount.Id,
                        false
                    );

                    // **MinNotional kontrolü**
                    if (calculatedQuantity * cryptoPrice < minNotional)
                    {
                        decimal minRequiredQty = Math.Ceiling(minNotional / cryptoPrice / decimal.Parse(lotSizeFilter.StepSize, CultureInfo.InvariantCulture))
                            * decimal.Parse(lotSizeFilter.StepSize, CultureInfo.InvariantCulture);

                        if (minRequiredQty * cryptoPrice > accountBalance)
                            return new ErrorResult($"Yetersiz bakiye. Minimum {minNotional} USDT değerinde işlem yapılmalı.");

                        calculatedQuantity = minRequiredQty;
                    }

                    var buyOrderResult = await _binanceService.PlaceOrderAsync(
                        brokerAccount.Id,
                        signal.Symbol,
                        calculatedQuantity,
                        "BUY",
                        signal.IsMarginTrade
                    );

                    if (!buyOrderResult)
                        return new ErrorResult(Messages.Trading.OrderFailed);

                    // **BUY işleminden sonra STOP-LOSS emri girilmeli**
                    decimal stopLossPrice = cryptoPrice * (1 - (userTradingSettings.SellPricePercentage / 100));
                    if (stopLossPrice >= cryptoPrice)
                        stopLossPrice = cryptoPrice * 0.99m;

                    stopLossPrice = await _binanceService.AdjustPriceForBinance(
                        signal.Symbol,
                        stopLossPrice,
                        cryptoPrice,
                        brokerAccount.Id
                    );

                    // **Stop Loss emrinin MinNotional sınırına uygun olması sağlanmalı**
                    if (stopLossPrice * calculatedQuantity < minNotional)
                        stopLossPrice = (minNotional / calculatedQuantity) * 1.01m;

                    var stopLossResult = await _binanceService.PlaceStopLossOrderAsync(
                        brokerAccount.Id,
                        signal.Symbol,
                        calculatedQuantity,
                        stopLossPrice
                    );

                    if (!stopLossResult)
                        return new ErrorResult(Messages.Trading.StopLossOrderFailed);

                    await _telegramBotService.SendMessageAsync(
                        signal.UserId.ToString(),
                        $"🟢 Alım tamamlandı: {signal.Symbol}, Miktar: {calculatedQuantity} USDT. Stop-loss {stopLossPrice} olarak ayarlandı."
                    );

                    return new SuccessResult(Messages.General.Success);
                }
            }
            catch (Exception ex)
            {
                return new ErrorResult($"{Messages.General.SystemError}: {ex.Message}");
            }
        }
    }
}
