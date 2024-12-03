using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.MarketData.Alpaca.DTOs
{
    public class MarketDataDto
    {
        public string Symbol { get; set; }
        public decimal LastPrice { get; set; }
        public decimal ChangePercent { get; set; }
        public int Volume { get; set; }
    }
}
