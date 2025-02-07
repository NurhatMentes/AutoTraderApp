namespace AutoTraderApp.Application.Features.UserTradingSettings.DTOs
{
    public class UserTradingSettingsCreateDto
    {
        public decimal RiskPercentage { get; set; }
        public decimal MaxRiskLimit { get; set; }
        public int MinBuyQuantity { get; set; }
        public int MaxBuyQuantity { get; set; }
        public decimal BuyPricePercentage { get; set; }
        public decimal SellPricePercentage { get; set; }
        public decimal MinBuyPrice { get; set; }
    }
}
