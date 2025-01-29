using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.ExternalModels.Binance
{
    public class BinanceAccountResponse
    {
        public List<BinanceBalance> Balances { get; set; } = new();
    }

    public class BinanceBalance
    {
        public string Asset { get; set; } = string.Empty;
        public decimal Free { get; set; }
        public decimal Locked { get; set; }
    }
}
