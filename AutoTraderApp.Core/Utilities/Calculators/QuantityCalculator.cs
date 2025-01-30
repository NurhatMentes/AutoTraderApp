using AutoTraderApp.Core.Utilities.Results;

namespace AutoTraderApp.Core.Utilities.Calculators
{
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
        public static int StockCalculateQuantity(decimal accountValue, decimal riskPercentage, decimal entryPrice, decimal stopLoss, decimal maxBuyingPowerPercent, int minBuyQuantity, int maxBuyQuantity)
        {
            if (entryPrice <= stopLoss)
            {
                throw new ArgumentException("Alış fiyatı, zararı durdurma fiyatından fazla olmalıdır.");
            }

            // Risk miktarını hesapla
            decimal riskAmount = accountValue * riskPercentage;

            // Hisse başına risk miktarını hesapla
            decimal perUnitRisk = entryPrice - stopLoss;
            if (perUnitRisk <= 0)
            {
                throw new ArgumentException("Stop-loss seviyesi giriş fiyatından büyük olamaz.");
            }

            // Hesaplanan miktarı bul
            int quantity = (int)(Math.Floor(riskAmount / perUnitRisk));

            // Toplam fiyatı hesapla
            decimal totalPrice = quantity * entryPrice;

            // Eğer toplam fiyat 2500 doların altındaysa, miktarı artır
            if (totalPrice < 2500)
            {
                quantity = (int)Math.Ceiling(2500 / entryPrice);
            }

            // Maksimum alım gücüne göre miktarı sınırla
            decimal maxQuantityForBuyingPower = (int)(maxBuyingPowerPercent / entryPrice);
            if (quantity > maxQuantityForBuyingPower)
            {
                quantity = (int)maxQuantityForBuyingPower;
            }

            // Kullanıcının belirlediği minimum ve maksimum alım miktarlarına göre sınırla
            if (quantity < minBuyQuantity)
            {
                quantity = minBuyQuantity;
            }
            else if (quantity > maxBuyQuantity)
            {
                quantity = maxBuyQuantity;
            }

            return quantity > 0 ? quantity : 1;
        }

        public static decimal CalculateCryptoQuantity(decimal accountBalance, decimal riskPercentage, decimal cryptoPrice, decimal maxRiskLimit)
        {
            decimal riskAmount = accountBalance * riskPercentage;

            if (riskAmount > maxRiskLimit)
            {
                riskAmount = maxRiskLimit;
            }

            if (cryptoPrice <= 0)
            {
                throw new Exception("Geçersiz kripto fiyatı.");
            }

            decimal quantity = riskAmount / cryptoPrice;

            return Math.Round(quantity, 8); 
        }
    }
}
