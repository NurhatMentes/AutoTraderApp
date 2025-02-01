using AutoTraderApp.Application.Features.CryptoCustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.CryptoCustomStocks.Commands
{
    public class CreateCryptoCustomStockCommand : IRequest<Guid>
    {
        public CreateCryptoCustomStockDto Dto { get; set; }
    }

    public class CreateCryptoCustomStockCommandHandler : IRequestHandler<CreateCryptoCustomStockCommand, Guid>
    {
        private readonly IBaseRepository<CryptoCustomStock> _repository;

        public CreateCryptoCustomStockCommandHandler(IBaseRepository<CryptoCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CreateCryptoCustomStockCommand request, CancellationToken cancellationToken)
        {
            var stock = new CryptoCustomStock
            {
                Symbol = request.Dto.Symbol
            };

            await _repository.AddAsync(stock);
            await _repository.SaveChangesAsync();

            return stock.Id;
        }
    }
}
