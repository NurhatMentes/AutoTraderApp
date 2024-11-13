using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;

namespace AutoTraderApp.Application.Features.Positions.Commands.ClosePosition
{
    public class ClosePositionCommand : IRequest<IResult>
    {
        public Guid PositionId { get; set; }
        public Guid UserId { get; set; }
        public decimal? ClosePrice { get; set; }  // Market veya limit fiyatı
    }

    public class ClosePositionCommandHandler : IRequestHandler<ClosePositionCommand, IResult>
    {
        private readonly IBaseRepository<Position> _positionRepository;
        private readonly IBaseRepository<Order> _orderRepository;

        public ClosePositionCommandHandler(
            IBaseRepository<Position> positionRepository,
            IBaseRepository<Order> orderRepository)
        {
            _positionRepository = positionRepository;
            _orderRepository = orderRepository;
        }

        public async Task<IResult> Handle(ClosePositionCommand request, CancellationToken cancellationToken)
        {
            var position = await _positionRepository.GetByIdAsync(request.PositionId);

            if (position == null)
                return new ErrorResult("Pozisyon bulunamadı");

            if (position.UserId != request.UserId)
                return new ErrorResult("Bu pozisyonu kapatma yetkiniz yok");

            if (position.Status != PositionStatus.Open)
                return new ErrorResult("Bu pozisyon zaten kapatılmış");

            // Pozisyonu kapatmak için karşıt emir oluştur
            var closeOrder = new Order
            {
                UserId = request.UserId,
                InstrumentId = position.InstrumentId,
                BrokerAccountId = position.BrokerAccountId,
                Type = request.ClosePrice.HasValue ? OrderType.Limit : OrderType.Market,
                Side = position.Side == PositionSide.Long ? OrderSide.Sell : OrderSide.Buy,
                Quantity = position.Quantity,
                Price = (decimal)request.ClosePrice,
                Status = OrderStatus.Created
            };

            await _orderRepository.AddAsync(closeOrder);

            // Pozisyon durumunu güncelle
            position.Status = PositionStatus.Closed;
            position.UpdatedAt = DateTime.UtcNow;
            await _positionRepository.UpdateAsync(position);

            return new SuccessResult($"Pozisyon kapatma emri verildi. Emir ID: {closeOrder.Id}");
        }
    }
}
