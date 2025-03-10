using TaskForge.Domain.Enums;

namespace TaskForge.Application.DTOs
{
    public class ProjectWithRoleDto
    {
        public int ProjectId { get; set; }
        public string? Title { get; set; }
        public ProjectStatus? Status { get; set; }
        public ProjectRole? UserRoleInThisProject { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
