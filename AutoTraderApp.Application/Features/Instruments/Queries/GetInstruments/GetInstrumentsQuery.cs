using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Enums;
using MediatR;
using AutoTraderApp.Application.Features.Instruments.DTOs;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.Instruments.Queries.GetInstruments
{
    public class GetInstrumentsQuery : IRequest<IDataResult<List<InstrumentDto>>>
    {
        public InstrumentType? Type { get; set; }
        public InstrumentStatus? Status { get; set; }
    }

    public class GetInstrumentsQueryHandler : IRequestHandler<GetInstrumentsQuery, IDataResult<List<InstrumentDto>>>
    {
        private readonly IBaseRepository<Instrument> _instrumentRepository;
        private readonly IMapper _mapper;

        public GetInstrumentsQueryHandler(IBaseRepository<Instrument> instrumentRepository, IMapper mapper)
        {
            _instrumentRepository = instrumentRepository;
            _mapper = mapper;
        }

        public async Task<IDataResult<List<InstrumentDto>>> Handle(GetInstrumentsQuery request, CancellationToken cancellationToken)
        {
            var instruments = await _instrumentRepository.GetListWithStringIncludeAsync(
                predicate: i => (!request.Type.HasValue || i.Type == request.Type.Value) &&
                                (!request.Status.HasValue || i.Status == request.Status.Value),
                orderBy: q => q.OrderBy(i => i.Symbol));

            var instrumentDtos = _mapper.Map<List<InstrumentDto>>(instruments);
            return new SuccessDataResult<List<InstrumentDto>>(instrumentDtos, "Enstrümanlar başarıyla listelendi");
        }
    }
}
