using AutoMapper;
using AutoTraderApp.Application.Features.CombinedStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using MediatR;

namespace AutoTraderApp.Application.Features.CombinedStocks.Queries
{
    public class GetCombinedStocksQuery : IRequest<IDataResult<List<CombinedStockDto>>> { }

    public class GetCombinedStocksQueryHandler : IRequestHandler<GetCombinedStocksQuery, IDataResult<List<CombinedStockDto>>>
    {
        private readonly IBaseRepository<Domain.Entities.CombinedStock> _repository;
        private readonly IMapper _mapper;

        public GetCombinedStocksQueryHandler(IBaseRepository<Domain.Entities.CombinedStock> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IDataResult<List<CombinedStockDto>>> Handle(GetCombinedStocksQuery request, CancellationToken cancellationToken)
        {
            var stocks = await _repository.GetAllAsync();
            if (stocks == null || !stocks.Any())
            {
                return new ErrorDataResult<List<CombinedStockDto>>(null, "Kombine hisse senedi bulunamadı.");
            }

            var stockDtos = _mapper.Map<List<CombinedStockDto>>(stocks);
            return new SuccessDataResult<List<CombinedStockDto>>(stockDtos, "Kombine hisseler başarıyla alındı.");
        }
    }
}
