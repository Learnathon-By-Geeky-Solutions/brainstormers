namespace TaskForge.Application.DTOs
{
    public class TaskDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
        public List<int> AssignedUserIds { get; set; } = new();
        public List<SimpleUserDto> AssignedUsers { get; set; } = new();
        public List<SuggestedUserDto> SuggestUserList { get; set; } = new();
    }

    public class AttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
    }

    public class SimpleUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class SuggestedUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
