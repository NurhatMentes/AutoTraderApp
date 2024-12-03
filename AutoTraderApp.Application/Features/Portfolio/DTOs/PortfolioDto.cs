using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.Portfolio.DTOs
{
    public class PortfolioDto
    {
        public string Symbol { get; set; } // Örn: "AAPL"
        public decimal Quantity { get; set; } // Pozisyondaki miktar
        public decimal MarketValue { get; set; } // Şu anki piyasa değeri
        public decimal CostBasis { get; set; } // Ortalama maliyet
        public decimal UnrealizedPnL { get; set; } // Gerçekleşmemiş kar/zarar
        public decimal CurrentPrice { get; set; } // Şu anki fiyat
    }
}
