using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Services
{
	public class TaskService : ITaskService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IFileService _fileService;
        public TaskService(IUnitOfWork unitOfWork, IFileService fileService)
		{
			_unitOfWork = unitOfWork;
			_fileService = fileService;
		}

		public async Task<IEnumerable<TaskItem>> GetTaskListAsync(int projectId)
		{
			return await _unitOfWork.Tasks.FindByExpressionAsync(
				predicate: t => t.ProjectId == projectId,
				includes: t => t.Include(t => t.AssignedUsers).ThenInclude(au => au.UserProfile),
				orderBy: q => q.OrderBy(t => t.DueDate)
			);
		}

		public async Task<TaskItem?> GetTaskByIdAsync(int taskId)
		{
			Expression<Func<TaskItem, bool>> predicate = t => t.Id == taskId;

			Func<IQueryable<TaskItem>, IQueryable<TaskItem>> includes = query =>
				query.Include(t => t.Attachments.Where(a => !a.IsDeleted))
					 .Include(t => t.AssignedUsers).ThenInclude(au => au.UserProfile)
					 .Include(t => t.Project);

			var result = await _unitOfWork.Tasks.FindByExpressionAsync(predicate, includes: includes);

			return result.FirstOrDefault();
		}

		public async Task CreateTaskAsync(TaskDto taskDto)
		{
			if (taskDto.Attachments != null && taskDto.Attachments.Count > 10)
				throw new Exception("You can only attach up to 10 files.");

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
			if (taskDto.Attachments != null && taskDto.Attachments.Any())
			{
				foreach (var file in taskDto.Attachments)
				{
					if (file.Length > 0)
					{
						var uploadsFolder = Path.Combine("wwwroot", "uploads", "tasks");
						Directory.CreateDirectory(uploadsFolder);

						var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
						var filePath = Path.Combine(uploadsFolder, fileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							await file.CopyToAsync(stream);
						}

						taskItem.Attachments.Add(new TaskAttachment
						{
							FileName = file.FileName,
							FilePath = Path.Combine("uploads", "tasks", fileName).Replace("\\", "/"),
							ContentType = file.ContentType
						});
					}
				}
			}

			await _unitOfWork.Tasks.AddAsync(taskItem);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<PaginatedList<TaskDto>> GetUserTaskAsync(int? userProfileId, int pageIndex, int pageSize)
		{
			if (userProfileId == null) return new PaginatedList<TaskDto>(new List<TaskDto>(), 0, pageIndex, pageSize);

			var userProjectList = await _unitOfWork.ProjectMembers.FindByExpressionAsync(pm => pm.UserProfileId == userProfileId);
			var userProjectIds = userProjectList.Select(pm => pm.ProjectId).ToList();


			Expression<Func<TaskItem, bool>> _predicate = t => userProjectIds.Contains(t.ProjectId);
			Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>> _orderBy = query => query.OrderBy(t => t.DueDate);

			var (taskList, totalCount) = await _unitOfWork.Tasks.GetPaginatedListAsync(
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

		public async Task RemoveTaskAsync(int id)
		{
			// Get the task with related data
			var task = await _unitOfWork.Tasks.FindByExpressionAsync(
				t => t.Id == id,
				includes: query => query
					.Include(t => t.Attachments)
					.Include(t => t.AssignedUsers)
			);

			var taskItem = task.FirstOrDefault();
			if (taskItem == null)
				throw new Exception("Task not found");

			// Soft delete the main task
			await _unitOfWork.Tasks.DeleteByIdAsync(id);

			// Soft delete attachments
			var attachmentIds = taskItem.Attachments.Select(a => a.Id);
			await _unitOfWork.TaskAttachments.DeleteByIdsAsync(attachmentIds);

			// Soft delete assignments
			var assignmentIds = taskItem.AssignedUsers.Select(a => a.Id);
			await _unitOfWork.TaskAssignments.DeleteByIdsAsync(assignmentIds);

			await _unitOfWork.SaveChangesAsync();
		}

		public async Task UpdateTaskAsync(TaskUpdateDto dto)
		{
			// Step 1: Retrieve the task with its related data (e.g., AssignedUsers, Attachments, etc.)
			var taskList = await _unitOfWork.Tasks.FindByExpressionAsync(
				t => t.Id == dto.Id && !t.IsDeleted,
				includes: query => query
					.Include(t => t.Attachments.Where(a => !a.IsDeleted))
					.Include(t => t.AssignedUsers)
					.ThenInclude(au => au.UserProfile)
			);

			var task = taskList.FirstOrDefault();

			if (task == null)
				throw new Exception("Task not found.");

			if (task.Attachments.Count + dto.Attachments?.Count > 10)
				throw new Exception("You can only attach up to 10 files.");


			// Step 2: Update task fields from the provided DTO
			task.Title = dto.Title;
			task.Description = dto.Description;
			task.Status = (TaskWorkflowStatus)dto.Status;
			task.Priority = (TaskPriority)dto.Priority;
			task.StartDate = dto.StartDate;
			task.SetDueDate(dto.DueDate);


			// Step 3: Update assigned members if any
			task.AssignedUsers.Clear();
			if (dto.AssignedUserIds != null && dto.AssignedUserIds.Any())
			{
				var users = await _unitOfWork.UserProfiles.FindByExpressionAsync(u => dto.AssignedUserIds.Contains(u.Id));
				foreach (var user in users)
				{
					task.AssignedUsers.Add(new TaskAssignment { UserProfile = user });
				}
			}


			// Step 4: Handle attachments (if any)
			if (dto.Attachments != null && dto.Attachments.Any())
			{
				foreach (var file in dto.Attachments)
				{
					if (file.Length > 0)
					{
						var uploadsFolder = Path.Combine("wwwroot", "uploads", "tasks");
						Directory.CreateDirectory(uploadsFolder);

						var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
						var filePath = Path.Combine(uploadsFolder, fileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							await file.CopyToAsync(stream);
						}

						task.Attachments.Add(new TaskAttachment
						{
							FileName = file.FileName,
							FilePath = Path.Combine("uploads", "tasks", fileName).Replace("\\", "/"),
							ContentType = file.ContentType
						});
					}
				}
			}

			// Step 5: Update the task in the repository
			await _unitOfWork.Tasks.UpdateAsync(task);

			// Step 6: Commit changes
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task DeleteAttachmentAsync(int attachmentId)
		{
			var attachment = await _unitOfWork.TaskAttachments.GetByIdAsync(attachmentId);
			if (attachment == null)
				throw new KeyNotFoundException("Attachment not found");

            await _fileService.DeleteFileAsync(attachment.FilePath);

            await _unitOfWork.TaskAttachments.DeleteByIdAsync(attachmentId);
			await _unitOfWork.SaveChangesAsync();
		}

	}
}
