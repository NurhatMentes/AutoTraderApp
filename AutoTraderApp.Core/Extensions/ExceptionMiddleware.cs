using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace AutoTraderApp.Core.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);

                // Eğer status kod 403 ise özel bir mesaj dön
                if (httpContext.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    await HandleForbiddenExceptionAsync(httpContext);
                }
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(httpContext, e);
            }
        }

        private static Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            httpContext.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Internal Server Error";
            var type = "Error";

            if (exception is ValidationException validationException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = string.Join(", ", validationException.Errors.Select(x => x.ErrorMessage));
                type = "ValidationError";
            }

            httpContext.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new ErrorDetails
            {
                StatusCode = httpContext.Response.StatusCode,
                Message = message,
                Type = type
            });

            return httpContext.Response.WriteAsync(result);
        }

        // 403 Forbidden hatası için özel bir mesaj
        private static Task HandleForbiddenExceptionAsync(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

            var result = JsonSerializer.Serialize(new ErrorDetails
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Message = "Bu işlem için yetkiniz yok.",
                Type = "Forbidden"
            });

            return httpContext.Response.WriteAsync(result);
        }
    }

    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
