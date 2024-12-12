using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.ExternalModels.TradingView
{
    public class TradingViewSignalModel
    {
        public string Ticker { get; set; } // Örn: "AAPL"
        public string Strategy { get; set; } // Kullanılan strateji
        public string Action { get; set; } // "al" veya "sat"
        public decimal Price { get; set; } // Fiyat
        public long Timestamp { get; set; } // Unix zaman damgası
    }
}
