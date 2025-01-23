namespace AutoTraderApp.Application.Features.CustomStocks.DTOs
{
    public class UpdateCustomStockDto
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}
