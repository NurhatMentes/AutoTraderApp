using AutoMapper;
using AutoTraderApp.Application.Features.Portfolio.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.Portfolio.Queries
{
    public class GetPortfolioQuery : IRequest<IDataResult<List<PortfolioDto>>> 
    {
        public Guid brokerId { get; set; }
    }

    public class GetPortfolioQueryHandler : IRequestHandler<GetPortfolioQuery, IDataResult<List<PortfolioDto>>>
    {
        private readonly IAlpacaService _alpacaService;
        private readonly IMapper _mapper;

        public GetPortfolioQueryHandler(IAlpacaService alpacaService, IMapper mapper)
        {
            _alpacaService = alpacaService;
            _mapper = mapper;
        }

        public async Task<IDataResult<List<PortfolioDto>>> Handle(GetPortfolioQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var portfolio = await _alpacaService.GetPortfolioAsync();
                var portfolioDto = _mapper.Map<List<PortfolioDto>>(portfolio);
                return new SuccessDataResult<List<PortfolioDto>>(portfolioDto, "Portföy başarıyla alındı.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<PortfolioDto>>(null, $"Portföy alınırken bir hata oluştu: {ex.Message}");
            }
        }
    }
}
