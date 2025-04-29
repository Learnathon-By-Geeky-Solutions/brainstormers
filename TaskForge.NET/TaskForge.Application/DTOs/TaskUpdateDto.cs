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
        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set => _startDate = value.HasValue
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : (DateTime?)null;
        }
        private DateTime? _dueDate;
        public DateTime? DueDate
        {
            get => _dueDate;
            set => _dueDate = value.HasValue
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : (DateTime?)null;
        }
        public List<int>? AssignedUserIds { get; set; }
        public List<int> DependsOnTaskIds { get; set; } = new List<int>();
        public List<int>? DependentTaskIds { get; set; }
        public List<IFormFile>? Attachments { get; set; }
    }
}
