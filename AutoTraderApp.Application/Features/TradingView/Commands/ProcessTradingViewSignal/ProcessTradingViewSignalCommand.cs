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
        private readonly IPolygonService _polygonService;

        public ProcessTradingViewSignalCommandHandler(
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaService alpacaService,
            TradingViewSignalLogService signalLogService,
            ITelegramBotService telegramBotService,
            IBaseRepository<CombinedStock> combinedStockRepository,
            IBaseRepository<BrokerLog> brokerLog,
            IPolygonService polygonService)
        {
            _brokerAccountRepository = brokerAccountRepository;
            _alpacaService = alpacaService;
            _signalLogService = signalLogService;
            _telegramBotService = telegramBotService;
            _combinedStockRepository = combinedStockRepository;
            _brokerLog = brokerLog;
            _polygonService = polygonService;
        }

        public async Task<IResult> Handle(ProcessTradingViewSignalCommand request, CancellationToken cancellationToken)
        {
            var signal = request.Signal;
            var transactionId = Guid.NewGuid();
            Console.WriteLine($"BROKER ACCOUNT ID: {signal.BrokerAccountId}");

            try
            {
                var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == signal.BrokerAccountId && b.UserId == signal.UserId);
                if (brokerAccount == null)
                    return new ErrorResult("Geçerli bir broker hesabı bulunamadı.");

                var combinedStocks = await _combinedStockRepository.GetAllAsync();
                if (combinedStocks == null || !combinedStocks.Any())
                {
                    return new ErrorResult("Birleşik hisse listesi bulunamadı.");
                }

                // ABD borsa saatleri kontrolü (Türkiye saatine göre)
                var nowTurkeyTime = DateTime.UtcNow.AddHours(3);
                var marketOpen = new TimeSpan(16, 30, 0);
                var marketClose = new TimeSpan(23, 35, 0);
                if (nowTurkeyTime.TimeOfDay < marketOpen || nowTurkeyTime.TimeOfDay > marketClose)
                    return new ErrorResult("Borsa saatleri dışında sinyal işlenemez.");

                // Mevcut pozisyonları kontrol et
                var openPositions = await _alpacaService.GetPositionsAsync(brokerAccount.Id);
                var existingPosition = openPositions.FirstOrDefault(p => p.Symbol == signal.Symbol);

                var latestPrice = await _alpacaService.GetLatestPriceAsync(signal.Symbol, signal.BrokerAccountId);
                Console.WriteLine($"{signal.Action} - {latestPrice} - {signal.Symbol}");

                // Risk kontrolü
                var account = await _alpacaService.GetAccountInfoAsync(brokerAccount.Id);
                if (account == null)
                    return new ErrorResult("Kullanıcı hesabı bilgileri alınamadı.");

                decimal accountValue = (decimal)account.BuyingPower;
                Console.WriteLine($"TransactionId: {transactionId} - Hesap değeri: {accountValue}");


                //risk kontrolü
                decimal riskPercentage = StockSelectionHelper.CalculateRiskPercentage(accountValue);
                var selectedStocks = StockSelectionHelper.SelectStocks(combinedStocks, accountValue);
                Console.WriteLine($"Account Equity: {account.Equity}");
                Console.WriteLine($"Buying Power: {account.BuyingPower}");
                Console.WriteLine($"Risk Percentage: {riskPercentage}");
                Console.WriteLine($"Calculated Risk Limit: {account.Equity * riskPercentage}");
                Console.WriteLine($"Signal Quantity: {signal.Quantity}");
                Console.WriteLine($"Latest Price: {latestPrice}");
                Console.WriteLine($"Signal Total Value: {signal.Quantity * latestPrice}");
                Console.WriteLine($"Buying Power: {account.BuyingPower}, Signal Total Cost: {signal.Quantity * latestPrice}, Risk Limit: {account.Equity * riskPercentage}");

               decimal riskLimit = accountValue * riskPercentage;
                decimal signalTotalCost = signal.Quantity * latestPrice;

                // Risk limiti kontrolü
                if (signalTotalCost > riskLimit)
                {
                    // Risk limitine uygun miktar hesaplanıyor
                    int adjustedQuantity = (int)(riskLimit / latestPrice);
                    Console.WriteLine($"Risk limitini aşan sinyal: {signal.Symbol}. Orijinal Miktar: {signal.Quantity}, Ayarlanan Miktar: {adjustedQuantity}, Risk Limiti: {riskLimit}");

                    if (adjustedQuantity > 0)
                    {
                        signal.Quantity = adjustedQuantity;
                        _alpacaService.AlpacaLog(signal.BrokerAccountId, $"Risk limitini aşan sinyal: {signal.Symbol}. Ayarlanan Miktar: {adjustedQuantity}, Risk Limiti: {riskLimit}");
                    }
                    else
                    {
                        // Hiçbir şey satın alınamıyorsa hata döndür
                        return new ErrorResult($"Risk limiti nedeniyle {signal.Symbol} için işlem gerçekleştirilemiyor. Risk Limiti: {riskLimit}");
                    }
                }

                var testfiyat = signal.Quantity;

                // Mevcut açık emirleri kontrol et
                var openOrders = await _alpacaService.GetAllOrdersAsync(signal.BrokerAccountId);
                var conflictingOrder = openOrders.FirstOrDefault(o => o.Symbol == signal.Symbol && o.Side != signal.Action);
                if (conflictingOrder != null)
                {
                    _alpacaService.AlpacaLog(signal.BrokerAccountId, $"Çakışan emir iptal edildi: {conflictingOrder.Symbol} - {conflictingOrder.OrderClass}");
                    Console.WriteLine($"Çakışan emir iptal edildi: {conflictingOrder.Symbol} - {conflictingOrder.OrderClass}");
                    await _alpacaService.CancelOrderAsync(conflictingOrder.OrderId, signal.BrokerAccountId);
                }

                // Varlığın kısa satışa uygunluğunu kontrol et
                var assetDetails = await _alpacaService.GetAssetDetailsAsync(signal.Symbol, signal.BrokerAccountId);
                if (signal.Action == "SELL" && !assetDetails.Shortable)
                {
                    _alpacaService.AlpacaLog(signal.BrokerAccountId, $"Varlık kısa satışa uygun değil: {signal.Symbol}");
                    Console.WriteLine($"Varlık kısa satışa uygun değil: {signal.Symbol}");
                    return new ErrorResult($"Varlık kısa satışa uygun değil: {signal.Symbol}");
                }

                // Varlığın ticarete uygunluğunu kontrol et
                if (!assetDetails.Tradable)
                {

                    _alpacaService.AlpacaLog(signal.BrokerAccountId, $"Varlık şu anda ticarete uygun değil: {signal.Symbol}");
                    Console.WriteLine($"Varlık şu anda ticarete uygun değil: {signal.Symbol}");
                    return new ErrorResult($"Varlık şu anda ticarete uygun değil: {signal.Symbol}");
                }

                // Mevcut pozisyon miktarını kontrol et
                var position = await _alpacaService.GetPositionBySymbolAsync(signal.Symbol, signal.BrokerAccountId);
                if (signal.Action == "SELL" && Convert.ToDecimal(position.AvailableQuantity) == 0)
                {

                    _alpacaService.AlpacaLog(signal.BrokerAccountId, $"Satılacak pozisyon bulunamadı: {signal.Symbol}");
                    Console.WriteLine($"Satılacak pozisyon bulunamadı ({signal.Action}): {signal.Symbol}");
                    return new ErrorResult($"{signal.Symbol} hissesi için açık bir pozisyon bulunamadı.");
                }
                if (signal.Action == "SELL" && Convert.ToDecimal(position.AvailableQuantity) < signal.Quantity)
                {
                    Console.WriteLine($"Yetersiz miktar: Mevcut {position.AvailableQuantity}, Gerekli {signal.Quantity}");
                    _alpacaService.AlpacaLog(signal.BrokerAccountId, $"Yetersiz miktar: Mevcut {position.AvailableQuantity}, Gerekli {signal.Quantity}");

                    var sellQuantity = Math.Min(signal.Quantity, Convert.ToDecimal(position.AvailableQuantity));
                    signal.Quantity = Convert.ToInt32(sellQuantity);
                    Console.WriteLine($"Yeni satılacak miktat: {signal.Quantity}");
                }

                Console.WriteLine(signal.Action + "Sinyali Alındı: " + signal.Symbol);



                // Retry mekanizması ile işlem yap
                await ExecuteWithRetry(async () =>
                {
                    // Güncel fiyatın alınması
                    var basePrice = Convert.ToDecimal(latestPrice);

                    // Minimum artış kurallarına uygun değer belirleme
                    var minIncrement = basePrice < 1.00M ? 0.01M : (basePrice < 100.00M ? 0.01M : 0.1M);

                    // Stop Loss ve Take Profit hesaplama
                    var calculatedStopPrice = basePrice - (basePrice * 6 / 100);
                    var calculatedTakeProfitPrice = basePrice * (basePrice * 50 / 100);

                    // Alpaca'nın artış kriterlerine uygun hale getirme
                    calculatedStopPrice = Math.Floor(calculatedStopPrice / minIncrement) * minIncrement;
                    calculatedTakeProfitPrice = Math.Floor(calculatedTakeProfitPrice / minIncrement) * minIncrement;

                    // Alpaca'nın minimum artış kurallarına zorla uyum
                    if (calculatedStopPrice >= basePrice)
                    {
                        calculatedStopPrice = Math.Floor((basePrice - minIncrement) / minIncrement) * minIncrement;
                    }

                    if (calculatedTakeProfitPrice <= calculatedStopPrice)
                    {
                        calculatedTakeProfitPrice = Math.Ceiling((calculatedStopPrice + minIncrement) / minIncrement) * minIncrement;
                    }


                    if (signal.Action.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                    {
                        var currentTime = nowTurkeyTime.TimeOfDay;
                        if (currentTime >= new TimeSpan(22, 45, 0) && currentTime <= new TimeSpan(23, 30, 0))
                        {
                            Console.WriteLine("Tüm hisseler satışa çıkarıldığı için alım işlemleri kapatılmıştr.");
                            return new ErrorResult("Tüm hisseler satışa çıkarıldığı için alım işlemleri kapatılmıştr.");
                        }
                        else
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
                                    LimitPrice = calculatedTakeProfitPrice
                                }
                            });

                            _alpacaService.AlpacaLog(signal.BrokerAccountId, $"Yeni emir oluşturuldu: {signal.Action} işlemi başarıyla gerçekleştirildi. {signal.Symbol}");
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
                                _alpacaService.AlpacaLog(signal.BrokerAccountId, $"{actionType} işlemi başarıyla gerçekleştirildi. {signal.Symbol}");
                                return new SuccessResult($"{actionType} işlemi başarıyla gerçekleştirildi.");
                            }
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
                            OrderClass = "simple"
                        });

                        _alpacaService.AlpacaLog(signal.BrokerAccountId, $"Yeni emir oluşturuldu: {signal.Action} işlemi başarıyla gerçekleştirildi. {signal.Symbol}");
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
                            _alpacaService.AlpacaLog(signal.BrokerAccountId, $"{actionType} işlemi başarıyla gerçekleştirildi. {signal.Symbol}");
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
                _alpacaService.AlpacaLog(signal.BrokerAccountId, $"XXXX Yeni emir oluşturulamadı: {signal.Action} ({signal.Quantity} adet) -- {signal.Symbol} :: HATA: {ex.Message}");
                await _brokerLog.AddAsync(new BrokerLog
                {
                    BrokerAccountId = signal.BrokerAccountId,
                    Message = $"XXXX Yeni emir oluşturulamadı: {signal.Action} ({signal.Quantity} adet) -- {signal.Symbol} :: HATA: {ex.Message}",

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
            var latestPrice = await _alpacaService.GetLatestPriceAsync(signal.Symbol, signal.BrokerAccountId);
            await _signalLogService.LogSignalAsync(
                signal.UserId,
                signal.BrokerAccountId,
                action,
                signal.Symbol,
                signal.Quantity,
                latestPrice,
                logTitle,
                logMessage
            );

            var telegramUser = await _telegramBotService.GetUserByIdOrPhoneNumberAsync(signal.UserId, null);
            if (telegramUser?.ChatId != null)
            {
                var message = $"TransactionId: {transactionId}\n{logTitle}:\n\nHisse: {signal.Symbol}\nİşlem: {action}\nMiktar: {signal.Quantity}\nFiyat: {latestPrice}";
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
