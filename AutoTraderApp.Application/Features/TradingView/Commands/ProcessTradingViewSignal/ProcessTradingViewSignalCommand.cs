using AutoTraderApp.Application.Features.Strategies.Helpers;
using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Core.Utilities.Calculators;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Core.Utilities.Services;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;
using Newtonsoft.Json;
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
            var position = await _alpacaService.GetPositionBySymbolAsync(signal.Symbol, signal.BrokerAccountId);

            try
            {
                var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == signal.BrokerAccountId && b.UserId == signal.UserId);
                if (brokerAccount == null)
                    return new ErrorResult("Geçerli bir broker hesabı bulunamadı.");

                // ABD borsa saatleri kontrolü (Türkiye saatine göre)
                var nowTurkeyTime = DateTime.UtcNow.AddHours(3);
                var marketOpen = new TimeSpan(11, 30, 0);
                var marketClose = new TimeSpan(23, 45, 0);
                if (nowTurkeyTime.TimeOfDay < marketOpen || nowTurkeyTime.TimeOfDay > marketClose)
                    return new ErrorResult("Borsa saatleri dışında sinyal işlenemez.");

                // Mevcut pozisyonları kontrol et
                var openPositions = await _alpacaService.GetPositionsAsync(brokerAccount.Id);
                var existingPosition = openPositions.FirstOrDefault(p => p.Symbol == signal.Symbol);

                var price = await _alpacaService.GetLatestPriceAsync(signal.Symbol, signal.BrokerAccountId);
                Console.WriteLine($"{signal.Action} - {price} - {signal.Symbol}");

                // Hesap bilgileri ve buying power kontrolü
                var account = await _alpacaService.GetAccountInfoAsync(brokerAccount.Id);
                if (account == null)
                    return new ErrorResult("Kullanıcı hesabı bilgileri alınamadı.");

                decimal accountBuyingPower = (decimal)account.BuyingPower;
                Console.WriteLine($"TransactionId: {transactionId} - Buying Power: {accountBuyingPower}");

                // Risk hesaplama
                decimal riskPercentage = StockSelectionHelper.CalculateRiskPercentage(accountBuyingPower);
                decimal riskLimit = accountBuyingPower * riskPercentage;
                decimal signalTotalCost = signal.Quantity * price;

                Console.WriteLine($"Account Equity: {account.Equity}");
                Console.WriteLine($"Buying Power: {accountBuyingPower}");
                Console.WriteLine($"Risk Percentage: {riskPercentage}");
                Console.WriteLine($"Calculated Risk Limit: {riskLimit}");
                Console.WriteLine($"Signal Quantity: {signal.Quantity}");
                Console.WriteLine($"Latest Price: {price}");
                Console.WriteLine($"Signal Total Value: {signalTotalCost}");

                // Minimum toplam fiyat kontrolü
                if (signal.Action.Equals("BUY", StringComparison.OrdinalIgnoreCase) && signalTotalCost < 2500)
                {
                    signal.Quantity = QuantityCalculator.CalculateQuantity(accountBuyingPower, riskPercentage, price, price * 0.95M); 
                    signalTotalCost = signal.Quantity * price;
                    Console.WriteLine($"Toplam maliyet 2500 doların altındaydı, ayarlanan miktar: {signal.Quantity}");
                }

                // Buying Power kontrolü ve miktar ayarlaması
                if (signal.Action.Equals("BUY", StringComparison.OrdinalIgnoreCase) && signalTotalCost > accountBuyingPower)
                {
                    int adjustedQuantity = (int)(accountBuyingPower / price);
                    Console.WriteLine($"Buying power limitini aşan sinyal: {signal.Symbol}. Orijinal Miktar: {signal.Quantity}, Ayarlanan Miktar: {adjustedQuantity}, Buying Power: {accountBuyingPower}");

                    if (adjustedQuantity > 0)
                    {
                        signal.Quantity = adjustedQuantity;
                        signalTotalCost = signal.Quantity * price;
                        await _alpacaService.AlpacaLog(signal.BrokerAccountId, signal.Symbol, price, adjustedQuantity, $"Buying power limitini aşan sinyal. Buying Power: {accountBuyingPower}");
                        Console.WriteLine($"Adjusted Quantity: {adjustedQuantity}, New Signal Total Cost: {signalTotalCost}");
                    }
                    else
                    {
                        return new ErrorResult($"Buying power limiti nedeniyle {signal.Symbol} için işlem gerçekleştirilemiyor. Buying Power: {accountBuyingPower}");
                    }
                }

                // Risk limiti kontrolü ve miktar ayarlaması
                if (signal.Action.Equals("BUY", StringComparison.OrdinalIgnoreCase) && signalTotalCost > riskLimit)
                {
                    int adjustedQuantity = (int)Math.Floor(riskLimit / price);
                    signalTotalCost = adjustedQuantity * price;

                    // Toplam maliyeti tekrar kontrol et ve ayarla
                    if (signalTotalCost > riskLimit)
                    {
                        adjustedQuantity = (int)Math.Floor(riskLimit / price);
                        signalTotalCost = adjustedQuantity * price;
                    }

                    Console.WriteLine($"Risk limitini aşan sinyal: {signal.Symbol}. Orijinal Miktar: {signal.Quantity}, Ayarlanan Miktar: {adjustedQuantity}, Risk Limiti: {riskLimit}");
                }


                // Mevcut açık emirleri kontrol et
                //var openOrders = await _alpacaService.GetAllOrdersAsync(signal.BrokerAccountId);
                //var conflictingOrder = openOrders.FirstOrDefault(o => o.Symbol == signal.Symbol && o.Side != signal.Action);
                //if (conflictingOrder != null)
                //{
                //    await _alpacaService.AlpacaLog(signal.BrokerAccountId, signal.Symbol, latestPrice, null, $"Çakışan emir iptal edildi: Sınıf: {conflictingOrder.OrderClass}");
                //    Console.WriteLine($"Çakışan emir iptal edildi: {conflictingOrder.Symbol} - {conflictingOrder.OrderClass}");
                //    await _alpacaService.CancelOrderAsync(conflictingOrder.OrderId, signal.BrokerAccountId);
                //}

                // Varlığın kısa satışa uygunluğunu kontrol et
                var assetDetails = await _alpacaService.GetAssetDetailsAsync(signal.Symbol, signal.BrokerAccountId);
                if (signal.Action == "SELL" && !assetDetails.Shortable)
                {
                    Console.WriteLine($"Varlık kısa satışa uygun değil: {signal.Symbol}");
                    await _alpacaService.ClosePositionAsync(signal.Symbol, Convert.ToDecimal(position.AvailableQuantity), signal.BrokerAccountId);
                    await _alpacaService.AlpacaLog(signal.BrokerAccountId, signal.Symbol, price, Convert.ToInt16(position.AvailableQuantity), $"Varlık kısa satışa uygun değil! Tüm miktar satıldı.");
                }

                // Varlığın ticarete uygunluğunu kontrol et
                if (!assetDetails.Tradable)
                {

                    await _alpacaService.AlpacaLog(signal.BrokerAccountId, signal.Symbol, price, null, $"Varlık şu anda ticarete uygun değil");
                    Console.WriteLine($"Varlık şu anda ticarete uygun değil: {signal.Symbol}");
                    return new ErrorResult($"Varlık şu anda ticarete uygun değil: {signal.Symbol}");
                }

                // Mevcut pozisyon miktarını kontrol et
                if (signal.Action.Equals("SELL", StringComparison.OrdinalIgnoreCase) && Convert.ToInt32(position.Quantity) == 0)
                {
                    await _alpacaService.AlpacaLog(signal.BrokerAccountId, signal.Symbol, price, null, $"Satılacak pozisyon bulunamadı");
                    Console.WriteLine($"Satılacak pozisyon bulunamadı ({signal.Action}): {signal.Symbol}");
                    return new ErrorResult($"{signal.Symbol} hissesi için açık bir pozisyon bulunamadı.");
                }
                if (signal.Action.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                {
                    if (position != null && signal.Quantity == Convert.ToInt32(position.Quantity))
                    {
                        signal.Quantity += 1;
                        Console.WriteLine($"Mevcut pozisyonla aynı miktar olduğu için miktar 1 artırıldı. Yeni miktar: {signal.Quantity}");
                    }
                }
                if (signal.Action.Equals("SELL", StringComparison.OrdinalIgnoreCase) && Convert.ToInt32(position.AvailableQuantity) < signal.Quantity)
                {
                    Console.WriteLine($"Yetersiz miktar: Mevcut {position.AvailableQuantity}, Gerekli {signal.Quantity}");
                    await _alpacaService.AlpacaLog(signal.BrokerAccountId, signal.Symbol, price, Convert.ToInt32(position.AvailableQuantity), $"Yetersiz miktar. Gerekli {signal.Quantity}");

                    var sellQuantity = Math.Max(Convert.ToDecimal(position.Quantity), Convert.ToDecimal(position.AvailableQuantity));
                    signal.Quantity = Convert.ToInt32(sellQuantity);
                    Console.WriteLine($"Yeni satılacak miktat: {signal.Quantity}");
                }

                //İlgili hissenin açık buy emri varsa iptal et.
                var openOrders = await _alpacaService.GetAllOrdersAsync(brokerAccount.Id);
                foreach (var order in openOrders)
                {
                    if (order.Symbol == signal.Symbol && order.Side == "buy")
                    {
                        // Alım emrini iptal et
                        await _alpacaService.CancelOrderAsync(order.OrderId, brokerAccount.Id);
                        Console.WriteLine($"Alım emri iptal edildi: {order.OrderId}");
                    }
                }


                await ExecuteWithRetry(async () =>
                {
                    var basePrice = Convert.ToDecimal(price);
                    var minIncrement = basePrice < 1.00M ? 0.0001M : 0.01M;

                    if (signal.Action.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                    {
                        var buyLimitPrice = basePrice * 0.98M; 
                        var buyLimitPriceRounded = Math.Floor(buyLimitPrice / minIncrement) * minIncrement; 

                        var buyOrderRequest = await _alpacaService.PlaceOrderAsync(brokerAccount.Id, new OrderRequest
                        {
                            Symbol = signal.Symbol,
                            Qty = signal.Quantity,
                            Side = "buy",
                            Type = "limit",
                            TimeInForce = "gtc",
                            LimitPrice = buyLimitPriceRounded 
                        });
                       
                        // 3. Loglama ve bildirim
                        string actionType = "BUY";
                        await NotifyAndLog(signal, transactionId, $"{actionType} işlemi başarılı", $"Hisse {actionType} yapıldı: {signal.Symbol}, Miktar: {signal.Quantity}", actionType);
                        await _alpacaService.AlpacaLog(signal.BrokerAccountId, signal.Symbol, buyLimitPriceRounded, signal.Quantity, $"{actionType} işlemi için emir verildi..");
                        return new SuccessResult($"{actionType} işlemi için emir verildi.");
                    }

                    else if (signal.Action.Equals("SELL", StringComparison.OrdinalIgnoreCase))
                    {
                        if (existingPosition == null || Convert.ToInt32(existingPosition.Quantity) <= 0)
                        {
                            return new ErrorResult($"Açık bir pozisyon bulunamadı: {signal.Symbol}");
                        }

                        var openOrders = await _alpacaService.GetAllOrdersAsync(brokerAccount.Id);
                        foreach (var order in openOrders)
                        {
                            if (order.Symbol == signal.Symbol && order.Type == "trailing_stop")
                            {
                                await _alpacaService.CancelOrderAsync(order.OrderId, brokerAccount.Id);
                                Console.WriteLine($"Trailing stop emri iptal edildi: {order.OrderId}");
                            }
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
                            string actionType = signal.Action.Equals("BUY", StringComparison.OrdinalIgnoreCase) ? "BUY" : "SELL";
                            await NotifyAndLog(signal, transactionId, $"{actionType} işlemi başarılı", $"Hisse {actionType} yapıldı: {signal.Symbol}, Miktar: {signal.Quantity}", actionType);
                            Console.WriteLine($"{actionType} işlemi başarıyla gerçekleştirildi. {signal.Symbol}");
                            await _alpacaService.AlpacaLog(signal.BrokerAccountId, signal.Symbol, price, signal.Quantity, $"{actionType} işlemi başarıyla gerçekleştirildi.");
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
                await _alpacaService.AlpacaLog(signal.BrokerAccountId, signal.Symbol, null, signal.Quantity, $"XXXX Yeni emir oluşturulamadı: {signal.Action} -- HATA: {ex.Message}");
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
