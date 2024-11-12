using System.Linq.Expressions;
using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Orders.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;

namespace AutoTraderApp.Application.Features.Orders.Queries.GetUserOrders
{
    public class GetUserOrdersQuery : IRequest<IDataResult<List<OrderDto>>>
    {
        public Guid UserId { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, IDataResult<List<OrderDto>>>
    {
        private readonly IBaseRepository<Order> _orderRepository;
        private readonly IMapper _mapper;

        public GetUserOrdersQueryHandler(IBaseRepository<Order> orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<IDataResult<List<OrderDto>>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.GetListWithExpressionIncludeAsync(
                predicate: o =>
                    o.UserId == request.UserId &&
                    (!request.Status.HasValue || o.Status == request.Status) &&
                    (!request.StartDate.HasValue || o.CreatedAt >= request.StartDate) &&
                    (!request.EndDate.HasValue || o.CreatedAt <= request.EndDate),
                orderBy: q => q.OrderByDescending(o => o.CreatedAt),
                includes: new List<Expression<Func<Order, object>>>
                {
                    o => o.Instrument,
                    o => o.BrokerAccount
                });

            // Sayfalama
            var pagedOrders = orders
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var orderDtos = _mapper.Map<List<OrderDto>>(pagedOrders);

            return new SuccessDataResult<List<OrderDto>>(
                orderDtos,
                $"Toplam {orders.Count} emirden {orderDtos.Count} tanesi getirildi");
        }
    }
}
