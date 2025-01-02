using AutoTraderApp.Application.Features.Strategies.Helpers;
using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Core.Utilities.Services;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;
using System.Diagnostics;

namespace AutoTraderApp.Application.Features.TradingView.Commands.ProcessTradingViewSignal
{
    public class ProcessTradingViewSignalCommand : IRequest<IResult>
    {
        public TradingViewSignalDto Signal { get; set; } = null!;
    }

    public class ProcessTradingViewSignalCommandHandler : IRequestHandler<ProcessTradingViewSignalCommand, IResult>
    {
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly IAlpacaService _alpacaService;
        private readonly TradingViewSignalLogService _signalLogService;
        private readonly ITelegramBotService _telegramBotService;
        private readonly IBaseRepository<CombinedStock> _combinedStockRepository;

        public ProcessTradingViewSignalCommandHandler(
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaService alpacaService,
            TradingViewSignalLogService signalLogService,
            ITelegramBotService telegramBotService,
            IBaseRepository<CombinedStock> combinedStockRepository)
        {
            _brokerAccountRepository = brokerAccountRepository;
            _alpacaService = alpacaService;
            _signalLogService = signalLogService;
            _telegramBotService = telegramBotService;
            _combinedStockRepository = combinedStockRepository;
        }

        public async Task<IResult> Handle(ProcessTradingViewSignalCommand request, CancellationToken cancellationToken)
        {
            var signal = request.Signal;
            var transactionId = Guid.NewGuid(); 

            try
            {
                // Broker hesabını doğrula
                var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == signal.BrokerAccountId && b.UserId == signal.UserId);
                if (brokerAccount == null)
                    return new ErrorResult("Geçerli bir broker hesabı bulunamadı.");

                var combinedStocks = await _combinedStockRepository.GetAllAsync();
                if (combinedStocks == null || !combinedStocks.Any())
                {
                    return new ErrorResult("Birleşik hisse listesi bulunamadı.");
                }

                // ABD borsa saatleri kontrolü (Türkiye saatine göre)
                var nowTurkeyTime = DateTime.UtcNow.AddHours(3); // UTC+3 Türkiye Saati
                var marketOpen = new TimeSpan(16, 30, 0);
                var marketClose = new TimeSpan(23, 30, 0);
                if (nowTurkeyTime.TimeOfDay < marketOpen || nowTurkeyTime.TimeOfDay > marketClose)
                    return new ErrorResult("Borsa saatleri dışında sinyal işlenemez.");

                // Mevcut pozisyonları kontrol et
                var openPositions = await _alpacaService.GetPositionsAsync(brokerAccount.Id);
                var existingPosition = openPositions.FirstOrDefault(p => p.Symbol == signal.Symbol);

                // Risk kontrolü
                var account = await _alpacaService.GetAccountInfoAsync(brokerAccount.Id);
                if (account == null)
                    return new ErrorResult("Kullanıcı hesabı bilgileri alınamadı.");

                decimal accountValue = account.Equity;
                Debug.WriteLine($"TransactionId: {transactionId} - Hesap değeri: {accountValue}");

                decimal riskPercentage = StockSelectionHelper.CalculateRiskPercentage(accountValue);
                var selectedStocks = StockSelectionHelper.SelectStocks(combinedStocks, accountValue);

                if (signal.Quantity * signal.Price > account.BuyingPower * riskPercentage)
                {
                    return new ErrorResult($"Risk limiti aşıldı: {signal.Symbol}");
                }

                // Retry mekanizması ile işlem yap
                await ExecuteWithRetry(async () =>
                {
                    // Zarar eden pozisyonları sat
                    await _alpacaService.SellLossMakingPositionsAsync(brokerAccount.Id);

                    if (signal.Action.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                    {
                        if (existingPosition != null && Convert.ToInt32(existingPosition.Quantity) > 0)
                        {
                            return new ErrorResult($"Hisse zaten açık pozisyonda: {signal.Symbol}");
                        }

                        var orderResult = await _alpacaService.PlaceOrderAsync(brokerAccount.Id, new OrderRequest
                        {
                            Symbol = signal.Symbol,
                            Qty = signal.Quantity,
                            Side = "buy",
                            Type = "market",
                            TimeInForce = "gtc"
                        });

                        if (IsOrderSuccessful(orderResult.Status))
                        {
                            await NotifyAndLog(signal, transactionId, "Alım işlemi başarılı", $"Hisse alındı: {signal.Symbol}, Miktar: {signal.Quantity}", "BUY");
                            return new SuccessResult("Alım işlemi başarıyla gerçekleştirildi.");
                        }
                    }
                    else if (signal.Action.Equals("SELL", StringComparison.OrdinalIgnoreCase))
                    {
                        if (existingPosition == null || Convert.ToInt32(existingPosition.Quantity) <= 0)
                        {
                            return new ErrorResult($"Açık bir pozisyon bulunamadı: {signal.Symbol}");
                        }

                        var orderResult = await _alpacaService.PlaceOrderAsync(brokerAccount.Id, new OrderRequest
                        {
                            Symbol = signal.Symbol,
                            Qty = signal.Quantity,
                            Side = "sell",
                            Type = "market",
                            TimeInForce = "gtc"
                        });

                        if (IsOrderSuccessful(orderResult.Status))
                        {
                            await NotifyAndLog(signal, transactionId, "Satış işlemi başarılı", $"Hisse satıldı: {signal.Symbol}, Miktar: {signal.Quantity}", "SELL");
                            return new SuccessResult("Satış işlemi başarıyla gerçekleştirildi.");
                        }
                    }

                    return new ErrorResult("İşlem gerçekleştirilemedi.");
                });

                return new SuccessResult("İşlem tamamlandı.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TransactionId: {transactionId} - Hata: {ex.Message}");
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }

        private static bool IsOrderSuccessful(string status)
        {
            return status == "accepted" || status == "new" || status == "pending_new" || status == "partially_filled" || status == "filled";
        }

        private async Task NotifyAndLog(TradingViewSignalDto signal, Guid transactionId, string logTitle, string logMessage, string action)
        {
            await _signalLogService.LogSignalAsync(
                signal.UserId,
                signal.BrokerAccountId,
                action,
                signal.Symbol,
                signal.Quantity,
                signal.Price,
                logTitle,
                logMessage
            );

            var telegramUser = await _telegramBotService.GetUserByIdOrPhoneNumberAsync(signal.UserId, null);
            if (telegramUser?.ChatId != null)
            {
                var message = $"TransactionId: {transactionId}\n{logTitle}:\n\nHisse: {signal.Symbol}\nİşlem: {action}\nMiktar: {signal.Quantity}\nFiyat: {signal.Price}";
                await _telegramBotService.SendMessageAsync(telegramUser.ChatId, message);
            }
        }

        private async Task ExecuteWithRetry(Func<Task<IResult>> action, int maxRetryCount = 3, int delayInSeconds = 2)
        {
            for (int attempt = 1; attempt <= maxRetryCount; attempt++)
            {
                try
                {
                    var result = await action();
                    if (result.Success)
                        return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Retry {attempt}/{maxRetryCount} failed: {ex.Message}");
                    if (attempt == maxRetryCount)
                        throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
            }
        }
    }
}
