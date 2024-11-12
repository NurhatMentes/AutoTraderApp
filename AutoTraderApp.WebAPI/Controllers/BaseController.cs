using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Utilities.Results;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IResult = AutoTraderApp.Core.Utilities.Results.IResult;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected IActionResult ActionResultInstance<T>(IDataResult<T> result)
        {
            return new ObjectResult(result)
            {
                StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest
            };
        }

        protected IActionResult ActionResultInstance(IResult result)
        {
            if (result == null)
                return new ObjectResult(new ErrorResult("İşlem başarısız"))
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };

            return new ObjectResult(result)
            {
                StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest
            };
        }

        protected Guid GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException(Messages.Auth.UnauthorizedAccess);

            return Guid.Parse(userIdClaim.Value);
        }

        protected string GetUserEmail()
        {
            var emailClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            if (emailClaim == null)
                throw new UnauthorizedAccessException(Messages.Auth.UnauthorizedAccess);

            return emailClaim.Value;
        }
    }
}
