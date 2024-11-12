using AutoTraderApp.Core.Utilities.Results;
using MediatR;

namespace AutoTraderApp.Application.Features.Common
{
    namespace AutoTraderApp.Application.Features.Common
    {
        public abstract class BaseRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
            where TResponse : IResult
        {
            public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
        }
    }

}
