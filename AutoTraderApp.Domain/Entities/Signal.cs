using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities
{
    public class Signal : BaseEntity
    {
        public string Ticker { get; set; } // Örn: "AAPL"
        public string Strategy { get; set; } // Örn: "Breakout"
        public string Action { get; set; } // Örn: "buy" veya "sell"
        public decimal Price { get; set; } // Alım/satım fiyatı
        public DateTime Timestamp { get; set; } // Sinyal zamanı
    }
}
