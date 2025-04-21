using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Interfaces.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> GetTaskListAsync(int projectId);
        Task<TaskItem?> GetTaskByIdAsync(int id);
        Task<List<List<List<int>>>> GetSortedTasksAsync(TaskWorkflowStatus status, int projectId);
		Task<List<int>> GetDependentTaskIdsAsync(int id, TaskWorkflowStatus status);
        Task<PaginatedList<TaskDto>> GetUserTaskAsync(int? userProfileId, int pageIndex, int pageSize);
        Task CreateTaskAsync(TaskDto taskDto);
        Task UpdateTaskAsync(TaskUpdateDto dto);
        Task RemoveTaskAsync(int id);
        Task DeleteAttachmentAsync(int attachmentId);

    }
}
