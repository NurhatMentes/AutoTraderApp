using AutoMapper;
using AutoTraderApp.Application.Features.Position.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;      

namespace AutoTraderApp.Application.Features.Position.Queries
{
    public class GetPositionsQuery : IRequest<IDataResult<List<PositionDto>>> { }

    public class GetPositionsQueryHandler : IRequestHandler<GetPositionsQuery, IDataResult<List<PositionDto>>>
    {
        private readonly IAlpacaService _alpacaService;
        private readonly IBaseRepository<Domain.Entities.Position> _positionRepository;
        private readonly IMapper _mapper;

        public GetPositionsQueryHandler(
            IAlpacaService alpacaService,
            IBaseRepository<Domain.Entities.Position> positionRepository,
            IMapper mapper)
        {
            _alpacaService = alpacaService;
            _positionRepository = positionRepository;
            _mapper = mapper;
        }

        public async Task<IDataResult<List<PositionDto>>> Handle(GetPositionsQuery request, CancellationToken cancellationToken)
        {
            var alpacaPositions = await _alpacaService.GetPositionsAsync();

            if (alpacaPositions == null)
                return new ErrorDataResult<List<PositionDto>>("Pozisyonlar alınamadı.");

            foreach (var alpacaPosition in alpacaPositions)
            {
                var position = _mapper.Map<Domain.Entities.Position>(alpacaPosition);

                var existingPosition = await _positionRepository.GetAsync(p => p.Symbol == position.Symbol && p.IsOpen);

                if (existingPosition == null)
                {
                    await _positionRepository.AddAsync(position);
                }
                else
                {
                    existingPosition.CostBasis = position.CostBasis;
                    existingPosition.TodayChange = position.TodayChange;
                    existingPosition.Quantity = position.Quantity;
                    existingPosition.CurrentPrice = position.CurrentPrice;
                    existingPosition.MarketValue = position.MarketValue;
                    existingPosition.UnrealizedPnL = position.UnrealizedPnL;
                    existingPosition.UnrealizedPnLPercentage = position.UnrealizedPnLPercentage;
                    existingPosition.RealizedPnL = position.RealizedPnL;

                    await _positionRepository.UpdateAsync(existingPosition);
                }
            }

            var positions = await _positionRepository.GetListWithExpressionIncludeAsync(p => p.IsOpen);
            var positionDtos = _mapper.Map<List<PositionDto>>(positions);

            return new SuccessDataResult<List<PositionDto>>(positionDtos, "Pozisyonlar başarıyla alındı.");
        }

    }
}
