using TaskForge.Application.Common.Utilities;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Helpers.TaskSorters
{
	public class TopologicalTaskSorter : ITaskSorter
	{
		private readonly ITaskDependencyRepository _taskDependencyRepository;
		private readonly ITaskRepository _taskRepository;

		public TopologicalTaskSorter(ITaskDependencyRepository taskDependencyRepository, ITaskRepository taskRepository)
		{
			_taskDependencyRepository = taskDependencyRepository;
			_taskRepository = taskRepository;
		}

		public async Task<List<List<List<int>>>> GetTopologicalOrderingsAsync(TaskWorkflowStatus status, int projectId)
		{
			var allTasks = await GetAllTaskIdsAsync(status, projectId);
			var dependencies = await GetFilteredDependenciesAsync(status, projectId);

			var (adj, inDegree) = BuildGraph(allTasks, dependencies);
			var components = GroupConnectedComponents(allTasks, dependencies);

			var result = new List<List<List<int>>>();
			foreach (var component in components.Values)
			{
				var subgraph = BuildSubgraph(component, adj, inDegree);
				result.Add(TopologicalSortLevels(subgraph.adj, subgraph.inDegree));
			}

			return result;
		}

		private async Task<HashSet<int>> GetAllTaskIdsAsync(TaskWorkflowStatus status, int projectId)
		{
			var taskIds = await _taskRepository.FindByExpressionAsync(
				t => t.Status == status && t.ProjectId == projectId);

			return taskIds.Select(t => t.Id).ToHashSet();
		}

		private async Task<List<TaskDependency>> GetFilteredDependenciesAsync(TaskWorkflowStatus status, int projectId)
		{
			return (await _taskDependencyRepository.FindByExpressionAsync(
			predicate: t => t.Task.ProjectId == projectId && t.Task.Status == status && t.DependsOnTask.Status == status)).ToList();
		}

		private static (Dictionary<int, List<int>> adj, Dictionary<int, int> inDegree)
			BuildGraph(HashSet<int> allTasks, List<TaskDependency> dependencies)
		{
			var adj = new Dictionary<int, List<int>>();
			var inDegree = new Dictionary<int, int>();

			foreach (var dep in dependencies)
			{
				if (!adj.ContainsKey(dep.DependsOnTaskId))
					adj[dep.DependsOnTaskId] = [];

				adj[dep.DependsOnTaskId].Add(dep.TaskId);
				inDegree[dep.TaskId] = inDegree.GetValueOrDefault(dep.TaskId) + 1;
			}

			foreach (var taskId in allTasks)
				if (!inDegree.ContainsKey(taskId)) inDegree[taskId] = 0;

			return (adj, inDegree);
		}

		private static Dictionary<int, List<int>> GroupConnectedComponents(
			HashSet<int> allTasks, List<TaskDependency> dependencies)
		{
			var dsu = new DisjointSetUnion();
			foreach (var task in allTasks)
				dsu.MakeSet(task);

			foreach (var dep in dependencies)
				dsu.Union(dep.TaskId, dep.DependsOnTaskId);

			var components = new Dictionary<int, List<int>>();
			foreach (var task in allTasks)
			{
				int root = dsu.Find(task);
				if (!components.ContainsKey(root))
					components[root] = [];
				components[root].Add(task);
			}
			return components;
		}

		private static (Dictionary<int, List<int>> adj, Dictionary<int, int> inDegree)
			BuildSubgraph(List<int> component, Dictionary<int, List<int>> fullAdj, Dictionary<int, int> fullInDegree)
		{
			var subAdj = new Dictionary<int, List<int>>();
			var subInDegree = new Dictionary<int, int>();

			foreach (var node in component)
			{
				subAdj[node] = fullAdj.ContainsKey(node)
					? fullAdj[node].Where(component.Contains).ToList()
					: [];

				subInDegree[node] = fullInDegree.GetValueOrDefault(node);
			}

			return (subAdj, subInDegree);
		}

		private static List<List<int>> TopologicalSortLevels(Dictionary<int, List<int>> adj, Dictionary<int, int> inDegree)
		{
			var result = new List<List<int>>();
			var queue = new Queue<int>(inDegree.Where(x => x.Value == 0).Select(x => x.Key));

			while (queue.Count > 0)
			{
				var currentLevel = new List<int>();
				var count = queue.Count;

				for (var i = 0; i < count; i++)
				{
					var node = queue.Dequeue();
					currentLevel.Add(node);

					if (!adj.TryGetValue(node, out var neighbors)) continue;
					foreach (var neighbor in neighbors)
					{
						inDegree[neighbor]--;
						if (inDegree[neighbor] == 0)
							queue.Enqueue(neighbor);
					}
				}
				result.Add(currentLevel);
			}

			// Optional cycle check
			if (result.SelectMany(l => l).Count() != inDegree.Count)
				throw new InvalidOperationException("Cycle detected in task dependencies.");

			return result;
		}
	}
}
