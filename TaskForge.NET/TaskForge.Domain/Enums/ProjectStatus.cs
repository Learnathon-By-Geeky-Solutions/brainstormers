using System.ComponentModel.DataAnnotations;

namespace TaskForge.Domain.Enums
{
    public enum ProjectStatus
    {
        [Display(Name = "Not Started")]
        NotStarted = 0,  // Project is created but no work has started
        [Display(Name = "In Progress")]
        InProgress = 1,  // Active development is happening
        [Display(Name = "On Hold")]
        OnHold = 2,      // Temporarily paused, but may resume
        Completed = 3,   // Successfully finished
        Cancelled = 4    // Discontinued, no further action
    }
}
