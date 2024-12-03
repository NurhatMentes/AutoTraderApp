using AutoTraderApp.Application.Features.BrokerAccounts.DTOs;
using FluentValidation;

namespace AutoTraderApp.Application.Features.BrokerAccounts.Commands.AddBrokerAccount;

public class AddBrokerAccountValidator : AbstractValidator<AddBrokerAccountDto>
{
    public AddBrokerAccountValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Hesap adı boş olamaz.");
        RuleFor(x => x.ApiKey).NotEmpty().WithMessage("API Anahtarı boş olamaz.");
        RuleFor(x => x.ApiSecret).NotEmpty().WithMessage("API Secret boş olamaz.");
    }
}