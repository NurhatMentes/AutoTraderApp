using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.UserTradingSettings.DTOs
{
    public class UserTradingSettingsDto
    {
        public string BrokerType { get; set; }
        public decimal RiskPercentage { get; set; }
        public decimal MaxRiskLimit { get; set; }
        public int MinBuyQuantity { get; set; }
        public int MaxBuyQuantity { get; set; }
        public decimal BuyPricePercentage { get; set; }
        public decimal SellPricePercentage { get; set; }
    }
}
