using AutoMapper;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.Position.Commands.ClosePosition
{
    public class ClosePositionCommand : IRequest<IResult>
    {
        public string Symbol { get; set; }
        public decimal Quantity { get; set; }
    }

    public class ClosePositionCommandHandler : IRequestHandler<ClosePositionCommand, IResult>
    {
        private readonly IBaseRepository<Domain.Entities.Position> _positionRepository;
        private readonly IBaseRepository<ClosedPosition> _closedPositionRepository;
        private readonly IAlpacaService _alpacaService;

        public ClosePositionCommandHandler(
            IBaseRepository<Domain.Entities.Position> positionRepository,
            IBaseRepository<ClosedPosition> closedPositionRepository,
            IAlpacaService alpacaService)
        {
            _positionRepository = positionRepository;
            _closedPositionRepository = closedPositionRepository;
            _alpacaService = alpacaService;
        }

        public async Task<IResult> Handle(ClosePositionCommand request, CancellationToken cancellationToken)
        {
            var positionCheck = await _alpacaService.GetPositionsAsync(request.BrokerAccountId);
            var position = await _positionRepository.GetAsync(p => p.Symbol == request.Symbol && p.IsOpen);

            if (position == null)
                return new ErrorResult("Pozisyon bulunamadı veya zaten kapalı.");

            if (request.Quantity > position.Quantity)
                return new ErrorResult("Kapatılacak miktar pozisyon miktarından büyük olamaz.");

            var alpacaResult = await _alpacaService.ClosePositionAsync(request.Symbol, request.Quantity);

            if (!alpacaResult.Success)
                return new ErrorResult($"Alpaca API hatası: {alpacaResult.Message}");

            var realizedPnL = (position.CurrentPrice - position.EntryPrice) * request.Quantity;

            var closedPosition = new ClosedPosition
            {
                Symbol = position.Symbol,
                Quantity = request.Quantity,
                RealizedPnL = realizedPnL,
                ClosedAt = DateTime.UtcNow
            };

            if (request.Quantity == position.Quantity)
            {
                position.IsOpen = false;
                position.Quantity = 0;
            }
            else
            {
                position.Quantity -= request.Quantity;
            }

            await _closedPositionRepository.AddAsync(closedPosition);
            await _positionRepository.UpdateAsync(position);

            return new SuccessResult("Pozisyon başarıyla kapatıldı.");
        }
    }

}
