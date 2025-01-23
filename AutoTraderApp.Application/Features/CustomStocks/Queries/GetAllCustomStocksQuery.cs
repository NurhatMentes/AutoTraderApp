using AutoTraderApp.Application.Features.CustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.CustomStocks.Queries
{
    public class GetAllCustomStocksQuery : IRequest<List<CustomStockDto>>
    {
    }

    public class GetAllCustomStocksQueryHandler : IRequestHandler<GetAllCustomStocksQuery, List<CustomStockDto>>
    {
        private readonly IBaseRepository<CustomStock> _repository;

        public GetAllCustomStocksQueryHandler(IBaseRepository<CustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<List<CustomStockDto>> Handle(GetAllCustomStocksQuery request, CancellationToken cancellationToken)
        {
            var stocks = await _repository.GetAllAsync();
            return stocks.Select(s => new CustomStockDto
            {
                Id = s.Id,
                Symbol = s.Symbol,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                CreatedByUserId = s.CreatedByUserId,
                UpdatedByUserId = s.UpdatedByUserId
            }).ToList();
        }
    }
}
