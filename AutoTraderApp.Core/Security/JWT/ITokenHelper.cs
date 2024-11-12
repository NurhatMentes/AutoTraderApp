using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Core.Security.JWT
{
    public interface ITokenHelper
    {
        AccessToken CreateToken(User user, List<OperationClaim> operationClaims);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
