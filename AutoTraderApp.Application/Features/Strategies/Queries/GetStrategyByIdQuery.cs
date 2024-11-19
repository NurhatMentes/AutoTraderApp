using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Strategies.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoTraderApp.Application.Features.Strategies.Queries;

public class GetStrategyByIdQuery : IRequest<IDataResult<StrategyDto>>
{
    public Guid Id { get; set; }

    public class GetStrategyByIdQueryHandler : IRequestHandler<GetStrategyByIdQuery, IDataResult<StrategyDto>>
    {
        private readonly IBaseRepository<Strategy> _strategyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetStrategyByIdQueryHandler> _logger;

        public GetStrategyByIdQueryHandler(IBaseRepository<Strategy> strategyRepository,
            IMapper mapper,
            ILogger<GetStrategyByIdQueryHandler> logger)
        {
            _strategyRepository = strategyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IDataResult<StrategyDto>> Handle(GetStrategyByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetStrategyByIdQuery handling started for ID: {StrategyId}", request.Id);

            try
            {
                var strategy = await _strategyRepository.GetAsync(
                    predicate: s => s.Id == request.Id,
                    includeString: "TradingRules");

                if (strategy == null)
                {
                    _logger.LogWarning("Strategy not found with ID: {StrategyId}", request.Id);
                    return new ErrorDataResult<StrategyDto>("Strateji bulunamadı.");
                }

                var strategyDto = _mapper.Map<StrategyDto>(strategy);

                _logger.LogInformation("Successfully retrieved strategy with ID: {StrategyId}", request.Id);
                return new SuccessDataResult<StrategyDto>(strategyDto, "Strateji başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving strategy with ID: {StrategyId}", request.Id);
                return new ErrorDataResult<StrategyDto>("Strateji getirilirken bir hata oluştu.");
            }
        }
    }
}