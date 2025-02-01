using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.CryptoCustomStocks.Commands
{
    public class DeleteCryptoCustomStockCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCryptoCustomStockCommandHandler : IRequestHandler<DeleteCryptoCustomStockCommand, bool>
    {
        private readonly IBaseRepository<CryptoCustomStock> _repository;

        public DeleteCryptoCustomStockCommandHandler(IBaseRepository<CryptoCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteCryptoCustomStockCommand request, CancellationToken cancellationToken)
        {
            var stock = await _repository.GetByIdAsync(request.Id);
            if (stock == null) return false;

            await _repository.DeleteAsync(stock);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
