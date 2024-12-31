using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.Strategies.Helpers
{
    /// <summary>
    /// Kullanıcının hesap bakiyesine ve risk faktörlerine göre hisse seçim işlemleri.
    /// </summary>
    public static class StockSelectionHelper
    {
        /// <summary>
        /// Kullanıcının bakiyesine göre hisse seçim kriterlerini uygular.
        /// </summary>
        /// <param name="combinedStocks">Birleşik (kombine) hisse listesi.</param>
        /// <param name="accountValue">Kullanıcının toplam hesap değeri.</param>
        /// <returns>Seçilen hisselerin listesi.</returns>
        public static IEnumerable<CombinedStock> SelectStocks(IEnumerable<CombinedStock> combinedStocks, decimal accountValue)
        {
            // Hisse fiyatı 50'nin altında olanları filtrele
            var filteredStocks = accountValue < 500
                ? combinedStocks.Where(cs => cs.Price <= 50).ToList()
                : combinedStocks.ToList();

            // Bakiye oranına göre hisse seçimi
            int selectionCount = accountValue switch
            {
                <= 500 => (int)(filteredStocks.Count * 0.3), 
                <= 1000 => (int)(filteredStocks.Count * 0.5),
                _ => filteredStocks.Count 
            };

            Console.WriteLine($"Seçilen hisse sayısı: {selectionCount}");
            return filteredStocks.Take(selectionCount);
        }

        /// <summary>
        /// Kullanıcının hesap değerine göre risk yüzdesini belirler.
        /// </summary>
        /// <param name="accountValue">Kullanıcının toplam hesap değeri.</param>
        /// <returns>Risk yüzdesi.</returns>
        public static decimal CalculateRiskPercentage(decimal accountValue)
        {
            if (accountValue < 500) return 0.07m;
            return 0.05m; 
        }
    }
}
