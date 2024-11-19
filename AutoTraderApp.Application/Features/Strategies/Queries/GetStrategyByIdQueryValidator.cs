using FluentValidation;

namespace AutoTraderApp.Application.Features.Strategies.Queries;

public class GetStrategyByIdQueryValidator : AbstractValidator<GetStrategyByIdQuery>
{
    public GetStrategyByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Strateji ID'si boş olamaz.")
            .NotEqual(Guid.Empty).WithMessage("Geçerli bir strateji ID'si girilmelidir.");
    }
}