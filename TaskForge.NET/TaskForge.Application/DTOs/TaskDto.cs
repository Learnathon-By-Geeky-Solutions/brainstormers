using Microsoft.AspNetCore.Http;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskWorkflowStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public List<IFormFile> Attachments { get; set; } = new();
    }
}
