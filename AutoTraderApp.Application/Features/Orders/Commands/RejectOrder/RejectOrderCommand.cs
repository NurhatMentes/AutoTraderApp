using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.Orders.Commands.RejectOrder
{
    public class RejectOrderCommand : IRequest<IResult>
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string RejectionReason { get; set; }
    }

    public class RejectOrderCommandHandler : IRequestHandler<RejectOrderCommand, IResult>
    {
        private readonly IBaseRepository<Order> _orderRepository;

        public RejectOrderCommandHandler(IBaseRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IResult> Handle(RejectOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);

            if (order == null)
                return new ErrorResult("Emir bulunamadı");

            // Sadece admin veya emri oluşturan kullanıcı reddedebilir
            if (order.UserId != request.UserId)
                return new ErrorResult("Bu emri reddetme yetkiniz yok");

            // Sadece belirli durumdaki emirler reddedilebilir
            if (order.Status != OrderStatus.Created && order.Status != OrderStatus.Pending)
                return new ErrorResult($"Bu emir {order.Status} durumunda olduğu için reddedilemez");

            order.Status = OrderStatus.Rejected;
            order.RejectionReason = request.RejectionReason;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            return new SuccessResult("Emir başarıyla reddedildi");
        }
    }
}
