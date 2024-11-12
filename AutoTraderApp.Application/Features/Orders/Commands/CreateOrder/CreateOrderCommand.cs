using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;

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

        public CreateOrderCommandHandler(
            IBaseRepository<Order> orderRepository,
            IBaseRepository<Instrument> instrumentRepository,
            IBaseRepository<BrokerAccount> brokerAccountRepository)
        {
            _orderRepository = orderRepository;
            _instrumentRepository = instrumentRepository;
            _brokerAccountRepository = brokerAccountRepository;
        }

        public async Task<IResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var instrument = await _instrumentRepository.GetByIdAsync(request.InstrumentId);
            if (instrument == null)
                return new ErrorResult("Geçersiz enstrüman");

            var brokerAccount = await _brokerAccountRepository.GetByIdAsync(request.BrokerAccountId);
            if (brokerAccount == null)
                return new ErrorResult("Geçersiz broker hesabı");

            if (brokerAccount.UserId != request.UserId)
                return new ErrorResult("Bu broker hesabına erişim yetkiniz yok");

            if (request.Quantity < instrument.MinTradeAmount || request.Quantity > instrument.MaxTradeAmount)
                return new ErrorResult($"Emir miktarı {instrument.MinTradeAmount} ile {instrument.MaxTradeAmount} arasında olmalıdır");

            var order = new Order
            {
                UserId = request.UserId,
                InstrumentId = request.InstrumentId,
                BrokerAccountId = request.BrokerAccountId,
                Type = request.Type,
                Side = request.Side,
                Quantity = request.Quantity,
                Price = (decimal)request.Price,
                StopLoss = request.StopLoss,
                TakeProfit = request.TakeProfit,
                Status = OrderStatus.Created,
                ExternalOrderId = null // Broker'a gönderildiğinde doldurulacak
            };

            await _orderRepository.AddAsync(order);

            return new SuccessResult($"Emir başarıyla oluşturuldu. Emir ID: {order.Id}");
        }
    }
}
