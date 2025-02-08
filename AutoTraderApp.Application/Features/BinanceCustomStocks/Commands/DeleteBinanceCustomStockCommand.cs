using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.BinanceCustomStocks.Commands
{
    public class DeleteBinanceCustomStockCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCryptoCustomStockCommandHandler : IRequestHandler<DeleteBinanceCustomStockCommand, bool>
    {
        private readonly IBaseRepository<BinanceCustomStock> _repository;

        public DeleteCryptoCustomStockCommandHandler(IBaseRepository<BinanceCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteBinanceCustomStockCommand request, CancellationToken cancellationToken)
        {
            var stock = await _repository.GetByIdAsync(request.Id);
            if (stock == null) return false;

            await _repository.DeleteAsync(stock);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
