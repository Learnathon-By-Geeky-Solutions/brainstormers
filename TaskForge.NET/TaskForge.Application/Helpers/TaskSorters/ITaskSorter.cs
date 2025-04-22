using TaskForge.Domain.Enums;

namespace TaskForge.Application.Helpers.TaskSorters
{
    public interface ITaskSorter
    {
        Task<List<List<List<int>>>> GetTopologicalOrderingsAsync(TaskWorkflowStatus status, int projectId);
    }
}
