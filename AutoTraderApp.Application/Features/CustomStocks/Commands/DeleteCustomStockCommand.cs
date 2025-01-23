using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.CustomStocks.Commands
{
    public class DeleteCustomStockCommand : IRequest
    {
        public Guid Id { get; set; }
    }
    public class DeleteCustomStockCommandHandler : IRequestHandler<DeleteCustomStockCommand>
    {
        private readonly IBaseRepository<CustomStock> _repository;

        public DeleteCustomStockCommandHandler(IBaseRepository<CustomStock> repository)
        {
            _repository = repository;
        }

        public async Task Handle(DeleteCustomStockCommand request, CancellationToken cancellationToken)
        {
            var stock = await _repository.GetByIdAsync(request.Id);
            if (stock != null)
            {
                await _repository.DeleteAsync(stock);
                await _repository.SaveChangesAsync();
            }
        }
    }
}
