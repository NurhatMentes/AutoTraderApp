using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.UserTradingAccounts.DTOs
{
    public class UserTradingAccountDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public DateTime? TwoFactorExpiry { get; set; }
    }
}
