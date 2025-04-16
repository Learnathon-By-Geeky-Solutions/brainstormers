using TaskForge.Domain.Entities.Common;

namespace TaskForge.Domain.Entities
{
    public class TaskAttachment : BaseEntity
    {
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty; // Unique GUID-based name for physical file storage
        public string FilePath { get; set; } = string.Empty; // Path where the file is stored
        public string ContentType { get; set; } = string.Empty; // MIME type (e.g., image/png, application/pdf)

        public int TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;

        public string? Description { get; set; }
    }
}
