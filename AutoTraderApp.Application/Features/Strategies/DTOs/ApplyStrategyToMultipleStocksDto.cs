namespace AutoTraderApp.Application.Features.Strategies.DTOs
{
    public class ApplyStrategyToMultipleStocksDto
    {
        public Guid StrategyId { get; set; }
        public Guid BrokerAccountId { get; set; }
        public Guid UserId { get; set; }
        public List<string> Symbols { get; set; }
    }

}
