using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Transactions;


namespace AutoTraderApp.Infrastructure.Services.Alpaca
{
    public class ScheduledTaskService
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAlpacaService _alpacaService;
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;

        public ScheduledTaskService(IServiceProvider serviceProvider, IAlpacaService alpacaService, IBaseRepository<BrokerAccount> brokerAccountRepository)
        {
            _serviceProvider = serviceProvider;
            _alpacaService = alpacaService;
            _brokerAccountRepository = brokerAccountRepository;
            StartScheduler();
        }

        private void StartScheduler()
        {
            Console.WriteLine("ScheduledTaskService başlatıldı.");
            _timer = new Timer(async _ =>
            {
                try
                {
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
                            await _alpacaService.AlpacaLog(brokerAccount.Id, "-",null, null, null, $"{Messages.General.SystemError}: {ex.Message} - BrokerAccountId: {brokerAccount.Id}");
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

                        foreach (var order in openOrders)
                        {
                            if (order.Side.Equals("BUY", StringComparison.OrdinalIgnoreCase) && order.Status == "filled")
                            {
                                var buyPrice = Convert.ToDecimal(order.FilledAvgPrice);
                                await _alpacaService.AlpacaLog(brokerAccount.Id, order.Symbol, "BUY", buyPrice, Convert.ToInt32(order.FilledQuantity), $"{order.Symbol} {Messages.Trading.OrderExecuted}");

                                var takeSellPrice = buyPrice * 1.02M;
                                var takeSellPriceRounded = Math.Floor(takeSellPrice / 0.01M) * 0.01M;

                                var stopSellPrice = buyPrice * 0.98M;
                                var stopSellPriceRounded = Math.Floor(takeSellPrice / 0.01M) * 0.01M;

                                var takeSellOrderRequest = new OrderRequest
                                {
                                    Symbol = order.Symbol,
                                    Qty = Convert.ToInt32(order.FilledQuantity),
                                    Side = "sell",
                                    Type = "limit",
                                    TimeInForce = "gtc",
                                    LimitPrice = takeSellPriceRounded
                                };
                                var takeSellOrderResponse = await _alpacaService.PlaceOrderAsync(brokerAccount.Id, takeSellOrderRequest);

                                await Task.Delay(2000);

                                var stopSellOrderRequest = new OrderRequest
                                {
                                    Symbol = order.Symbol,
                                    Qty = Convert.ToInt32(order.FilledQuantity),
                                    Side = "sell",
                                    Type = "stop",
                                    TimeInForce = "gtc",
                                    LimitPrice = stopSellPriceRounded
                                };
                                var stopSellOrderResponse = await _alpacaService.PlaceOrderAsync(brokerAccount.Id, stopSellOrderRequest);

                                if (takeSellOrderResponse.Status == "accepted")
                                {
                                    await _alpacaService.AlpacaLog(brokerAccount.Id, order.Symbol, "SELL", takeSellPriceRounded, Convert.ToInt32(order.FilledQuantity), $"{order.Symbol} {Messages.Trading.SellOrderPlaced} - {takeSellPriceRounded}");
                                }

                                await Task.Delay(2000);

                                if (takeSellOrderResponse.Status == "filled")
                                {
                                    await _alpacaService.AlpacaLog(brokerAccount.Id, order.Symbol, "SELL", takeSellPriceRounded, Convert.ToInt32(order.FilledQuantity), $"{order.Symbol} {Messages.Trading.OrderExecuted}");
                                }

                                if (stopSellOrderResponse.Status == "accepted")
                                {
                                    await _alpacaService.AlpacaLog(brokerAccount.Id, order.Symbol, "SELL", takeSellPriceRounded, Convert.ToInt32(order.FilledQuantity), $"{order.Symbol} {Messages.Trading.SellOrderPlaced} - {stopSellPriceRounded}");
                                }

                                await Task.Delay(2000);

                                if (stopSellOrderResponse.Status == "filled")
                                {
                                    await _alpacaService.AlpacaLog(brokerAccount.Id, order.Symbol, "SELL", takeSellPriceRounded, Convert.ToInt32(order.FilledQuantity), $"{order.Symbol} {Messages.Trading.OrderExecuted}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{Messages.General.SystemError}: {ex.Message} - BrokerAccountId: {brokerAccount.Id}");
                        await _alpacaService.AlpacaLog(brokerAccount.Id, "-",null, null, null, $"{Messages.General.SystemError}: {ex.Message} - BrokerAccountId: {brokerAccount.Id}");
                    }
                }
            }
        }
    }
}
                                                