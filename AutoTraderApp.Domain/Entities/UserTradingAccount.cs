using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities
{
    public class UserTradingAccount : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string EncryptedPassword { get; set; } = null!;
        public string PasswordSalt { get; set; } = null!;
        public DateTime? TwoFactorExpiry { get; set; }
    }
}
