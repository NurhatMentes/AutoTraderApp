using AutoTraderApp.Application.Features.OkxCustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.OkxCustomStocks.Queries
{
    public class GetAllOkxCustomStocksQuery : IRequest<List<OkxCustomStockDto>> { }

    public class GetAllOkxCustomStocksQueryHandler : IRequestHandler<GetAllOkxCustomStocksQuery, List<OkxCustomStockDto>>
    {
        private readonly IBaseRepository<OkxCustomStock> _repository;

        public GetAllOkxCustomStocksQueryHandler(IBaseRepository<OkxCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<List<OkxCustomStockDto>> Handle(GetAllOkxCustomStocksQuery request, CancellationToken cancellationToken)
        {
            var stocks = await _repository.GetAllAsync();
            return stocks.Select(s => new OkxCustomStockDto
            {
                Id = s.Id,
                Symbol = s.Symbol,
                CreatedAt = s.CreatedAt,
                UpdatedAt = (DateTime)s.UpdatedAt,
                CreatedByUserId = s.CreatedByUserId,
                UpdatedByUserId = s.UpdatedByUserId
            }).ToList();
        }
    }
}
