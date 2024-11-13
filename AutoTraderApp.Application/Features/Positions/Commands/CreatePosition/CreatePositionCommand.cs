using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace AutoTraderApp.Application.Features.Positions.Commands.CreatePosition
{
    public class CreatePositionCommand : IRequest<IResult>
    {
        public Guid OrderId { get; set; }  
        public Guid UserId { get; set; }  
    }


    public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, IResult>
    {
        private readonly IBaseRepository<Position> _positionRepository;
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly ILogger<CreatePositionCommandHandler> _logger;

        public CreatePositionCommandHandler(
            IBaseRepository<Position> positionRepository,
            IBaseRepository<Order> orderRepository,
            ILogger<CreatePositionCommandHandler> logger)
        {
            _positionRepository = positionRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<IResult> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orderRepository.GetListWithExpressionIncludeAsync(
                    predicate: o => o.Id == request.OrderId && o.UserId == request.UserId,
                    includes: new List<Expression<Func<Order, object>>>
                    {
                    o => o.Instrument,
                    o => o.BrokerAccount
                    });

                var orderEntity = order.FirstOrDefault();
                if (orderEntity == null)
                    return new ErrorResult("Emir bulunamadı veya bu emre erişim yetkiniz yok");

                if (orderEntity.Status != OrderStatus.Filled)
                    return new ErrorResult("Bu emir henüz gerçekleşmemiş");

                // Mevcut açık pozisyonu kontrol et
                var existingPosition = await _positionRepository.GetSingleAsync(p =>
                    p.InstrumentId == orderEntity.InstrumentId &&
                    p.UserId == request.UserId &&
                    p.Status == PositionStatus.Open);

                if (existingPosition != null)
                {
                    // Aynı yönde ise pozisyonu güncelle
                    if (existingPosition.Side == (orderEntity.Side == OrderSide.Buy ? PositionSide.Long : PositionSide.Short))
                    {
                        existingPosition.Quantity += orderEntity.Quantity;
                        existingPosition.EntryPrice = ((existingPosition.EntryPrice * (existingPosition.Quantity - orderEntity.Quantity)) +
                            (orderEntity.Price ?? 0) * orderEntity.Quantity) / existingPosition.Quantity;

                        await _positionRepository.UpdateAsync(existingPosition);
                        return new SuccessResult($"Mevcut pozisyon güncellendi. Pozisyon ID: {existingPosition.Id}");
                    }
                    else
                    {
                        if (orderEntity.Quantity >= existingPosition.Quantity)
                        {
                            existingPosition.Status = PositionStatus.Closed;
                            existingPosition.ClosedAt = DateTime.UtcNow;
                            existingPosition.RealizedPnL = CalculateRealizedPnL(existingPosition, orderEntity);

                            await _positionRepository.UpdateAsync(existingPosition);
                            return new SuccessResult($"Pozisyon kapatıldı. Pozisyon ID: {existingPosition.Id}");
                        }
                        else
                        {
                            existingPosition.Quantity -= orderEntity.Quantity;
                            await _positionRepository.UpdateAsync(existingPosition);
                            return new SuccessResult($"Pozisyon güncellendi. Pozisyon ID: {existingPosition.Id}");
                        }
                    }
                }

                var position = new Position
                {
                    UserId = request.UserId,
                    InstrumentId = orderEntity.InstrumentId,
                    BrokerAccountId = orderEntity.BrokerAccountId,
                    Quantity = orderEntity.Quantity,
                    EntryPrice = orderEntity.Price ?? 0,
                    CurrentPrice = orderEntity.Price ?? 0,
                    Side = orderEntity.Side == OrderSide.Buy ? PositionSide.Long : PositionSide.Short,
                    Status = PositionStatus.Open,
                    UnrealizedPnL = 0,
                    RealizedPnL = 0,
                    StopLoss = orderEntity.StopLoss,
                    TakeProfit = orderEntity.TakeProfit
                };

                await _positionRepository.AddAsync(position);

                _logger.LogInformation($"Position created successfully: {position.Id}");
                return new SuccessResult($"Pozisyon başarıyla oluşturuldu. Pozisyon ID: {position.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating position: {ex}");
                return new ErrorResult("Pozisyon oluşturulurken bir hata oluştu");
            }
        }

        private decimal CalculateRealizedPnL(Position position, Order closingOrder)
        {
            var closingPrice = closingOrder.Price ?? 0;
            if (position.Side == PositionSide.Long)
            {
                return (closingPrice - position.EntryPrice) * position.Quantity;
            }
            return (position.EntryPrice - closingPrice) * position.Quantity;
        }
    }
}
