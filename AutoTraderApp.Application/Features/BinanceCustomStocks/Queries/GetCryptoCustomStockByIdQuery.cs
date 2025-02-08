using AutoTraderApp.Application.Features.CryptoCustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.CryptoCustomStocks.Queries
{
    public class GetCryptoCustomStockByIdQuery : IRequest<CryptoCustomStockDto>
    {
        public Guid Id { get; set; }
    }

    public class GetCryptoCustomStockByIdQueryHandler : IRequestHandler<GetCryptoCustomStockByIdQuery, CryptoCustomStockDto>
    {
        private readonly IBaseRepository<BinanceCustomStock> _repository;

        public GetCryptoCustomStockByIdQueryHandler(IBaseRepository<BinanceCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<CryptoCustomStockDto> Handle(GetCryptoCustomStockByIdQuery request, CancellationToken cancellationToken)
        {
            var stock = await _repository.GetByIdAsync(request.Id);
            if (stock == null) return null;

            return new CryptoCustomStockDto
            {
                Id = stock.Id,
                Symbol = stock.Symbol,
                CreatedAt = stock.CreatedAt,
                UpdatedAt = (DateTime)stock.UpdatedAt,
                CreatedByUserId = stock.CreatedByUserId,
                UpdatedByUserId = stock.UpdatedByUserId
            };
        }
    }
}
