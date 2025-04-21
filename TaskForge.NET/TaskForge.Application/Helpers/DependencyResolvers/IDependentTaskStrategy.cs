using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Helpers.DependencyResolvers
{
    public interface IDependentTaskStrategy
    {
        Task InitializeAsync(TaskWorkflowStatus taskWorkflowStatus);

        Task<List<int>> GetDependentTaskIdsAsync(int taskId);
    }

}
