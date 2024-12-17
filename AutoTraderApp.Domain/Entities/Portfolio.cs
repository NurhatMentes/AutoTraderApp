using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities
{
    public class Portfolio : BaseEntity
    {
        public Guid BrokerAccountId { get; set; }
        public string Symbol { get; set; } // Örn: "AAPL"
        public decimal Quantity { get; set; } // Pozisyondaki miktar
        public decimal MarketValue { get; set; } // Şu anki piyasa değeri
        public decimal CostBasis { get; set; } // Ortalama maliyet
        public decimal UnrealizedPnL { get; set; } // Gerçekleşmemiş kar/zarar
        public decimal CurrentPrice { get; set; } // Şu anki fiyat
    }
}
