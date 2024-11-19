using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Strategies.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.Strategies.Queries
{
    public class GetStrategiesQuery : IRequest<IDataResult<List<StrategyListDto>>>
    {
        public class GetStrategiesQueryHandler : IRequestHandler<GetStrategiesQuery, IDataResult<List<StrategyListDto>>>
        {
            private readonly IBaseRepository<Strategy> _strategyRepository;
            private readonly IMapper _mapper;
            private readonly ILogger<GetStrategiesQueryHandler> _logger;

            public GetStrategiesQueryHandler(IBaseRepository<Strategy> strategyRepository,
                IMapper mapper,
                ILogger<GetStrategiesQueryHandler> logger)
            {
                _strategyRepository = strategyRepository;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<IDataResult<List<StrategyListDto>>> Handle(GetStrategiesQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("GetStrategiesQuery handling started");

                try
                {
                    var strategies = await _strategyRepository.GetListWithStringIncludeAsync(
                        includeString: "TradingRules");

                    var strategiesDto = _mapper.Map<List<StrategyListDto>>(strategies);

                    _logger.LogInformation("Successfully retrieved {Count} strategies", strategiesDto.Count);
                    return new SuccessDataResult<List<StrategyListDto>>(strategiesDto, "Stratejiler başarıyla listelendi.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while retrieving strategies");
                    return new ErrorDataResult<List<StrategyListDto>>("Stratejiler listelenirken bir hata oluştu.");
                }
            }
        }
    }
}
