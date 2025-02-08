using AutoTraderApp.Application.Features.CryptoCustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.BinanceCustomStocks.Commands
{
    public class CreateBinanceCustomStockCommand : IRequest<IResult>
    {
        public CreateBinanceCustomStockDto Dto { get; set; }
    }

    public class CreateCryptoCustomStockCommandHandler : IRequestHandler<CreateBinanceCustomStockCommand, IResult>
    {
        private readonly IBaseRepository<BinanceCustomStock> _repository;

        public CreateCryptoCustomStockCommandHandler(IBaseRepository<BinanceCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<IResult> Handle(CreateBinanceCustomStockCommand request, CancellationToken cancellationToken)
        {
            var isSymbol = _repository.GetAsync(x => x.Symbol == request.Dto.Symbol).Result;
            if (isSymbol != null)
            {
                return new ErrorResult($"{request.Dto.Symbol} sembolü sistemde mevcut.");
            }

            var stock = new BinanceCustomStock
            {
                Symbol = request.Dto.Symbol
            };

            await _repository.AddAsync(stock);
            await _repository.SaveChangesAsync();

            return new SuccessResult($"{request.Dto.Symbol} sembolü başarılıyla kaydedildi.");
        }
    }
}
