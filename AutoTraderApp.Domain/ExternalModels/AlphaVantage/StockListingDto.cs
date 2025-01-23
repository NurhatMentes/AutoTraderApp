namespace AutoTraderApp.Domain.ExternalModels.AlphaVantage
{
    public class StockListingDto
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Exchange { get; set; }
        public string AssetType { get; set; }
        public string IpoDate { get; set; }
        public string DelistingDate { get; set; }
        public string Status { get; set; }
    }
}
