using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;

namespace AutoTraderApp.Application.Features.Orders.Commands.CancelOrder
{
    public class CancelOrderCommand : IRequest<IResult>
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
    }

    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, IResult>
    {
        private readonly IBaseRepository<Order> _orderRepository;

        public CancelOrderCommandHandler(IBaseRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);

            if (order == null)
                return new ErrorResult("Emir bulunamadı");

            if (order.UserId != request.UserId)
                return new ErrorResult("Bu emri iptal etme yetkiniz yok");

            if (order.Status == OrderStatus.Filled || order.Status == OrderStatus.Cancelled)
                return new ErrorResult($"Bu emir zaten {order.Status.ToString().ToLower()} durumunda");

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            return new SuccessResult("Emir başarıyla iptal edildi");
        }
    }
}
