namespace AutoTraderApp.Domain.ExternalModels.TradingView
{
    public class SignalRequest
    {
        public string Action { get; set; }
        public string Symbol { get; set; }
        public int Quantity { get; set; } 
        public decimal Price { get; set; } 
    }
}
