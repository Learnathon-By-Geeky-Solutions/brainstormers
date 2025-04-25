namespace TaskForge.Application.Common.Model;

public class PaginatedList<T>
{
    public PaginatedList(IEnumerable<T> items, int totalCount, int pageIndex, int pageSize)
    {
        Items = items.ToList();
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public List<T> Items { get; }
    public int PageIndex { get; }
    public int PageSize { get; }
    public int TotalPages { get; }

    public int TotalCount => Items.Count();
    public bool HasNextPage => PageIndex < TotalPages;
    public bool HasPreviousPage => PageIndex > 1;
}