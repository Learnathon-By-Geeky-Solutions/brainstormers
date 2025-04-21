using Microsoft.AspNetCore.Http;

namespace TaskForge.Application.DTOs
{
    public class TaskUpdateDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public List<int>? AssignedUserIds { get; set; }
        public List<int> DependsOnTaskIds { get; set; } = new List<int>();
        public List<int>? DependentTaskIds { get; set; }
        public List<IFormFile>? Attachments { get; set; }
    }
}
