using FluentValidation;

namespace AutoTraderApp.Application.Features.Strategies.Commands.UpdateStrategy;

public class UpdateStrategyCommandValidator : AbstractValidator<UpdateStrategyCommand>
{
    public UpdateStrategyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Strateji ID'si boş olamaz.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Strateji adı boş olamaz.")
            .MinimumLength(3).WithMessage("Strateji adı en az 3 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Strateji adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Strateji açıklaması boş olamaz.")
            .MaximumLength(500).WithMessage("Strateji açıklaması en fazla 500 karakter olabilir.");

        RuleFor(x => x.MaxPositionSize)
            .GreaterThan(0).WithMessage("Maksimum pozisyon büyüklüğü 0'dan büyük olmalıdır.");

        RuleFor(x => x.StopLossPercentage)
            .GreaterThan(0).WithMessage("Zarar durdurma yüzdesi 0'dan büyük olmalıdır.")
            .LessThan(100).WithMessage("Zarar durdurma yüzdesi 100'den küçük olmalıdır.");

        RuleFor(x => x.TakeProfitPercentage)
            .GreaterThan(0).WithMessage("Kar alma yüzdesi 0'dan büyük olmalıdır.");

        RuleFor(x => x.TradingRules)
            .NotEmpty().WithMessage("En az bir alım-satım kuralı belirtilmelidir.");
    }
}