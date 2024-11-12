namespace AutoTraderApp.Core.Utilities.Results;

public class ErrorDataResult<T> : DataResult<T>
{
    public ErrorDataResult(string message, int statusCode = 400)
        : base(default, false, message, statusCode)
    {
    }

    public ErrorDataResult(T data, string message, int statusCode = 400)
        : base(data, false, message, statusCode)
    {
    }
}