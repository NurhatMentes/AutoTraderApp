using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Enums;
using AutoTraderApp.Domain.ValueObjects;

namespace AutoTraderApp.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public Email Email { get; set; } = null!;
        public byte[] PasswordHash { get; set; } = null!;
        public byte[] PasswordSalt { get; set; } = null!;
        public bool IsTwoFactorEnabled { get; set; }
        public string? TwoFactorSecret { get; set; }
        public string? ProfileImageUrl { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Inactive;

        public virtual ICollection<UserOperationClaim> UserOperationClaims { get; set; }
        public User()
        {
            UserOperationClaims = new HashSet<UserOperationClaim>();
        }
    }
}
