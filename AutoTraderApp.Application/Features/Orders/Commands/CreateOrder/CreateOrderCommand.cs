using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutoTraderApp.Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<IResult>
    {
        public Guid UserId { get; set; }
        public Guid InstrumentId { get; set; }
        public Guid BrokerAccountId { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public decimal Quantity { get; set; }
        public decimal? Price { get; set; }  // Limit emirler için
        public decimal? StopLoss { get; set; }
        public decimal? TakeProfit { get; set; }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, IResult>
    {
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IBaseRepository<Instrument> _instrumentRepository;
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(
            IBaseRepository<Order> orderRepository,
            IBaseRepository<Instrument> instrumentRepository,
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _instrumentRepository = instrumentRepository;
            _brokerAccountRepository = brokerAccountRepository;
            _logger = logger;
        }

        public async Task<IResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Creating order: {JsonSerializer.Serialize(request)}");

                // 1. Enstrüman kontrolü
                var instrument = await _instrumentRepository.GetByIdAsync(request.InstrumentId);
                if (instrument == null)
                {
                    _logger.LogWarning($"Invalid instrument ID: {request.InstrumentId}");
                    return new ErrorResult("Geçersiz enstrüman");
                }

                // 2. Broker hesap kontrolü
                var brokerAccount = await _brokerAccountRepository.GetByIdAsync(request.BrokerAccountId);
                if (brokerAccount == null)
                {
                    _logger.LogWarning($"Invalid broker account ID: {request.BrokerAccountId}");
                    return new ErrorResult("Geçersiz broker hesabı");
                }

                // 3. Hesap yetki kontrolü
                if (brokerAccount.UserId != request.UserId)
                {
                    _logger.LogWarning($"User {request.UserId} tried to access broker account {request.BrokerAccountId}");
                    return new ErrorResult("Bu broker hesabına erişim yetkiniz yok");
                }

                // 4. İşlem miktarı kontrolü
                if (request.Quantity < instrument.MinTradeAmount || request.Quantity > instrument.MaxTradeAmount)
                {
                    var order = new Order
                    {
                        UserId = request.UserId,
                        InstrumentId = request.InstrumentId,
                        BrokerAccountId = request.BrokerAccountId,
                        Type = request.Type,
                        Side = request.Side,
                        Quantity = request.Quantity,
                        Price = request.Price,
                        StopLoss = request.StopLoss,
                        TakeProfit = request.TakeProfit,
                        Status = OrderStatus.Rejected,
                        RejectionReason = $"İşlem miktarı limitlerin dışında. Min: {instrument.MinTradeAmount}, Max: {instrument.MaxTradeAmount}"
                    };
                    await _orderRepository.AddAsync(order);
                    return new ErrorResult(order.RejectionReason);
                }

                // 5. Bakiye kontrolü (Market emirler için anlık fiyat, Limit emirler için limit fiyatı kullanılır)
                decimal requiredAmount = request.Quantity * (request.Price ?? 0); // Gerçek uygulamada market fiyatı servisten alınmalı
                if (brokerAccount.Balance < requiredAmount)
                {
                    var order = new Order
                    {
                        UserId = request.UserId,
                        InstrumentId = request.InstrumentId,
                        BrokerAccountId = request.BrokerAccountId,
                        Type = request.Type,
                        Side = request.Side,
                        Quantity = request.Quantity,
                        Price = request.Price,
                        StopLoss = request.StopLoss,
                        TakeProfit = request.TakeProfit,
                        Status = OrderStatus.Rejected,
                        RejectionReason = $"Yetersiz bakiye. Gerekli: {requiredAmount}, Mevcut: {brokerAccount.Balance}"
                    };
                    await _orderRepository.AddAsync(order);
                    return new ErrorResult(order.RejectionReason);
                }

                var successfulOrder = new Order
                {
                    UserId = request.UserId,
                    InstrumentId = request.InstrumentId,
                    BrokerAccountId = request.BrokerAccountId,
                    Type = request.Type,
                    Side = request.Side,
                    Quantity = request.Quantity,
                    Price = request.Price,
                    StopLoss = request.StopLoss,
                    TakeProfit = request.TakeProfit,
                    Status = OrderStatus.Created
                };

                await _orderRepository.AddAsync(successfulOrder);
                _logger.LogInformation($"Order created successfully: {successfulOrder.Id}");

                return new SuccessResult($"Emir başarıyla oluşturuldu. Emir ID: {successfulOrder.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex}");
                return new ErrorResult("Emir oluşturulurken bir hata oluştu");
            }
        }
    }
}

