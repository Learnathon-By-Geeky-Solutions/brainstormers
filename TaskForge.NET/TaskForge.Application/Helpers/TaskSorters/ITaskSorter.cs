using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Helpers.TaskSorters
{
	public interface ITaskSorter
	{
		Task<List<List<List<int>>>> GetTopologicalOrderingsAsync(TaskWorkflowStatus status, int projectId);
	}
}
