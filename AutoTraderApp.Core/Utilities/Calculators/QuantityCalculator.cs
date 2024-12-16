using AutoTraderApp.Core.Utilities.Results;

namespace AutoTraderApp.Core.Utilities.Calculators
{
    public static class QuantityCalculator
    {
        public static int CalculateQuantity(decimal portfolioValue, decimal riskPercentage, decimal entryPrice, decimal stopLoss)
        {
            if (entryPrice <= stopLoss)
            {
                new ErrorResult("Alış fiyatı, zararı durdurma fiyatından fazla olmalıdır.");
            }

            decimal riskAmount = portfolioValue * riskPercentage; // Alınabilecek risk tutarı
            decimal perUnitRisk = entryPrice - stopLoss;          // Bir birim başına risk
            int quantity = (int)(riskAmount / perUnitRisk);       // Alınabilecek miktar

            return quantity > 0 ? quantity : 1; 
        }
    }
}
