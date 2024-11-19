using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Interfaces;
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
        private readonly IMarketDataService _marketDataService;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(
            IBaseRepository<Order> orderRepository,
            IBaseRepository<Instrument> instrumentRepository,
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IMarketDataService marketDataService,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _instrumentRepository = instrumentRepository;
            _brokerAccountRepository = brokerAccountRepository;
            _marketDataService = marketDataService;
            _logger = logger;
        }

        public async Task<IResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Creating order: {JsonSerializer.Serialize(request)}");

                var instrument = await _instrumentRepository.GetByIdAsync(request.InstrumentId);
                if (instrument == null)
                    return new ErrorResult("Geçersiz enstrüman");

                var brokerAccount = await _brokerAccountRepository.GetByIdAsync(request.BrokerAccountId);
                if (brokerAccount == null)
                    return new ErrorResult("Geçersiz broker hesabı");

                if (brokerAccount.UserId != request.UserId)
                    return new ErrorResult("Bu broker hesabına erişim yetkiniz yok");

                // Market fiyatını al
                var currentPrice = await _marketDataService.GetCurrentPrice(instrument.Symbol);
                if (!currentPrice.HasValue)
                    return new ErrorResult("Fiyat bilgisi alınamadı");

                decimal orderPrice = request.Type == OrderType.Market ? currentPrice.Value : request.Price ?? currentPrice.Value;

                if (request.Quantity < instrument.MinTradeAmount || request.Quantity > instrument.MaxTradeAmount)
                {
                    return await CreateRejectedOrder(request, instrument, $"İşlem miktarı limitlerin dışında. Min: {instrument.MinTradeAmount}, Max: {instrument.MaxTradeAmount}");
                }

                decimal requiredAmount = request.Quantity * orderPrice;
                if (brokerAccount.Balance < requiredAmount)
                {
                    return await CreateRejectedOrder(request, instrument, $"Yetersiz bakiye. Gerekli: {requiredAmount}, Mevcut: {brokerAccount.Balance}");
                }

                var order = new Order
                {
                    UserId = request.UserId,
                    InstrumentId = request.InstrumentId,
                    BrokerAccountId = request.BrokerAccountId,
                    Type = request.Type,
                    Side = request.Side,
                    Quantity = request.Quantity,
                    Price = orderPrice,
                    StopLoss = request.StopLoss,
                    TakeProfit = request.TakeProfit,
                    Status = OrderStatus.Created
                };

                await _orderRepository.AddAsync(order);
                _logger.LogInformation($"Order created successfully: {order.Id}");

                // Market emri ise Pending statüsüne çek
                // Broker entegrasyonu yapıldığında burada broker'a gönderilecek
                if (request.Type == OrderType.Market)
                {
                    order.Status = OrderStatus.Pending;
                    await _orderRepository.UpdateAsync(order);
                    _logger.LogInformation($"Market order status updated to Pending: {order.Id}");
                }

                return new SuccessResult($"Emir başarıyla oluşturuldu. Emir ID: {order.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex}");
                return new ErrorResult("Emir oluşturulurken bir hata oluştu");
            }
        }

        private async Task<IResult> CreateRejectedOrder(CreateOrderCommand request, Instrument instrument, string reason)
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
                RejectionReason = reason
            };
            await _orderRepository.AddAsync(order);
            return new ErrorResult(reason);
        }
    }
}

