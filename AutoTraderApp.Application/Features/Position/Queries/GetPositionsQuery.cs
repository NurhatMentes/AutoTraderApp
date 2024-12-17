using AutoMapper;
using AutoTraderApp.Application.Features.Portfolio.DTOs;
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
            var positions = await _alpacaService.GetPositionsAsync(request.BrokerAccountId);

            try
            {
                var positionDto = _mapper.Map<List<PositionDto>>(positions);
                return new SuccessDataResult<List<PositionDto>>(positionDto, "Pozisyonlar başarıyla alındı.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<PositionDto>>(null, $"Pozisyonlar alınamadı: {ex.Message}");
            }
           
        }

    }
}
