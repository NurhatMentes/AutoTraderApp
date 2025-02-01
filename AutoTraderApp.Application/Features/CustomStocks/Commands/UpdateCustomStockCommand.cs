using AutoTraderApp.Application.Features.CustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.CustomStocks.Commands
{
    public class UpdateCustomStockCommand : IRequest
    {
        public UpdateCustomStockDto Dto { get; set; }
    }

    public class UpdateCustomStockCommandHandler : IRequestHandler<UpdateCustomStockCommand>
    {
        private readonly IBaseRepository<CustomStock> _repository;

        public UpdateCustomStockCommandHandler(IBaseRepository<CustomStock> repository)
        {
            _repository = repository;
        }

        public async Task Handle(UpdateCustomStockCommand request, CancellationToken cancellationToken)
        {
            var stock = await _repository.GetByIdAsync(request.Dto.Id);
            if (stock != null)
            {
                stock.Symbol = request.Dto.Symbol;
                stock.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(stock);
                await _repository.SaveChangesAsync();
            }
        }
    }
}
