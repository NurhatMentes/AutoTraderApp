using FluentValidation;

namespace AutoTraderApp.Application.Features.Instruments.Commands.CreateInstrument;

public class CreateInstrumentCommandValidator : AbstractValidator<CreateInstrumentCommand>
{
    public CreateInstrumentCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Sembol boş olamaz")
            .MaximumLength(20).WithMessage("Sembol 20 karakterden uzun olamaz");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("İsim boş olamaz")
            .MaximumLength(100).WithMessage("İsim 100 karakterden uzun olamaz");

        RuleFor(x => x.Exchange)
            .NotEmpty().WithMessage("Borsa bilgisi boş olamaz")
            .MaximumLength(50).WithMessage("Borsa bilgisi 50 karakterden uzun olamaz");

        RuleFor(x => x.MinTradeAmount)
            .GreaterThan(0).WithMessage("Minimum işlem miktarı 0'dan büyük olmalıdır");

        RuleFor(x => x.MaxTradeAmount)
            .GreaterThan(x => x.MinTradeAmount)
            .WithMessage("Maksimum işlem miktarı minimum işlem miktarından büyük olmalıdır");
    }
}