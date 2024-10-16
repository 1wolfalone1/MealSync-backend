namespace MealSync.Application.Common.Models.Responses;

public class PaginationResponse<TEntity, TResponse> where TEntity : class where TResponse : class
{
    public PaginationResponse(IList<TResponse> items, int count, int pageIndex, int pageSize)
    {
        TotalCount = count;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items;
    }

    public PaginationResponse(IList<TEntity> items, int count, int pageIndex, int pageSize,
        Func<TEntity, TResponse> mapper)
    {
        TotalCount = count;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items.Select(mapper).ToList();
    }

    public PaginationResponse(IQueryable<TEntity> source, int pageIndex, int pageSize, Func<TEntity, TResponse> mapper)
    {
        TotalCount = source.Count();
        PageSize = pageSize;
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);

        var items = source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        Items = items.Select(mapper).ToList();
    }

    public int PageIndex { get; }

    public int TotalPages { get; }

    public int PageSize { get; set; }

    public int TotalCount { get; }

    public bool HasPrevious => PageIndex > 1;

    public bool HasNext => PageIndex < TotalPages;

    public IList<TResponse> Items { get; }
}

public class PaginationResponse<TEntity> : PaginationResponse<TEntity, TEntity> where TEntity : class
{
    public PaginationResponse(IList<TEntity> items, int count, int pageIndex, int pageSize)
        : base(items, count, pageIndex, pageSize)
    {
    }

    public PaginationResponse(IList<TEntity> items, int count, int pageIndex, int pageSize, Func<TEntity, TEntity> mapper)
        : base(items, count, pageIndex, pageSize, mapper)
    {
    }

    public PaginationResponse(IQueryable<TEntity> source, int pageIndex, int pageSize, Func<TEntity, TEntity> mapper)
        : base(source, pageIndex, pageSize, mapper)
    {
    }

    public PaginationResponse(IQueryable<TEntity> source, int pageIndex, int pageSize) : base(source, pageIndex, pageSize, entity => entity)
    {
    }

}