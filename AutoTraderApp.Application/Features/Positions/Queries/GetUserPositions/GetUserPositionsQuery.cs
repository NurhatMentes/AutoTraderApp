using System.Linq.Expressions;
using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Positions.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;

namespace AutoTraderApp.Application.Features.Positions.Queries.GetUserPositions
{
    public class GetUserPositionsQuery : IRequest<IDataResult<List<PositionDto>>>
    {
        public Guid UserId { get; set; }
        public PositionStatus? Status { get; set; }
        public PositionSide? Side { get; set; }
        public Guid? InstrumentId { get; set; }
    }

    public class GetUserPositionsQueryHandler : IRequestHandler<GetUserPositionsQuery, IDataResult<List<PositionDto>>>
    {
        private readonly IBaseRepository<Position> _positionRepository;
        private readonly IMapper _mapper;

        public GetUserPositionsQueryHandler(IBaseRepository<Position> positionRepository, IMapper mapper)
        {
            _positionRepository = positionRepository;
            _mapper = mapper;
        }

        public async Task<IDataResult<List<PositionDto>>> Handle(GetUserPositionsQuery request, CancellationToken cancellationToken)
        {
            var positions = await _positionRepository.GetListWithExpressionIncludeAsync(
                predicate: p =>
                    p.UserId == request.UserId &&
                    (!request.Status.HasValue || p.Status == request.Status) &&
                    (!request.Side.HasValue || p.Side == request.Side) &&
                    (!request.InstrumentId.HasValue || p.InstrumentId == request.InstrumentId),
                orderBy: q => q.OrderByDescending(p => p.OpenedAt),
                includes: new List<Expression<Func<Position, object>>>
                {
                    p => p.Instrument,
                    p => p.BrokerAccount
                });

            var positionDtos = _mapper.Map<List<PositionDto>>(positions);
            return new SuccessDataResult<List<PositionDto>>(positionDtos);
        }
    }
}
