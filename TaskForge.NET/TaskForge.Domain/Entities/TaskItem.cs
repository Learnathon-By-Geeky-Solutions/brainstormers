using System.ComponentModel.DataAnnotations;
using TaskForge.Domain.Entities.Common;
using TaskForge.Domain.Enums;

namespace TaskForge.Domain.Entities
{
    public class TaskItem : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public TaskWorkflowStatus Status { get; set; } = TaskWorkflowStatus.ToDo; // Updated

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; private set; }

        public IList<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
        public virtual IList<TaskAssignment> AssignedUsers { get; set; } = new List<TaskAssignment>();

        // Tasks this one depends on
        public IList<TaskDependency> Dependencies { get; set; } = new List<TaskDependency>();

        // Tasks that depend on this one
        public IList<TaskDependency> DependentOnThis { get; set; } = new List<TaskDependency>(); 

        public void SetDueDate(DateTime? dueDate)
        {
            if (dueDate.HasValue && StartDate.HasValue && dueDate < StartDate)
                throw new ValidationException("Due date cannot be earlier than Start date.");

            DueDate = dueDate;
        }


        public void SetStatus(TaskWorkflowStatus newStatus)
        {
            if (newStatus == TaskWorkflowStatus.InProgress && StartDate == null)
            {
                StartDate = DateTime.UtcNow;
            }

            Status = newStatus;
        }
    }
}
