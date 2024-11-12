using AutoTraderApp.Core.Utilities.Results;

namespace AutoTraderApp.Application.Features.Rules
{
    public abstract class BusinessRule
    {
        public abstract Task<IResult> CheckAsync();
    }
}
