using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.ExternalModels.Binance
{
    public class BinancePosition
    {
        public string Symbol { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
