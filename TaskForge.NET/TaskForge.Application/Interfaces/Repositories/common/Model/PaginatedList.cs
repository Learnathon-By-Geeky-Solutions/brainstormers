namespace TaskForge.Application.Common.Model
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int TotalCount { get; }
        public int PageIndex { get; }
        public int PageSize { get; }
        public int TotalPages { get; }

        public bool HasNextPage => PageIndex < TotalPages;
        public bool HasPreviousPage => PageIndex > 1;

        public PaginatedList(IEnumerable<T> items, int totalCount, int pageIndex, int pageSize)
        {
            Items = items.ToList();
            TotalCount = totalCount;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }
    }
}