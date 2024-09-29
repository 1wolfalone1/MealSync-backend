using MealSync.Application.Common.Constants;

namespace MealSync.Application.Common.Models.Requests;

public abstract class PaginationRequest
{
    private int _pageIndex = PaginationConstants.DefaultPageIndex;

    private int _pageSize = PaginationConstants.DefaultPageSize;

    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value > 0
            ? value
            : PaginationConstants.DefaultPageIndex;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 0 && value <= PaginationConstants.MaxPageSize
            ? value
            : PaginationConstants.DefaultPageSize;
    }
}