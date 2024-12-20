namespace AutoTraderApp.Domain.ExternalModels.AlphaVantage
{
    public class LoserDto
    {
        public string Ticker { get; set; }
        public decimal Price { get; set; }
        public decimal ChangeAmount { get; set; }
        public string ChangePercentage { get; set; }
        public long Volume { get; set; }
    }
}
