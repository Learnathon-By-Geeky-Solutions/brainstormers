using TaskForge.Domain.Enums;

namespace TaskForge.Application.DTOs
{
    public class ProjectWithRoleDto
    {
        public int ProjectId { get; set; }
        public string? ProjectTitle { get; set; }
        public ProjectStatus? ProjectStatus { get; set; }
        public ProjectRole? UserRoleInThisProject { get; set; }
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
    }
}
