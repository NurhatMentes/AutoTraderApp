using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;


namespace AutoTraderApp.Infrastructure.Services.Alpaca
{
    public class ScheduledTaskService
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAlpacaService _alpacaService;
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly IBaseRepository<UserTradingSetting> _userTradingSetting;

        public ScheduledTaskService(IServiceProvider serviceProvider, IAlpacaService alpacaService,
            IBaseRepository<BrokerAccount> brokerAccountRepository, IBaseRepository<UserTradingSetting> userTradingSetting)
        {
            _serviceProvider = serviceProvider;
            _alpacaService = alpacaService;
            _brokerAccountRepository = brokerAccountRepository;
            StartScheduler();
            _userTradingSetting = userTradingSetting;
        }

        private void StartScheduler()
        {
            _timer = new Timer(async _ =>
            {
                try
                {
                    Console.WriteLine("ScheduledTaskService başlatıldı.");
                    await ExecuteScheduledTaskAsync();
                    await CheckAndPlaceSellOrdersAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{Messages.General.SystemError}: {ex.Message}");
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        private async Task ExecuteScheduledTaskAsync()
        {
            var nowTurkeyTime = DateTime.UtcNow.AddHours(3); // UTC+3 Türkiye saati
            var currentDay = nowTurkeyTime.DayOfWeek;

            if (currentDay == DayOfWeek.Saturday || currentDay == DayOfWeek.Sunday)
                return;

            var currentTime = nowTurkeyTime.TimeOfDay;
            if ((currentTime >= new TimeSpan(17, 30, 0) && currentTime <= new TimeSpan(17, 33, 0)) ||
                (currentTime >= new TimeSpan(23, 50, 10) && currentTime <= new TimeSpan(24, 00, 0)))
            {
                Console.WriteLine("SellAllPositionsAtEndOfDayAsync çalıştırılıyor...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var brokerAccountRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<BrokerAccount>>();
                    var alpacaService = scope.ServiceProvider.GetRequiredService<IAlpacaService>();

                    var brokerAccounts = await brokerAccountRepository.GetAllAsync();

                    foreach (var brokerAccount in brokerAccounts)
                    {
                        try
                        {
                            await alpacaService.SellAllPositionsAtEndOfDayAsync(brokerAccount.Id);
                            Console.WriteLine($"{Messages.Trading.OrderSuccess} - BrokerAccountId: {brokerAccount.Id}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{Messages.General.SystemError}: {ex.Message} - BrokerAccountId: {brokerAccount.Id}");
                            await _alpacaService.AlpacaLog(brokerAccount.Id, "-", null, null, null, $"{Messages.General.SystemError}: {ex.Message} - BrokerAccountId: {brokerAccount.Id}");
                        }
                    }
                }
            }
        }

        private async Task CheckAndPlaceSellOrdersAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var brokerAccounts = await _brokerAccountRepository.GetAllAsync();

                foreach (var brokerAccount in brokerAccounts)
                {
                    try
                    {
                        var openOrders = await _alpacaService.GetAllOrdersAsync(brokerAccount.Id);
                        var userTradingSettings = await _userTradingSetting.GetAsync(uts => uts.UserId == brokerAccount.UserId);
                        if (userTradingSettings == null)
                        {
                            Console.WriteLine($"Kullanıcı ayarları bulunamadı - BrokerAccountId: {brokerAccount.Id}");
                            continue;
                        }

                        foreach (var order in openOrders)
                        {
                            if (!order.Side.Equals("buy", StringComparison.OrdinalIgnoreCase) ||
                                order.Status != "filled")
                            {
                                continue;
                            }

                            var existingSellOrders = openOrders.Where(o =>
                                o.Symbol == order.Symbol &&
                                o.Side.Equals("sell", StringComparison.OrdinalIgnoreCase) &&
                                (o.Status == "new" || o.Status == "accepted" || o.Status == "partially_filled")).ToList();

                            if (existingSellOrders.Any())
                            {
                                Console.WriteLine($"Hisse için zaten aktif sell emirleri mevcut: {order.Symbol}");
                                continue;
                            }

                            // Mevcut pozisyonları kontrol et
                            var openPositions = await _alpacaService.GetPositionsAsync(brokerAccount.Id);
                            var existingPosition = openPositions.FirstOrDefault(p => p.Symbol == order.Symbol);

                            if (existingPosition != null)
                            {
                                decimal buyPrice;
                                try
                                {
                                    buyPrice = decimal.Parse(order.FilledAvgPrice,
                                        System.Globalization.NumberStyles.Any,
                                        System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Fiyat dönüşüm hatası - Symbol: {order.Symbol}, " +
                                        $"FilledAvgPrice: {order.FilledAvgPrice}, Hata: {ex.Message}");
                                    continue;
                                }

                                await _alpacaService.AlpacaLog(brokerAccount.Id, order.Symbol, "BUY",
                                    buyPrice, Convert.ToInt32(order.FilledQuantity),
                                    $"{order.Symbol} alım emri gerçekleşti, OCO emirleri oluşturuluyor");

                                decimal takeProfitPrice = buyPrice * (1 + userTradingSettings.SellPricePercentage / 100m);
                                decimal takeProfitPriceRounded = Math.Floor(takeProfitPrice * 100) / 100;

                                decimal stopLossPrice = buyPrice * (1 - userTradingSettings.BuyPricePercentage / 100m);
                                decimal stopLossPriceRounded = Math.Floor(stopLossPrice * 100) / 100;

                                // OCO (One-Cancels-Other) order oluştur
                                var ocoOrder = new OrderRequest
                                {
                                    Symbol = order.Symbol,
                                    Qty = Convert.ToInt32(existingPosition.Quantity),
                                    Side = "sell",
                                    Type = "limit",
                                    TimeInForce = "gtc",
                                    OrderClass = "oco",
                                    TakeProfit = new TakeProfit
                                    {
                                        LimitPrice = takeProfitPriceRounded
                                    },
                                    StopLoss = new StopLoss
                                    {
                                        StopPrice = stopLossPriceRounded,
                                        LimitPrice = stopLossPriceRounded - 0.01M
                                    }
                                };

                                try
                                {
                                    var ocoResponse = await _alpacaService.PlaceOrderAsync(brokerAccount.Id, ocoOrder);

                                    if (ocoResponse != null && ocoResponse.Status == "accepted" || ocoResponse.Status == "pending_new" || ocoResponse.Status == "filled")
                                    {
                                        await _alpacaService.AlpacaLog(brokerAccount.Id, order.Symbol, "SELL",
                                            takeProfitPriceRounded,
                                            Convert.ToInt32(order.FilledQuantity),
                                            $"OCO emri verildi - Take Profit: {takeProfitPriceRounded}, Stop Loss: {stopLossPriceRounded}");

                                        Console.WriteLine($"OCO emri başarıyla oluşturuldu - Symbol: {order.Symbol}");
                                    }
                                    else
                                    {
                                        await _alpacaService.AlpacaLog(brokerAccount.Id, order.Symbol, "SELL",
                                            takeProfitPriceRounded,
                                            Convert.ToInt32(order.FilledQuantity),
                                            $"OCO emri başarısız - Status: {ocoResponse?.Status ?? "null"}");

                                        Console.WriteLine($"OCO emri başarısız - Symbol: {order.Symbol}, Status: {ocoResponse?.Status ?? "null"}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"OCO emir hatası - Symbol: {order.Symbol}, Hata: {ex.Message}");
                                    await _alpacaService.AlpacaLog(brokerAccount.Id, order.Symbol, "SELL",
                                        takeProfitPriceRounded,
                                        Convert.ToInt32(order.FilledQuantity),
                                        $"OCO emir hatası: {ex.Message}");
                                }

                                await Task.Delay(2000);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Sistem hatası: {ex.Message} - BrokerAccountId: {brokerAccount.Id}");
                        await _alpacaService.AlpacaLog(brokerAccount.Id, "-", null, null, null,
                            $"Sistem hatası: {ex.Message} - BrokerAccountId: {brokerAccount.Id}");
                    }
                }
            }
        }
    }
}