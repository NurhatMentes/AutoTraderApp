
namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models
{
    public class ActivityResponse
    {
        public string Symbol { get; set; }
        public decimal Quantity { get; set; } 
        public decimal Price { get; set; } 
        public string Side { get; set; } 
        public DateTime TransactionTime { get; set; } 
    }

}
