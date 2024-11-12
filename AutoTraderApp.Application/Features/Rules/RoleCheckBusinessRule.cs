using System.Security.Claims;
using AutoTraderApp.Core.Utilities.Results;

namespace AutoTraderApp.Application.Features.Rules;

public class RoleCheckBusinessRule : BusinessRule
{
    private readonly ClaimsPrincipal _user;
    private readonly string _requiredRole;

    public RoleCheckBusinessRule(ClaimsPrincipal user, string requiredRole)
    {
        _user = user;
        _requiredRole = requiredRole;
    }

    public override async Task<IResult> CheckAsync()
    {
        var userRole = _user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (userRole != _requiredRole)
        {
            return new ErrorResult("Yetkiniz yok. Bu işlemi sadece Admin rolündeki kullanıcılar yapabilir.");
        }

        return new SuccessResult();
    }
}