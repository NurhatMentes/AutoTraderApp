namespace AutoTraderApp.Application.Features.Position.DTOs
{
    public class PositionDto
    {
        public string Symbol { get; set; } // Örn: "AAPL"
        public decimal Quantity { get; set; } // Pozisyondaki miktar
        public decimal EntryPrice { get; set; } // Ortalama giriş fiyatı
        public decimal MarketValue { get; set; } // Pozisyonun piyasa değeri
        public decimal CostBasis { get; set; } // Ortalama maliyet
        public decimal UnrealizedPnL { get; set; } // Gerçekleşmemiş kar/zarar
        public decimal UnrealizedPnLPercentage { get; set; } // Gerçekleşmemiş kar/zarar yüzdesi
        public decimal CurrentPrice { get; set; } // Şu anki fiyat
        public decimal RealizedPnL { get; set; } // Gerçekleşmiş kar/zarar
        public decimal TodayChange { get; set; } // Günlük değişim yüzdesi
        public bool IsOpen { get; set; } = true;
    }
}