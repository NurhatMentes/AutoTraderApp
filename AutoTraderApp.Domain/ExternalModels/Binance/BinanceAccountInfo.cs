using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Domain.ExternalModels.Binance
{
    public class BinanceAccountInfo
    {
        public int MakerCommission { get; set; }
        public int TakerCommission { get; set; }
        public int BuyerCommission { get; set; }
        public int SellerCommission { get; set; }
        public bool CanTrade { get; set; }
        public bool CanWithdraw { get; set; }
        public bool CanDeposit { get; set; }
        public long UpdateTime { get; set; }
        public string AccountType { get; set; } = null!;
        public List<BinanceBalance> Balances { get; set; }
    }
}
