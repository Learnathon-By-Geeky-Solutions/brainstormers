using TaskForge.Domain.Entities.Common;

namespace TaskForge.Domain.Entities
{
    public class TaskDependency : BaseEntity
    {
        public int TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;

        public int DependsOnTaskId { get; set; }
        public TaskItem DependsOnTask { get; set; } = null!;
    }
}
