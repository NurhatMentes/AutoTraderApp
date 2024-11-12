namespace AutoTraderApp.Core.Utilities.Results;

public class PagedResult<T> : DataResult<IEnumerable<T>>
{
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public PagedResult(
        IEnumerable<T> data,
        int count,
        int page,
        int pageSize,
        string message = null)
        : base(data, true, message)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = count;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }
}