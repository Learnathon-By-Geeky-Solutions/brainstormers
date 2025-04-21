using TaskForge.Application.Common.Utilities;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Helpers.TaskSorters
{
	public class TopologicalTaskSorter : ITaskSorter
	{
		private readonly ITaskDependencyRepository _taskDependencyRepository;

		public TopologicalTaskSorter(ITaskDependencyRepository taskDependencyRepository)
		{
			_taskDependencyRepository = taskDependencyRepository;
		}

		public async Task<List<List<List<int>>>> GetTopologicalOrderingsAsync(TaskWorkflowStatus status)
		{
			var dependencies = await _taskDependencyRepository.FindByExpressionAsync(
				predicate: t => t.Task.Status == status && t.DependsOnTask.Status == status
			);

			// Build full adjacency and in-degree maps
			Dictionary<int, List<int>> adj = new();
			Dictionary<int, int> inDegree = new();
			HashSet<int> allTasks = new();

			if (!dependencies.Any())
				return [];

			foreach (var dep in dependencies)
			{
				int from = dep.DependsOnTaskId;
				int to = dep.TaskId;

				if (!adj.ContainsKey(from)) adj[from] = new List<int>();
				adj[from].Add(to);

				if (!inDegree.ContainsKey(to)) inDegree[to] = 0;
				inDegree[to]++;

				allTasks.Add(from);
				allTasks.Add(to);
			}

			// Ensure all tasks appear in in-degree map
			foreach (var taskId in allTasks)
				if (!inDegree.ContainsKey(taskId)) inDegree[taskId] = 0;

			var result = new List<List<List<int>>>(); // List of DAGs

			var dsu = new DisjointSetUnion();
			foreach (var task in allTasks)
				dsu.MakeSet(task);

			// Treat as undirected for component grouping
			foreach (var dep in dependencies)
			{
				dsu.Union(dep.TaskId, dep.DependsOnTaskId);
			}

			// Group components
			var components = new Dictionary<int, List<int>>();
			foreach (var task in allTasks)
			{
				int root = dsu.Find(task);
				if (!components.ContainsKey(root))
					components[root] = new List<int>();
				components[root].Add(task);
			}

			// For each component, build subgraph and sort
			foreach (var component in components.Values)
			{
				var componentAdj = new Dictionary<int, List<int>>();
				var componentInDegree = new Dictionary<int, int>();

				foreach (var t in component)
				{
					componentAdj[t] = adj.ContainsKey(t)
						? adj[t].Where(x => component.Contains(x)).ToList()
						: new List<int>();

					componentInDegree[t] = inDegree.ContainsKey(t) ? inDegree[t] : 0;
				}

				var dagLevels = TopologicalSortLevels(componentAdj, componentInDegree);
				result.Add(dagLevels);
			}

			return result;
		}

		private List<List<int>> TopologicalSortLevels(Dictionary<int, List<int>> adj, Dictionary<int, int> inDegree)
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
