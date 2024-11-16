using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Interfaces;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace AutoTraderApp.Application.Features.Positions.Commands.ClosePosition
{
    public class ClosePositionCommand : IRequest<IResult>
    {
        public Guid PositionId { get; set; }
        public Guid UserId { get; set; }
    }

    public class ClosePositionCommandHandler : IRequestHandler<ClosePositionCommand, IResult>
    {
        private readonly IBaseRepository<Position> _positionRepository;
        private readonly IMarketDataService _marketDataService;
        private readonly ILogger<ClosePositionCommandHandler> _logger;

        public ClosePositionCommandHandler(
            IBaseRepository<Position> positionRepository,
            IMarketDataService marketDataService,
            ILogger<ClosePositionCommandHandler> logger)
        {
            _positionRepository = positionRepository;
            _marketDataService = marketDataService;
            _logger = logger;
        }

        public async Task<IResult> Handle(ClosePositionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var position = await _positionRepository.GetListWithExpressionIncludeAsync(
                    predicate: p => p.Id == request.PositionId && p.UserId == request.UserId,
                    includes: new List<Expression<Func<Position, object>>>
                    {
                    p => p.Instrument
                    });

                var currentPosition = position.FirstOrDefault();
                if (currentPosition == null)
                    return new ErrorResult("Pozisyon bulunamadı veya bu pozisyona erişim yetkiniz yok");

                if (currentPosition.Status != PositionStatus.Open)
                    return new ErrorResult($"Bu pozisyon {currentPosition.Status} durumunda");

                // Güncel piyasa fiyatını al
                var currentPrice = await _marketDataService.GetCurrentPrice(currentPosition.Instrument.Symbol);
                if (!currentPrice.HasValue)
                    return new ErrorResult($"{currentPosition.Instrument.Symbol} için fiyat verisi alınamadı");

                currentPosition.RealizedPnL = CalculateRealizedPnL(
                    currentPosition.Side,
                    currentPosition.EntryPrice,
                    currentPrice.Value,
                    currentPosition.Quantity);

                currentPosition.Status = PositionStatus.Closed;
                currentPosition.ClosedAt = DateTime.UtcNow;
                currentPosition.CurrentPrice = currentPrice.Value;
                currentPosition.UnrealizedPnL = 0; // Pozisyon kapandığı için UnrealizedPnL sıfırlanır

                await _positionRepository.UpdateAsync(currentPosition);

                return new SuccessResult(
                    $"Pozisyon kapatıldı. RealizedPnL: {currentPosition.RealizedPnL:F2}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing position {PositionId}", request.PositionId);
                return new ErrorResult("Pozisyon kapatılırken bir hata oluştu");
            }
        }

        private decimal CalculateRealizedPnL(PositionSide side, decimal entryPrice, decimal exitPrice, decimal quantity)
        {
            return side == PositionSide.Long
                ? (exitPrice - entryPrice) * quantity
                : (entryPrice - exitPrice) * quantity;
        }
    }
}
