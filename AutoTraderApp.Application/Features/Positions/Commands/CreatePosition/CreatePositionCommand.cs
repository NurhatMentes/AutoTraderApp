using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Interfaces;
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
        private readonly IMarketDataService _marketDataService;

        public CreatePositionCommandHandler(
            IBaseRepository<Position> positionRepository,
            IBaseRepository<Order> orderRepository,
            ILogger<CreatePositionCommandHandler> logger,
            IMarketDataService marketDataService)
        {
            _positionRepository = positionRepository;
            _orderRepository = orderRepository;
            _marketDataService = marketDataService;
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

                var currentPrice = await _marketDataService.GetCurrentPrice(orderEntity.Instrument.Symbol);
                if (!currentPrice.HasValue)
                    return new ErrorResult($"{orderEntity.Instrument.Symbol} için fiyat verisi alınamadı");

                var existingPosition = await _positionRepository.GetSingleAsync(p =>
                    p.InstrumentId == orderEntity.InstrumentId &&
                    p.UserId == request.UserId &&
                    p.Status == PositionStatus.Open);

                if (existingPosition != null)
                {
                    _logger.LogInformation($"Existing position found: {existingPosition.Id}");

                    // Aynı yönde ise pozisyonu güncelle
                    if (existingPosition.Side == (orderEntity.Side == OrderSide.Buy ? PositionSide.Long : PositionSide.Short))
                    {
                        decimal totalQuantity = existingPosition.Quantity + orderEntity.Quantity;
                        decimal newEntryPrice = ((existingPosition.EntryPrice * existingPosition.Quantity) +
                            (orderEntity.Price ?? currentPrice.Value) * orderEntity.Quantity) / totalQuantity;

                        existingPosition.Quantity = totalQuantity;
                        existingPosition.EntryPrice = newEntryPrice;
                        existingPosition.CurrentPrice = currentPrice.Value;
                        existingPosition.UnrealizedPnL = CalculateUnrealizedPnL(
                            existingPosition.Side,
                            existingPosition.EntryPrice,
                            currentPrice.Value,
                            existingPosition.Quantity);

                        await _positionRepository.UpdateAsync(existingPosition);
                        return new SuccessResult($"Mevcut pozisyon güncellendi. Pozisyon ID: {existingPosition.Id}");
                    }
                    else
                    {
                        // Ters yönde ise pozisyonu kapat veya azalt
                        if (orderEntity.Quantity >= existingPosition.Quantity)
                        {
                            existingPosition.Status = PositionStatus.Closed;
                            existingPosition.ClosedAt = DateTime.UtcNow;
                            existingPosition.RealizedPnL = CalculateRealizedPnL(
                                existingPosition.Side,
                                existingPosition.EntryPrice,
                                currentPrice.Value,
                                existingPosition.Quantity);

                            await _positionRepository.UpdateAsync(existingPosition);
                            return new SuccessResult($"Pozisyon kapatıldı. Pozisyon ID: {existingPosition.Id}");
                        }
                        else
                        {
                            existingPosition.Quantity -= orderEntity.Quantity;
                            existingPosition.RealizedPnL = CalculateRealizedPnL(
                                existingPosition.Side,
                                existingPosition.EntryPrice,
                                currentPrice.Value,
                                orderEntity.Quantity);

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
                    EntryPrice = orderEntity.Price ?? currentPrice.Value,
                    CurrentPrice = currentPrice.Value,
                    Side = orderEntity.Side == OrderSide.Buy ? PositionSide.Long : PositionSide.Short,
                    Status = PositionStatus.Open,
                    UnrealizedPnL = 0, // Yeni pozisyon için başlangıç PnL'i 0
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
                _logger.LogError(ex, "Error creating position");
                return new ErrorResult("Pozisyon oluşturulurken bir hata oluştu");
            }
        }

        private decimal CalculateUnrealizedPnL(PositionSide side, decimal entryPrice, decimal currentPrice, decimal quantity)
        {
            return side == PositionSide.Long
                ? (currentPrice - entryPrice) * quantity
                : (entryPrice - currentPrice) * quantity;
        }

        private decimal CalculateRealizedPnL(PositionSide side, decimal entryPrice, decimal exitPrice, decimal quantity)
        {
            return side == PositionSide.Long
                ? (exitPrice - entryPrice) * quantity
                : (entryPrice - exitPrice) * quantity;
        }
    }
}
