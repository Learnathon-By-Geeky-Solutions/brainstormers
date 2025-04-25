using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Helpers.DependencyResolvers;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Application.Helpers.TaskSorters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TaskForge.Application.Interfaces.Repositories;

namespace TaskForge.Application.Services
{
	public class TaskService : ITaskService
	{
		private const string UploadsFolder = "uploads";
		private const string TaskFolder = "tasks";
		private const string RootFolder = "wwwroot";
		private const int MaxAttachments = 10;

		private readonly IUnitOfWork _unitOfWork;
		private readonly ITaskRepository _taskRepository;
		private readonly IProjectMemberRepository _projectMemberRepository;
		private readonly IUserProfileRepository _userProfileRepository;
		private readonly ITaskAssignmentRepository _taskAssignmentRepository;
		private readonly ITaskAttachmentRepository _taskAttachmentRepository;

		private readonly IFileService _fileService;
		private readonly ITaskSorter _taskSorter;
		private readonly IDependentTaskStrategy _dependentTaskStrategy;
		private readonly ILogger<TaskService> _logger;
		public TaskService(IUnitOfWork unitOfWork, 
			IFileService fileService, 
			IDependentTaskStrategy dependentTaskStrategy, 
			ITaskSorter taskSorter,
			ITaskRepository taskRepository,
			IProjectMemberRepository projectMemberRepository,
			IUserProfileRepository userProfileRepository,
			ITaskAssignmentRepository taskAssignmentRepository,
			ITaskAttachmentRepository taskAttachmentRepository,
			ILogger<TaskService> logger)
		{
			_unitOfWork = unitOfWork;
			_taskRepository = taskRepository;
			_projectMemberRepository = projectMemberRepository;
			_userProfileRepository = userProfileRepository;
			_taskAssignmentRepository = taskAssignmentRepository;
			_taskAttachmentRepository = taskAttachmentRepository;
			_fileService = fileService;
			_taskSorter = taskSorter;
			_dependentTaskStrategy = dependentTaskStrategy;
			_logger = logger;
		}



		public async Task<IEnumerable<TaskItem>> GetTaskListAsync(int projectId)
		{
			return await _taskRepository.FindByExpressionAsync(
				predicate: t => t.ProjectId == projectId,
				includes: t => t.Include(t => t.AssignedUsers).ThenInclude(au => au.UserProfile),
				orderBy: q => q.OrderBy(t => t.DueDate)
			);
		}
		public async Task<TaskItem?> GetTaskByIdAsync(int id)
		{
			Expression<Func<TaskItem, bool>> predicate = t => t.Id == id;

			Func<IQueryable<TaskItem>, IQueryable<TaskItem>> includes = query =>
				query.Include(t => t.Attachments.Where(a => !a.IsDeleted))
					 .Include(t => t.AssignedUsers).ThenInclude(au => au.UserProfile)
					 .Include(t => t.Dependencies)
					 .Include(t => t.Project);

			var result = await _taskRepository.FindByExpressionAsync(predicate, includes: includes);

			return result.FirstOrDefault();
		}
		public async Task<PaginatedList<TaskDto>> GetUserTaskAsync(int? userProfileId, int pageIndex, int pageSize)
		{
			if (pageIndex < 1)
				throw new ArgumentOutOfRangeException(nameof(pageIndex), "Page index must be greater than zero."); 
			if (pageSize < 1)
				throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
			if (userProfileId == null) return new PaginatedList<TaskDto>(new List<TaskDto>(), 0, pageIndex, pageSize);

			var userProjectList = await _projectMemberRepository.FindByExpressionAsync(pm => pm.UserProfileId == userProfileId);
			var userProjectIds = userProjectList.Select(pm => pm.ProjectId).ToList();


			Expression<Func<TaskItem, bool>> _predicate = t => userProjectIds.Contains(t.ProjectId);
			Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>> _orderBy = query => query.OrderByDescending(t => t.UpdatedDate);

			var (taskList, totalCount) = await _taskRepository.GetPaginatedListAsync(
				predicate: _predicate,
				orderBy: _orderBy,
				includes: query => query.Include(t => t.Project),
				skip: (pageIndex - 1) * pageSize,
				take: pageSize
			);

			// Convert tasks to DTOs
			var taskListDto = taskList.Select(t => new TaskDto
			{
				Id = t.Id,
				Title = t.Title,
				ProjectId = t.ProjectId,
				ProjectTitle = t.Project.Title,
				DueDate = t.DueDate,
				Status = t.Status,
				Priority = t.Priority
			}).ToList();

			return new PaginatedList<TaskDto>(taskListDto, totalCount, pageIndex, pageSize);
		}
		public async Task<List<List<List<int>>>> GetSortedTasksAsync(TaskWorkflowStatus status, int projectId)
		{
			if (projectId <= 0)
				throw new ArgumentException("Invalid project ID", nameof(projectId));

			var sortedTasks = await _taskSorter.GetTopologicalOrderingsAsync(status, projectId);
			return sortedTasks ?? new List<List<List<int>>>();
		}
		public async Task<List<int>> GetDependentTaskIdsAsync(int id, TaskWorkflowStatus status)
		{
			await _dependentTaskStrategy.InitializeAsync(status);

			var result = await _dependentTaskStrategy.GetDependentTaskIdsAsync(id);

			return result;
		}



		public async Task CreateTaskAsync(TaskDto taskDto)
		{
			if (taskDto.Attachments != null && taskDto.Attachments.Count > MaxAttachments)
				throw new InvalidOperationException($"You can only attach up to {MaxAttachments} files.");

			var taskItem = new TaskItem
			{
				ProjectId = taskDto.ProjectId,
				Title = taskDto.Title,
				Description = taskDto.Description,
				StartDate = taskDto.StartDate,
				Status = taskDto.Status,
				Priority = taskDto.Priority,
			};
			if (taskDto.DueDate != null) taskItem.SetDueDate(taskDto.DueDate);

			// Save attachments if any
			if (taskDto.Attachments != null && taskDto.Attachments.Count > 0)
			{
				foreach (var file in taskDto.Attachments)
				{
					if (file.Length == 0) continue;
					var attachment = await SaveAttachmentAsync(file);
					taskItem.Attachments.Add(attachment);
				}
			}

			await _taskRepository.AddAsync(taskItem);
			await _unitOfWork.SaveChangesAsync();
		}



		public async Task UpdateTaskAsync(TaskUpdateDto dto)
		{
			await using var transaction = await _unitOfWork.BeginTransactionAsync();

			try
			{
				var task = await GetTaskWithRelations(dto.Id);
				if (task == null) throw new KeyNotFoundException("Task not found.");

				var newAttachmentsCount = dto.Attachments?.Count ?? 0;
				if (task.Attachments.Count + newAttachmentsCount > MaxAttachments)
					throw new InvalidOperationException($"You can only attach up to {MaxAttachments} files.");

				UpdateBasicFields(task, dto);
				await UpdateAssignedUsersAsync(task, dto.AssignedUserIds);
				UpdateDependencies(task, dto.DependsOnTaskIds);
				await HandleAttachmentsAsync(task, dto.Attachments);

				await _taskRepository.UpdateAsync(task);
				await _unitOfWork.SaveChangesAsync();

				await transaction.CommitAsync();
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}
		private async Task<TaskItem?> GetTaskWithRelations(int taskId)
		{
			var taskList = await _taskRepository.FindByExpressionAsync(
				t => t.Id == taskId && !t.IsDeleted,
				includes: query => query
					.Include(t => t.Attachments.Where(a => !a.IsDeleted))
					.Include(t => t.AssignedUsers).ThenInclude(au => au.UserProfile)
					.Include(t => t.Dependencies)
			);

			return taskList.FirstOrDefault();
		}
		private void UpdateBasicFields(TaskItem task, TaskUpdateDto dto)
		{
			task.Title = dto.Title;
			task.Description = dto.Description;
			task.Status = (TaskWorkflowStatus)dto.Status;
			task.Priority = (TaskPriority)dto.Priority;
			task.StartDate = dto.StartDate;
			task.SetDueDate(dto.DueDate);
		}
		private async Task UpdateAssignedUsersAsync(TaskItem task, List<int>? userIds)
		{
			task.AssignedUsers.Clear();

			if (userIds != null && userIds.Count > 0)
			{
				var users = await _userProfileRepository.FindByExpressionAsync(u => userIds.Contains(u.Id));
				foreach (var user in users)
				{
					task.AssignedUsers.Add(new TaskAssignment { UserProfile = user });
				}
			}
		}
		private void UpdateDependencies(TaskItem task, List<int>? dependencyIds)
		{
			task.Dependencies.Clear();

			if (dependencyIds != null && dependencyIds.Count > 0)
			{
				foreach (var dependsOnId in dependencyIds)
				{
					task.Dependencies.Add(new TaskDependency { DependsOnTaskId = dependsOnId });
				}
			}
		}
		private async Task HandleAttachmentsAsync(TaskItem task, List<IFormFile>? attachments)
		{
			if (attachments == null || attachments.Count == 0) return;

			foreach (var file in attachments)
			{
				if (file.Length == 0) continue;
				var attachment = await SaveAttachmentAsync(file);
				task.Attachments.Add(attachment);
			}
		}
		private async Task<TaskAttachment> SaveAttachmentAsync(IFormFile file)
		{
			if (file.Length == 0) throw new ArgumentException("File is empty.");

			var uploadsFolder = Path.Combine(RootFolder, UploadsFolder, TaskFolder);
			Directory.CreateDirectory(uploadsFolder);

			var storedFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
			var filePath = Path.Combine(uploadsFolder, storedFileName);

			try
			{
				await using var stream = new FileStream(filePath, FileMode.Create);
				await file.CopyToAsync(stream);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to save file {FileName}", file.FileName);
				throw new IOException("An error occurred while saving the attachment.", ex);
			}

			return new TaskAttachment
			{
				FileName = file.FileName,
				StoredFileName = storedFileName,
				FilePath = Path.Combine(UploadsFolder, TaskFolder, storedFileName).Replace("\\", "/"),
				ContentType = file.ContentType
			};
		}



		public async Task RemoveTaskAsync(int id)
		{
			await using var transaction = await _unitOfWork.BeginTransactionAsync();

			try
			{
				var task = await _taskRepository.FindByExpressionAsync(
					t => t.Id == id,
					includes: query => query
						.Include(t => t.Attachments)
						.Include(t => t.AssignedUsers)
				);

				var taskItem = task.FirstOrDefault();
				if (taskItem == null)
					throw new KeyNotFoundException("Task not found");

				await _taskRepository.DeleteByIdAsync(id);

				var attachmentIds = taskItem.Attachments.Select(a => a.Id);
				await _taskAttachmentRepository.DeleteByIdsAsync(attachmentIds);

				var assignmentIds = taskItem.AssignedUsers.Select(a => a.Id);
				await _taskAssignmentRepository.DeleteByIdsAsync(assignmentIds);

				await _unitOfWork.SaveChangesAsync();

				await transaction.CommitAsync();

				try
				{
					foreach (var attachment in taskItem.Attachments)
					{
						await _fileService.DeleteFileAsync(attachment.FilePath);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "DB committed but failed to delete attachment file(s) for task {TaskId}", id);
				}
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public async Task DeleteAttachmentAsync(int attachmentId)
		{
			var attachment = await _taskAttachmentRepository.GetByIdAsync(attachmentId);
			if (attachment == null)
			{
				throw new KeyNotFoundException("Attachment not found");
			}

			await _fileService.DeleteFileAsync(attachment.FilePath);
			await _taskAttachmentRepository.DeleteByIdAsync(attachmentId);
			await _unitOfWork.SaveChangesAsync();
		}

	}
}
