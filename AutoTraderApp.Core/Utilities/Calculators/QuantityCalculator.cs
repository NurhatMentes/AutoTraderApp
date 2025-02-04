using AutoTraderApp.Core.Utilities.Results;
using System;

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
        public static int StockCalculateQuantity(
            decimal accountValue, 
            decimal riskPercentage, 
            decimal entryPrice, decimal stopLoss,
            decimal maxBuyingPowerPercent, 
            int minBuyQuantity, 
            int maxBuyQuantity,
            decimal minBuyPrice)
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

            int quantity = (int)(Math.Floor(riskAmount / perUnitRisk));

            decimal totalPrice = quantity * entryPrice; 

            if (totalPrice < minBuyPrice)
            {
                quantity = (int)Math.Ceiling(minBuyPrice / entryPrice);
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

        public static decimal CalculateCryptoQuantity(
           decimal accountBalance,
           decimal riskPercentage,
           decimal cryptoPrice,
           decimal maxRiskLimit,
           decimal maxBuyQuantity,
           decimal minBuyPrice)
        {
            decimal riskAmount = accountBalance * riskPercentage;

            if (riskAmount > maxRiskLimit)
            {
                riskAmount = maxRiskLimit;
            }

            decimal quantity = riskAmount / cryptoPrice;

            if (quantity > maxBuyQuantity)
            {
                quantity = maxBuyQuantity;
            }

            decimal totalPrice = quantity * cryptoPrice;

            if (totalPrice < minBuyPrice)
            {
                quantity = Math.Ceiling(minBuyPrice / cryptoPrice);
            }

            return Math.Round(quantity, 8);
        }

    }
}
