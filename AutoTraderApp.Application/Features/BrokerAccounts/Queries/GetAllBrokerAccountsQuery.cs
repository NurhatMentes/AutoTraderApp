using AutoMapper;
using AutoTraderApp.Application.Features.BrokerAccounts.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.BrokerAccounts.Queries
{
    public class GetAllBrokerAccountsQuery : IRequest<IDataResult<List<BrokerAccountDto>>>
    {
        public class GetAllBrokerAccountsQueryHandler : IRequestHandler<GetAllBrokerAccountsQuery, IDataResult<List<BrokerAccountDto>>>
        {
            private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
            private readonly IMapper _mapper;

            public GetAllBrokerAccountsQueryHandler(IBaseRepository<BrokerAccount> brokerAccountRepository, IMapper mapper)
            {
                _brokerAccountRepository = brokerAccountRepository;
                _mapper = mapper;
            }

            public async Task<IDataResult<List<BrokerAccountDto>>> Handle(GetAllBrokerAccountsQuery request, CancellationToken cancellationToken)
            {
                var brokerAccounts = await _brokerAccountRepository.GetAllAsync();
                var brokerAccountDtos = _mapper.Map<List<BrokerAccountDto>>(brokerAccounts);

                return new SuccessDataResult<List<BrokerAccountDto>>(brokerAccountDtos, "Tüm broker hesapları başarıyla getirildi.");
            }
        }
    }
}