using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;

namespace AutoTraderApp.Application.Features.Orders.Commands.FillOrder
{
    public class FillOrderCommand : IRequest<IResult>
    {
        public Guid OrderId { get; set; }
        public decimal ExecutedPrice { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }

    public class FillOrderCommandHandler : IRequestHandler<FillOrderCommand, IResult>
    {
        private readonly IBaseRepository<Order> _orderRepository;

        public FillOrderCommandHandler(IBaseRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IResult> Handle(FillOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                return new ErrorResult("Emir bulunamadı");

            order.Status = OrderStatus.Filled;
            order.Price = request.ExecutedPrice;
            order.UpdatedAt = request.ExecutedAt;

            await _orderRepository.UpdateAsync(order);
            return new SuccessResult("Emir gerçekleşti");
        }
    }
}
