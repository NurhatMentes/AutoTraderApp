using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Positions.Commands.UpdatePositionPnL;
using AutoTraderApp.Application.Interfaces;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using AutoTraderApp.Infrastructure.MarketData.Models;
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
                        var marketDataService = scope.ServiceProvider.GetRequiredService<IMarketDataService>();
                        var instrumentRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<Instrument>>();

                        // Açık pozisyonları getir
                        var openPositions = await positionRepository.GetListWithStringIncludeAsync(
                            predicate: p => p.Status == PositionStatus.Open);

                        foreach (var position in openPositions)
                        {
                            // Enstrümanın sembolünü al
                            var instrument = await instrumentRepository.GetByIdAsync(position.InstrumentId);
                            if (instrument == null)
                            {
                                _logger.LogWarning($"Enstrüman bulunamadı. InstrumentId: {position.InstrumentId}");
                                continue;
                            }

                            decimal? currentPrice = await marketDataService.GetCurrentPrice(instrument.Symbol);
                            if (!currentPrice.HasValue)
                            {
                                _logger.LogWarning($"Güncel fiyat alınamadı. Sembol: {instrument.Symbol}");
                                continue;
                            }

                            _logger.LogInformation($"Pozisyon güncelleniyor. PositionId: {position.Id}, CurrentPrice: {currentPrice}");


                            var command = new UpdatePositionPnLCommand
                            {
                                PositionId = position.Id,
                                CurrentPrice = currentPrice.Value
                            };

                            await mediator.Send(command);
                        }
                    }

                    // Servisin güncelleme sıklığı
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Pozisyonlar güncellenirken hata oluştu.");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }
    }
}
