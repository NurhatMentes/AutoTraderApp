namespace AutoTraderApp.Core.Utilities.Results;

public class SuccessDataResult<T> : DataResult<T>
{
    public SuccessDataResult(T data, string? message)
        : base(data, true, message, 200)
    {
    }

    public SuccessDataResult(string v, T data)
        : base(data, true, null, 200)
    {
    }
}