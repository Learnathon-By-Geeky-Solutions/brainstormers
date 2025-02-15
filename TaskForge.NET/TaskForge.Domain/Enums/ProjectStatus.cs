namespace TaskForge.Domain.Enums
{
    public enum ProjectStatus
    {
        NotStarted = 0,  // Project is created but no work has started
        InProgress = 1,  // Active development is happening
        OnHold = 2,      // Temporarily paused, but may resume
        Completed = 3,   // Successfully finished
        Cancelled = 4    // Discontinued, no further action
    }
}
