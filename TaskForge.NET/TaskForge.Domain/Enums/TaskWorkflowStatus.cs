using System.ComponentModel.DataAnnotations;

namespace TaskForge.Domain.Enums;

public enum TaskWorkflowStatus
{
    [Display(Name = "To Do")]
    ToDo = 0,

    [Display(Name = "In Progress")]
    InProgress = 1,
    Done = 2,
    Blocked = 3
}