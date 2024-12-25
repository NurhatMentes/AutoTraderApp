namespace AutoTraderApp.Application.Features.Strategies.DTOs
{
    public class CreateStrategyRequest
    {
        public string StrategyName { get; set; }
        public string Symbol { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
        public string TimeFrame { get; set; }
        public string WebhookUrl { get; set; }
        public int AtrLength { get; set; } = 14;
        public int BollingerLength { get; set; } = 20;
        public float BollingerMultiplier { get; set; } = 1.5f;
        public int DmiLength { get; set; } = 14;
        public int AdxSmoothing { get; set; } = 14;
        public float AdxThreshold { get; set; } = 25;
        public int RsiLength { get; set; } = 14;
        public float RsiUpper { get; set; } = 70;
        public float RsiLower { get; set; } = 30;
        public int StochRsiLength { get; set; } = 14;
        public float StochRsiUpper { get; set; } = 0.8f;
        public float StochRsiLower { get; set; } = 0.2f;
    }

}
