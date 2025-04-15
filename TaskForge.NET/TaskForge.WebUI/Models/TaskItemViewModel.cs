using System.ComponentModel.DataAnnotations;
using TaskForge.Domain.Enums;

namespace TaskForge.WebUI.Models
{
    public class TaskItemCreateViewModel
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskWorkflowStatus Status { get; set; }
        public TaskPriority Priority { get; set; }

        public List<IFormFile> Attachments { get; set; } = new();
    }

    public class TaskItemUpdateViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class TaskItemViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskWorkflowStatus Status { get; set; }
        public TaskPriority Priority { get; set; }

        public List<TaskAssignmentViewModel> AssignedUsers { get; set; } = new List<TaskAssignmentViewModel>();

    }

    public class TaskAssignmentViewModel
    {
        public int UserProfileId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }

    }


}