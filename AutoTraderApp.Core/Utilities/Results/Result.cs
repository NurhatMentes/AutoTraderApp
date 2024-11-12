namespace AutoTraderApp.Core.Utilities.Results;

public class Result : IResult
{
    public bool Success { get; }
    public string Message { get; }
    public int StatusCode { get; }

    public Result(bool success, string message = null, int statusCode = 200)
    {
        Success = success;
        Message = message;
        StatusCode = statusCode;
    }
}