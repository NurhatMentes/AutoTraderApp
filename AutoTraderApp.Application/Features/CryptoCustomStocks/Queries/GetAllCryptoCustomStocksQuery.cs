using AutoTraderApp.Application.Features.CryptoCustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.CryptoCustomStocks.Queries
{
    public class GetAllCryptoCustomStocksQuery : IRequest<List<CryptoCustomStockDto>> { }

    public class GetAllCryptoCustomStocksQueryHandler : IRequestHandler<GetAllCryptoCustomStocksQuery, List<CryptoCustomStockDto>>
    {
        private readonly IBaseRepository<CryptoCustomStock> _repository;

        public GetAllCryptoCustomStocksQueryHandler(IBaseRepository<CryptoCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<List<CryptoCustomStockDto>> Handle(GetAllCryptoCustomStocksQuery request, CancellationToken cancellationToken)
        {
            var stocks = await _repository.GetAllAsync();
            return stocks.Select(s => new CryptoCustomStockDto
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
