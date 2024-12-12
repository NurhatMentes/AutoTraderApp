namespace AutoTraderApp.Application.Features.UserTradingAccounts.DTOs
{
    public class TradingViewLoginDto
    {
        public Guid UserId { get; set; }
        public string Password { get; set; } = null!;
    }
}
