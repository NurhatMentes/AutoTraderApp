using AutoTraderApp.Application.Features.CryptoCustomStocks.DTOs;
using FluentValidation;

namespace AutoTraderApp.Application.Features.CryptoCustomStocks.Validators
{
    public class UpdateCryptoCustomStockValidator : AbstractValidator<UpdateCryptoCustomStockDto>
    {
        public UpdateCryptoCustomStockValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID boş olamaz.");

            RuleFor(x => x.Symbol)
                .NotEmpty().WithMessage("Kripto ismi boş olamaz.")
                .Length(2, 50).WithMessage("Kripto ismi 2 ile 50 karakter arasında olmalıdır.");
        }
    }
}
