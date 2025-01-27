using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.UserTradingSettings.DTOs
{
    public class UserTradingSettingsDtoValidator : AbstractValidator<UserTradingSettingsDto>
    {
        public UserTradingSettingsDtoValidator()
        {
            RuleFor(x => x.RiskPercentage)
                .GreaterThan(0).WithMessage("Risk yüzdesi 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(100).WithMessage("Risk yüzdesi 100'den küçük veya eşit olmalıdır.");

            RuleFor(x => x.MaxRiskLimit)
                .GreaterThan(0).WithMessage("Maksimum risk limiti 0'dan büyük olmalıdır.");

            RuleFor(x => x.MinBuyQuantity)
                .GreaterThan(0).WithMessage("Minimum alım miktarı 0'dan büyük olmalıdır.");

            RuleFor(x => x.MaxBuyQuantity)
                .GreaterThanOrEqualTo(x => x.MinBuyQuantity).WithMessage("Maksimum alım miktarı, minimum alım miktarından büyük veya eşit olmalıdır.");

            RuleFor(x => x.BuyPricePercentage)
                .GreaterThan(0).WithMessage("Alım fiyatı yüzdesi 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(100).WithMessage("Alım fiyatı yüzdesi 100'den küçük veya eşit olmalıdır.");

            RuleFor(x => x.SellPricePercentage)
                .GreaterThan(0).WithMessage("Satış fiyatı yüzdesi 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(100).WithMessage("Satış fiyatı yüzdesi 100'den küçük veya eşit olmalıdır.");
        }
    }
}
