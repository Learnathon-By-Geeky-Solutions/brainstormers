namespace TaskForge.Application.Common.Model
{
    public class PaginationViewModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}