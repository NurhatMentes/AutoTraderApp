using AutoTraderApp.Core.Utilities.Results;

namespace AutoTraderApp.Core.Utilities.Calculators
{
    /// <summary>
    /// Kullanıcının hesap bakiyesi ve risk toleransına göre alım-satım miktarını hesaplar.
    /// </summary>
    public static class QuantityCalculator
    {
        /// <summary>
        /// Alım-satım için miktarı hesaplar.
        /// </summary>
        /// <param name="accountValue">Kullanıcının toplam hesap değeri.</param>
        /// <param name="riskPercentage">Risk yüzdesi.</param>
        /// <param name="entryPrice">Giriş fiyatı.</param>
        /// <param name="stopLoss">Zararı durdurma fiyatı.</param>
        /// <returns>Alınabilecek hisse miktarı.</returns>
        public static int CalculateQuantity(decimal accountValue, decimal riskPercentage, decimal entryPrice, decimal stopLoss)
        {
            if (entryPrice <= stopLoss)
            {
                new ErrorResult("Alış fiyatı, zararı durdurma fiyatından fazla olmalıdır.");
            }

            decimal riskAmount = accountValue * riskPercentage;
            decimal perUnitRisk = entryPrice - stopLoss;         
            int quantity = (int)(riskAmount / perUnitRisk);       

            return quantity > 0 ? quantity : 1;
        }
    }
}
