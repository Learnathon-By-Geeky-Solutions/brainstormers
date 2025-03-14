using System;
using System.Collections.Generic;
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


        public virtual ICollection<TaskAssignment> AssignedUsers { get; set; } = new List<TaskAssignment>();
        public void SetDueDate(DateTime? dueDate)
        {
            if (dueDate.HasValue && dueDate?.Date < CreatedDate.Date)
            {
                throw new ArgumentException("DueDate cannot be earlier than the CreatedDate.");
            }
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
