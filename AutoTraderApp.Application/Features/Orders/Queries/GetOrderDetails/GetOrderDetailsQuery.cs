using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Orders.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.Orders.Queries.GetOrderDetails
{
    public class GetOrderDetailsQuery : IRequest<IDataResult<OrderDto>>
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
    }

    public class GetOrderDetailsQueryHandler : IRequestHandler<GetOrderDetailsQuery, IDataResult<OrderDto>>
    {
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IMapper _mapper;

        public GetOrderDetailsQueryHandler(IBaseRepository<Order> orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<IDataResult<OrderDto>> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetListWithExpressionIncludeAsync(
                predicate: o => o.Id == request.OrderId,
                includes: new List<Expression<Func<Order, object>>>
                {
                    o => o.Instrument,
                    o => o.BrokerAccount
                });

            var orderEntity = order.FirstOrDefault();
            if (orderEntity == null)
                return new ErrorDataResult<OrderDto>("Emir bulunamadı");

            if (orderEntity.UserId != request.UserId)
                return new ErrorDataResult<OrderDto>("Bu emri görüntüleme yetkiniz yok");

            var orderDto = _mapper.Map<OrderDto>(orderEntity);
            return new SuccessDataResult<OrderDto>(orderDto);
        }
    }
}
