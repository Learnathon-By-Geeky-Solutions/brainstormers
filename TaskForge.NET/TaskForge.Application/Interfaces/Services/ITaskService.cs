using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Services
{
	public interface ITaskService
	{
		Task<IEnumerable<TaskItem>> GetTaskListAsync(int projectId);
		Task<PaginatedList<TaskDto>> GetUserTaskAsync(int? userProfileId, int pageIndex, int pageSize);
		Task CreateTaskAsync(TaskDto taskDto);
		Task UpdateTaskAsync(TaskUpdateDto dto);
		Task RemoveTaskAsync(int id);
		Task DeleteAttachmentAsync(int attachmentId);
		Task<TaskDetailsDto?> GetTaskDetailsAsync(int id);
    }
}
