using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Positions.Commands.UpdatePositionPnL;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace AutoTraderApp.Infrastructure.BackgroundServices
{
    public class PositionUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PositionUpdateService> _logger;

        public PositionUpdateService(
            IServiceProvider serviceProvider,
            ILogger<PositionUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        var positionRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<Position>>();

                        // Açık pozisyonları getir
                        var openPositions = await positionRepository.GetListWithStringIncludeAsync(
                            predicate: p => p.Status == PositionStatus.Open);

                        foreach (var position in openPositions)
                        {
                            // Gerçek uygulamada burası market data service'den güncel fiyatı alacak
                            decimal currentPrice = await GetCurrentPrice(position.InstrumentId);

                            var command = new UpdatePositionPnLCommand
                            {
                                PositionId = position.Id,
                                CurrentPrice = currentPrice
                            };

                            await mediator.Send(command);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating positions");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }

        private async Task<decimal> GetCurrentPrice(Guid instrumentId)
        {
            // Gerçek uygulamada burası market data service'e bağlanacak
            return 0; // Şimdilik dummy değer
        }
    }
}
