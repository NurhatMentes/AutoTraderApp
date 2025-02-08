using AutoTraderApp.Application.Features.CryptoCustomStocks.DTOs;
using FluentValidation;

namespace AutoTraderApp.Application.Features.CryptoCustomStocks.Validators
{
    public class CreateCryptoCustomStockValidator : AbstractValidator<CreateBinanceCustomStockDto>
    {
        public CreateCryptoCustomStockValidator()
        {
            RuleFor(x => x.Symbol)
                .NotEmpty().WithMessage("Kripto ismi boş olamaz.")
                .Length(2, 50).WithMessage("Kripto ismi 2 ile 50 karakter arasında olmalıdır.");
        }
    }
}
