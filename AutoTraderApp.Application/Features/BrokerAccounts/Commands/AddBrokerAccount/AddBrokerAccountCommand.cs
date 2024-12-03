using AutoMapper;
using AutoTraderApp.Application.Features.BrokerAccounts.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using MediatR;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.BrokerAccounts.Commands.AddBrokerAccount
{
    public class AddBrokerAccountCommand : IRequest<IResult>
    {
        public AddBrokerAccountDto BrokerAccountDto { get; set; }

        public class AddBrokerAccountCommandHandler : IRequestHandler<AddBrokerAccountCommand, IResult>
        {
            private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
            private readonly IMapper _mapper;

            public AddBrokerAccountCommandHandler(IBaseRepository<BrokerAccount> brokerAccountRepository, IMapper mapper)
            {
                _brokerAccountRepository = brokerAccountRepository;
                _mapper = mapper;
            }

            public async Task<IResult> Handle(AddBrokerAccountCommand request, CancellationToken cancellationToken)
            {
                var brokerAccount = _mapper.Map<BrokerAccount>(request.BrokerAccountDto);

                await _brokerAccountRepository.AddAsync(brokerAccount);
                return new SuccessResult("Broker hesabı başarıyla eklendi.");
            }
        }
    }
}
