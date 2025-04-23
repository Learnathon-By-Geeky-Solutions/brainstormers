using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Helpers.DependencyResolvers
{
    public class RecursiveDependentTaskStrategy : IDependentTaskStrategy
    {
        private readonly ITaskDependencyRepository _taskDependencyRepository;
        private readonly Dictionary<int, List<int>> _adjacencyList;

        public RecursiveDependentTaskStrategy(ITaskDependencyRepository taskDependencyRepository)
        {
            _taskDependencyRepository = taskDependencyRepository;
            _adjacencyList = [];
        }

        public async Task InitializeAsync(TaskWorkflowStatus taskWorkflowStatus)
        {
            // Fetch task dependencies based on the status
            var taskDependencyPairList = (await _taskDependencyRepository.FindByExpressionAsync(
                predicate: t => t.Task.Status == taskWorkflowStatus && t.DependsOnTask.Status == taskWorkflowStatus
            )).Select(t => new Tuple<int, int>(t.DependsOnTaskId, t.TaskId)).ToList();

            // Populate the adjacency list
            foreach (var taskDependency in taskDependencyPairList)
            {
                if (!_adjacencyList.TryGetValue(taskDependency.Item1, out List<int>? value))
                {
                    value = [];
                    _adjacencyList[taskDependency.Item1] = value;
                }

                value.Add(taskDependency.Item2);
            }
        }

        public async Task<List<int>> GetDependentTaskIdsAsync(int taskId)
        {
            var dependentTaskIds = new HashSet<int>();
            await TraverseAsync(taskId, dependentTaskIds);
            return dependentTaskIds.ToList();
        }

        private async Task TraverseAsync(int taskId, HashSet<int> visited)
        {
            if (!visited.Add(taskId)) return;

            if (_adjacencyList.ContainsKey(taskId))
            {
                foreach (var dependentTaskId in _adjacencyList[taskId])
                {
                    await TraverseAsync(dependentTaskId, visited);
                }
            }
        }
    }

}
