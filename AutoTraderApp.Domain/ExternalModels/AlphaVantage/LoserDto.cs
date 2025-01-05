namespace AutoTraderApp.Domain.ExternalModels.AlphaVantage
{
    public class LoserDto
    {
        public string Ticker { get; set; }
        public string Price { get; set; }
        public string ChangeAmount { get; set; }
        public string ChangePercentage { get; set; }
        public long Volume { get; set; }
    }
}
