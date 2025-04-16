using TaskForge.Domain.Enums;

namespace TaskForge.Application.DTOs
{
    public class ProjectFilterDto
    {
        public string? UserId { get; set; }
        public string? Title { get; set; }
        public ProjectStatus? Status { get; set; }
        public ProjectRole? Role { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }

        public string? SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";
    }
}
