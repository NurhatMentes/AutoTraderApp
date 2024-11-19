using System.Linq.Expressions;
using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Strategies.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using MediatR;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.Strategies.Queries
{
    public class GetStrategyDetailQuery : IRequest<IDataResult<StrategyDetailDto>>
    {
        public Guid Id { get; set; }

        public class GetStrategyDetailQueryHandler : IRequestHandler<GetStrategyDetailQuery, IDataResult<StrategyDetailDto>>
        {
            private readonly IBaseRepository<Strategy> _strategyRepository;
            private readonly IMapper _mapper;

            public GetStrategyDetailQueryHandler(IBaseRepository<Strategy> strategyRepository, IMapper mapper)
            {
                _strategyRepository = strategyRepository;
                _mapper = mapper;
            }

            public async Task<IDataResult<StrategyDetailDto>> Handle(GetStrategyDetailQuery request, CancellationToken cancellationToken)
            {
                var includes = new List<Expression<Func<Strategy, object>>>
                {
                    x => x.TradingRules,
                    x => x.User
                };

                var strategy = await _strategyRepository.GetListWithExpressionIncludeAsync(
                    predicate: s => s.Id == request.Id,
                    includes: includes);

                if (!strategy.Any())
                    return new ErrorDataResult<StrategyDetailDto>("Strateji bulunamadı.");

                var strategyDetailDto = _mapper.Map<StrategyDetailDto>(strategy.First());
                return new SuccessDataResult<StrategyDetailDto>(strategyDetailDto, "Strateji detayı başarıyla getirildi.");
            }
        }
    }
}
