using AutoMapper;
using AutoTraderApp.Application.Features.BrokerAccounts.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.BrokerAccounts.Queries
{
    public class GetAccountInfoQuery : IRequest<IDataResult<AccountInfoDto>>
    {
        public Guid brokerAccountId { get; set; }
    }
    public class GetAccountInfoQueryHandler : IRequestHandler<GetAccountInfoQuery, IDataResult<AccountInfoDto>>
    {
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly IAlpacaService _alpacaService;
        private readonly IMapper _mapper;

        public GetAccountInfoQueryHandler(
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaService alpacaService,
            IMapper mapper)
        {
            _brokerAccountRepository = brokerAccountRepository;
            _alpacaService = alpacaService;
            _mapper = mapper;
        }

        public async Task<IDataResult<AccountInfoDto>> Handle(GetAccountInfoQuery request, CancellationToken cancellationToken)
        {
            var brokerAccount = await _brokerAccountRepository.GetByIdAsync(request.brokerAccountId);
            if (brokerAccount == null)
                return new ErrorDataResult<AccountInfoDto>("Broker hesabı bulunamadı.");

            try
            {
                var accountInfo = await _alpacaService.GetAccountInfoAsync(brokerAccount.Id);

                var accountInfoDto = _mapper.Map<AccountInfoDto>(accountInfo);
                return new SuccessDataResult<AccountInfoDto>(accountInfoDto, "Hesap bilgileri başarıyla alındı.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<AccountInfoDto>($"Hesap bilgileri alınırken bir hata oluştu: {ex.Message}");
            }
        }

    }
}