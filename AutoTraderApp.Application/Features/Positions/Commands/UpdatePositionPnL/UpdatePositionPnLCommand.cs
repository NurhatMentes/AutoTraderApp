using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoTraderApp.Application.Features.Positions.Commands.UpdatePositionPnL
{
    public class UpdatePositionPnLCommand : IRequest<IResult>
    {
        public Guid PositionId { get; set; }
        public decimal CurrentPrice { get; set; }
    }

    public class UpdatePositionPnLCommandHandler : IRequestHandler<UpdatePositionPnLCommand, IResult>
    {
        private readonly IBaseRepository<Position> _positionRepository;
        private readonly ILogger<UpdatePositionPnLCommandHandler> _logger;

        public UpdatePositionPnLCommandHandler(
            IBaseRepository<Position> positionRepository,
            ILogger<UpdatePositionPnLCommandHandler> logger)
        {
            _positionRepository = positionRepository;
            _logger = logger;
        }

        public async Task<IResult> Handle(UpdatePositionPnLCommand request, CancellationToken cancellationToken)
        {
            var position = await _positionRepository.GetByIdAsync(request.PositionId);
            if (position == null)
                return new ErrorResult("Pozisyon bulunamadı");

            _logger.LogInformation($"Updating position {position.Id} PnL. Current price: {request.CurrentPrice}");

            position.CurrentPrice = request.CurrentPrice;

            // UnrealizedPnL hesaplama
            if (position.Side == PositionSide.Long)
            {
                position.UnrealizedPnL = (position.CurrentPrice - position.EntryPrice) * position.Quantity;
                _logger.LogInformation($"Long position PnL: ({position.CurrentPrice} - {position.EntryPrice}) * {position.Quantity} = {position.UnrealizedPnL}");
            }
            else
            {
                position.UnrealizedPnL = (position.EntryPrice - position.CurrentPrice) * position.Quantity;
                _logger.LogInformation($"Short position PnL: ({position.EntryPrice} - {position.CurrentPrice}) * {position.Quantity} = {position.UnrealizedPnL}");
            }

            // Stop Loss kontrolü
            if (position.StopLoss.HasValue)
            {
                if ((position.Side == PositionSide.Long && position.CurrentPrice <= position.StopLoss.Value) ||
                    (position.Side == PositionSide.Short && position.CurrentPrice >= position.StopLoss.Value))
                {
                    _logger.LogWarning($"Position {position.Id} hit stop loss at price {position.CurrentPrice}");
                    position.Status = PositionStatus.Closed;
                }
            }

            // Take Profit kontrolü
            if (position.TakeProfit.HasValue)
            {
                if ((position.Side == PositionSide.Long && position.CurrentPrice >= position.TakeProfit.Value) ||
                    (position.Side == PositionSide.Short && position.CurrentPrice <= position.TakeProfit.Value))
                {
                    _logger.LogInformation($"Position {position.Id} hit take profit at price {position.CurrentPrice}");
                    position.Status = PositionStatus.Closed;
                }
            }

            await _positionRepository.UpdateAsync(position);
            return new SuccessResult($"Pozisyon PnL güncellendi. Güncel PnL: {position.UnrealizedPnL}");
        }
    }
}
