using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.TradingView.DTOs
{
    public class TradingViewCryptoSignalDto
    {
        public Guid BrokerAccountId { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } = null!;
        public string Symbol { get; set; } = null!;
        public decimal Quantity { get; set; }
        public bool IsMarginTrade { get; set; }
    }
}
