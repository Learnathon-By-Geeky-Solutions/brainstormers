using TaskForge.Domain.Enums;

namespace TaskForge.Application.DTOs
{
    public class ProjectMemberDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserProfileId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public ProjectRole Role { get; set; }
    }

}
