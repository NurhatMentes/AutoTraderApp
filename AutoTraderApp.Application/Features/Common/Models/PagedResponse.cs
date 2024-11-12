using AutoTraderApp.Core.Utilities.Results;

namespace AutoTraderApp.Application.Features.Common.Models;

public class PagedResponse<T> : IDataResult<IReadOnlyList<T>>
{
    public IReadOnlyList<T> Data { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }

    public PagedResponse(IReadOnlyList<T> data, int totalCount, int pageNumber, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Success = true;
    }
}