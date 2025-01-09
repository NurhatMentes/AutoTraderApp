namespace AutoTraderApp.Application.Features.Strategies.DTOs
{
    public class StrategyDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string StrategyName { get; set; }
        public string Symbol { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
        public string TimeFrame { get; set; }
        public string WebhookUrl { get; set; }
    }
}
