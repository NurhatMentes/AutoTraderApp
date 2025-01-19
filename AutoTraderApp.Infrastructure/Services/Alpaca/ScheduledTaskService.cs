using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;


namespace AutoTraderApp.Infrastructure.Services.Alpaca
{
    public class ScheduledTaskService
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;

        public ScheduledTaskService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ScheduledTaskService hata aldı: {ex.Message}");
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
                            Console.WriteLine($"BrokerAccountId: {brokerAccount.Id} için işlem tamamlandı.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Hata oluştu: {ex.Message} - BrokerAccountId: {brokerAccount.Id}");
                        }
                    }
                }
            }
        }
    }
}
