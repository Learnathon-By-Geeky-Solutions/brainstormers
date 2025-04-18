using TaskForge.Domain.Entities.Common;

namespace TaskForge.Domain.Entities
{
    public class TaskAssignment : BaseEntity
    {
        public int TaskItemId { get; set; }
        public virtual TaskItem TaskItem { get; set; } = null!;

        public int UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; } = null!;
    }
}
