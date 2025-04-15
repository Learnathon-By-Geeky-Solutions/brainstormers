using System.ComponentModel.DataAnnotations;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.WebUI.Models
{
    public class ProjectUpdateViewModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public ProjectStatus Status { get; set; } = ProjectStatus.NotStarted; // Enum for status

        public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public DateTime? EndDateInput { get; set; }
    }
}
