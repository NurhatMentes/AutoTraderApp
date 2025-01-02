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
        private readonly IBaseRepository<BrokerLog> _brokerLog;
        private readonly IAlpacaService _alpacaService;
        private readonly TradingViewSignalLogService _signalLogService;
        private readonly ITelegramBotService _telegramBotService;
        private readonly IBaseRepository<CombinedStock> _combinedStockRepository;

        public ProcessTradingViewSignalCommandHandler(
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaService alpacaService,
            TradingViewSignalLogService signalLogService,
            ITelegramBotService telegramBotService,
            IBaseRepository<CombinedStock> combinedStockRepository,
            IBaseRepository<BrokerLog> brokerLog)
        {
            _brokerAccountRepository = brokerAccountRepository;
            _alpacaService = alpacaService;
            _signalLogService = signalLogService;
            _telegramBotService = telegramBotService;
            _combinedStockRepository = combinedStockRepository;
            _brokerLog = brokerLog;
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
                Console.WriteLine($"TransactionId: {transactionId} - Hesap değeri: {accountValue}");

                decimal riskPercentage = StockSelectionHelper.CalculateRiskPercentage(accountValue);
                var selectedStocks = StockSelectionHelper.SelectStocks(combinedStocks, accountValue);

                if (signal.Quantity * signal.Price > account.BuyingPower * riskPercentage)
                {
                    return new ErrorResult($"Risk limiti aşıldı: {signal.Symbol}");
                }

                // alım gücünü doğrula
                var accountInfo = await _alpacaService.GetAccountInfoAsync(signal.BrokerAccountId);
                if (accountInfo == null || accountInfo.BuyingPower < signal.Quantity * signal.Price)
                {
                    Console.WriteLine($"Yetersiz alım gücü: Mevcut {accountInfo?.BuyingPower}, Gerekli {signal.Quantity * signal.Price}");
                    return new ErrorResult($"Yetersiz alım gücü: Mevcut {accountInfo?.BuyingPower}, Gerekli {signal.Quantity * signal.Price}");
                }

                // Mevcut açık emirleri kontrol et
                var openOrders = await _alpacaService.GetAllOrdersAsync(signal.BrokerAccountId);
                var conflictingOrder = openOrders.FirstOrDefault(o => o.Symbol == signal.Symbol && o.Side != signal.Action);
                if (conflictingOrder != null)
                {
                    await _alpacaService.CancelOrderAsync(conflictingOrder.OrderId, signal.BrokerAccountId);
                    Console.WriteLine($"Çakışan emir iptal edildi: {conflictingOrder.Symbol} - {conflictingOrder.OrderClass}");
                }   

                // Varlığın kısa satışa uygunluğunu kontrol et
                var assetDetails = await _alpacaService.GetAssetDetailsAsync(signal.Symbol, signal.BrokerAccountId);
                if (signal.Action == "SELL" && !assetDetails.Shortable)
                {
                    Console.WriteLine($"Varlık kısa satışa uygun değil: {signal.Symbol}");
                    return new ErrorResult($"Varlık kısa satışa uygun değil: {signal.Symbol}");
                }

                // Varlığın ticarete uygunluğunu kontrol et
                if (!assetDetails.Tradable)
                {
                    Console.WriteLine($"Varlık şu anda ticarete uygun değil: {signal.Symbol}");
                    return new ErrorResult($"Varlık şu anda ticarete uygun değil: {signal.Symbol}");
                }

                // Mevcut pozisyon miktarını kontrol et
                var position = await _alpacaService.GetPositionBySymbolAsync(signal.Symbol, signal.BrokerAccountId);
                if (signal.Action == "SELL" && Convert.ToDecimal(position.AvailableQuantity) < signal.Quantity)
                {
                    return new ErrorResult($"Yetersiz miktar: Mevcut {position.AvailableQuantity}, Gerekli {signal.Quantity}");
                }

                Console.WriteLine(signal.Action + "Sinyali Alındı: " + signal.Symbol);
                // Retry mekanizması ile işlem yap
                await ExecuteWithRetry(async () =>
                {
                    // Zarar eden pozisyonları kontrol et ve sat
                    var sellLossResult = await _alpacaService.SellLossMakingPositionsAsync(brokerAccount.Id);

                    var basePrice = signal.Price; // Gelen fiyat
                    var calculatedStopPrice = Math.Min(basePrice * 0.95m, basePrice - 0.01m); // %5 zarar durdurma hedefi veya Alpaca kuralı

                    // Alpaca'nın izin verdiği minimum stop_price doğrulaması
                    if (calculatedStopPrice > basePrice - 0.01m)
                    {
                        calculatedStopPrice = basePrice - 0.01m;
                    }

                    if (signal.Action.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                    {
                        var orderRequest = await _alpacaService.PlaceOrderAsync(brokerAccount.Id, new OrderRequest
                        {
                            Symbol = signal.Symbol,
                            Qty = signal.Quantity,
                            Side = "buy",
                            Type = "market",
                            TimeInForce = "gtc",
                            OrderClass = "bracket",
                            StopLoss = new StopLoss
                            {
                                StopPrice = calculatedStopPrice
                            },
                            TakeProfit = new TakeProfit
                            {
                                LimitPrice = signal.Price * 1.50m 
                            }
                        });


                        await _brokerLog.AddAsync(new BrokerLog
                        {
                            BrokerAccountId = signal.BrokerAccountId,
                            Message = $"Yeni emir oluşturuldu: {signal.Action} işlemi başarıyla gerçekleştirildi. {signal.Symbol}",

                        });

                        if (IsOrderSuccessful(orderRequest.Status))
                        {
                            string actionType = signal.Action.Equals("BUY", StringComparison.OrdinalIgnoreCase) ? "BUY" : "SELL";
                            await NotifyAndLog(signal, transactionId, $"{actionType} işlemi başarılı", $"Hisse {actionType} yapıldı: {signal.Symbol}, Miktar: {signal.Quantity}", actionType);
                            Console.WriteLine($"{actionType} işlemi başarıyla gerçekleştirildi. {signal.Symbol}");
                            return new SuccessResult($"{actionType} işlemi başarıyla gerçekleştirildi.");
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
                            TimeInForce = "gtc",
                            OrderClass = "bracket",
                            StopLoss = new StopLoss
                            {
                                StopPrice = calculatedStopPrice 
                            },
                            TakeProfit = new TakeProfit
                            {
                                LimitPrice = signal.Price * 1.50m 
                            }
                        });

                        await _brokerLog.AddAsync(new BrokerLog
                        {
                            BrokerAccountId = signal.BrokerAccountId,
                            Message = $"Yeni emir oluşturuldu: {signal.Action} işlemi başarıyla gerçekleştirildi. {signal.Symbol}",

                        });


                        if (IsOrderSuccessful(orderResult.Status))
                        {
                            string actionType = signal.Action.Equals("BUY", StringComparison.OrdinalIgnoreCase) ? "BUY" : "SELL";
                            await NotifyAndLog(signal, transactionId, $"{actionType} işlemi başarılı", $"Hisse {actionType} yapıldı: {signal.Symbol}, Miktar: {signal.Quantity}", actionType);
                            Console.WriteLine($"{actionType} işlemi başarıyla gerçekleştirildi. {signal.Symbol}");
                            return new SuccessResult($"{actionType} işlemi başarıyla gerçekleştirildi.");
                        }
                    }


                    Console.WriteLine("İşlem gerçekleştirilemedi.");
                    return new ErrorResult("İşlem gerçekleştirilemedi.");
                });


                return new SuccessResult("İşlem tamamlandı.");
            }
            catch (Exception ex)
            {
                await _brokerLog.AddAsync(new BrokerLog
                {
                    BrokerAccountId = signal.BrokerAccountId,
                    Message = $"XXXX Yeni emir oluşturulamadı: {signal.Action} -- {signal.Symbol} :: HATA: {ex.Message}",

                });
                Console.WriteLine($"TransactionId: {transactionId} - Hata: {ex.Message}");
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
