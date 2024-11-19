using FluentValidation;

namespace AutoTraderApp.Application.Features.Strategies.Commands.CreateStrategy;

public class CreateStrategyCommandValidator : AbstractValidator<CreateStrategyCommand>
{
    public CreateStrategyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Strateji adı boş olamaz")
            .MaximumLength(100)
            .WithMessage("Strateji adı en fazla 100 karakter olabilir");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Strateji açıklaması en fazla 500 karakter olabilir");

        RuleFor(x => x.MaxPositionSize)
            .GreaterThan(0)
            .WithMessage("Maksimum pozisyon büyüklüğü 0'dan büyük olmalıdır");

        RuleFor(x => x.StopLossPercentage)
            .GreaterThan(0)
            .WithMessage("Zarar kesme yüzdesi 0'dan büyük olmalıdır")
            .LessThan(100)
            .WithMessage("Zarar kesme yüzdesi 100'den küçük olmalıdır");

        RuleFor(x => x.TakeProfitPercentage)
            .GreaterThan(0)
            .WithMessage("Kar alma yüzdesi 0'dan büyük olmalıdır")
            .LessThan(1000)
            .WithMessage("Kar alma yüzdesi 1000'den küçük olmalıdır");

        RuleFor(x => x.RiskLevel)
            .IsInEnum()
            .WithMessage("Geçerli bir risk seviyesi seçiniz");
    }
}
