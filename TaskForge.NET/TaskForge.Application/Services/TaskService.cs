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

namespace TaskForge.Application.Services
{
    public class TaskService : ITaskService
    {
        private const string UploadsFolder = "uploads";
        private const string TaskFolder = "tasks";
        private const string RootFolder = "wwwroot";
        private const int MaxAttachments = 10;

		private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
		private readonly ITaskSorter _taskSorter;
		private readonly IDependentTaskStrategy _dependentTaskStrategy;
		private readonly ILogger<IdentitySeeder> _logger;
		public TaskService(IUnitOfWork unitOfWork, IFileService fileService, IDependentTaskStrategy dependentTaskStrategy, ITaskSorter taskSorter,
			ILogger<IdentitySeeder> logger)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
			_taskSorter = taskSorter;
			_dependentTaskStrategy = dependentTaskStrategy;
			_logger = logger;
		}



        public async Task<IEnumerable<TaskItem>> GetTaskListAsync(int projectId)
        {
            return await _unitOfWork.Tasks.FindByExpressionAsync(
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

            var result = await _unitOfWork.Tasks.FindByExpressionAsync(predicate, includes: includes);

            return result.FirstOrDefault();
        }
        public async Task<PaginatedList<TaskDto>> GetUserTaskAsync(int? userProfileId, int pageIndex, int pageSize)
        {
	        if (userProfileId == null) return new PaginatedList<TaskDto>(new List<TaskDto>(), 0, pageIndex, pageSize);

	        var userProjectList = await _unitOfWork.ProjectMembers.FindByExpressionAsync(pm => pm.UserProfileId == userProfileId);
	        var userProjectIds = userProjectList.Select(pm => pm.ProjectId).ToList();


	        Expression<Func<TaskItem, bool>> _predicate = t => userProjectIds.Contains(t.ProjectId);
	        Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>> _orderBy = query => query.OrderByDescending(t => t.UpdatedDate);

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
                throw new InvalidOperationException("You can only attach up to 10 files.");

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

            await _unitOfWork.Tasks.AddAsync(taskItem);
            await _unitOfWork.SaveChangesAsync();
        }


		public async Task UpdateTaskAsync(TaskUpdateDto dto)
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

			await _unitOfWork.Tasks.UpdateAsync(task);
			await _unitOfWork.SaveChangesAsync();
		}

		private async Task<TaskItem?> GetTaskWithRelations(int taskId)
		{
			var taskList = await _unitOfWork.Tasks.FindByExpressionAsync(
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
				var users = await _unitOfWork.UserProfiles.FindByExpressionAsync(u => userIds.Contains(u.Id));
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

			var uploadsFolder = Path.Combine(RootFolder, UploadsFolder, TaskFolder);
			Directory.CreateDirectory(uploadsFolder);

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
				_logger.LogError(ex, $"Failed to save file {file.FileName}");
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



		//public async Task UpdateTaskAsync(TaskUpdateDto dto)
		//{
		//    // Step 1: Retrieve the task with its related data (e.g., AssignedUsers, Attachments, etc.)
		//    var taskList = await _unitOfWork.Tasks.FindByExpressionAsync(
		//        t => t.Id == dto.Id && !t.IsDeleted,
		//        includes: query => query
		//            .Include(t => t.Attachments.Where(a => !a.IsDeleted))
		//            .Include(t => t.AssignedUsers)
		//            .ThenInclude(au => au.UserProfile)
		//            .Include(t => t.Dependencies)
		//    );

		//    var task = taskList.FirstOrDefault();

		//    if (task == null)
		//        throw new KeyNotFoundException("Task not found.");

		//    if (task.Attachments.Count + dto.Attachments?.Count > 10)
		//        throw new InvalidOperationException("You can only attach up to 10 files.");


		//    // Step 2: Update task fields from the provided DTO
		//    task.Title = dto.Title;
		//    task.Description = dto.Description;
		//    task.Status = (TaskWorkflowStatus)dto.Status;
		//    task.Priority = (TaskPriority)dto.Priority;
		//    task.StartDate = dto.StartDate;
		//    task.SetDueDate(dto.DueDate);


		//    // Step 3: Update assigned members if any
		//    task.AssignedUsers.Clear();
		//    if (dto.AssignedUserIds != null && dto.AssignedUserIds.Count>0)
		//    {
		//        var users = await _unitOfWork.UserProfiles.FindByExpressionAsync(u => dto.AssignedUserIds.Contains(u.Id));
		//        foreach (var user in users)
		//        {
		//            task.AssignedUsers.Add(new TaskAssignment { UserProfile = user });
		//        }
		//    }

		//    // Step 4: Update dependent tasks if any
		//    task.Dependencies.Clear();
		//    if (dto.DependsOnTaskIds != null && dto.DependsOnTaskIds.Count>0)
		//    {
		//        foreach (var dependsOnTaskId in dto.DependsOnTaskIds)
		//        {
		//            task.Dependencies.Add(new TaskDependency { DependsOnTaskId = dependsOnTaskId });
		//        }
		//    }

		//    // Step 5: Handle attachments (if any)
		//    if (dto.Attachments != null && dto.Attachments.Count>0)
		//    {
		//        foreach (var file in dto.Attachments)
		//        {
		//            if (file.Length > 0)
		//            {
		//                var uploadsFolder = Path.Combine(RootFolder, UploadsFolder, TaskFolder);
		//                Directory.CreateDirectory(uploadsFolder);

		//                var fileName = file.FileName;
		//                var StoredFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
		//                var filePath = Path.Combine(uploadsFolder, StoredFileName);

		//                using (var stream = new FileStream(filePath, FileMode.Create))
		//                {
		//                    await file.CopyToAsync(stream);
		//                }

		//                task.Attachments.Add(new TaskAttachment
		//                {
		//                    FileName = fileName,
		//                    StoredFileName = StoredFileName,
		//                    FilePath = Path.Combine(UploadsFolder, TaskFolder, StoredFileName).Replace("\\", "/"),
		//                    ContentType = file.ContentType
		//                });
		//            }
		//        }
		//    }

		//    // Step 6: Update the task in the repository
		//    await _unitOfWork.Tasks.UpdateAsync(task);

		//    // Step 7: Commit changes
		//    await _unitOfWork.SaveChangesAsync();
		//}

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
                throw new KeyNotFoundException("Task not found");

            // Delete media files associated with attachments
            foreach (var attachment in taskItem.Attachments)
            {
                await _fileService.DeleteFileAsync(attachment.FilePath);
            }

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

        public async Task DeleteAttachmentAsync(int attachmentId)
        {
            var attachment = await _unitOfWork.TaskAttachments.GetByIdAsync(attachmentId);
            if (attachment == null)
            {
                throw new KeyNotFoundException("Attachment not found");
            }

            await _fileService.DeleteFileAsync(attachment.FilePath);
            await _unitOfWork.TaskAttachments.DeleteByIdAsync(attachmentId);
            await _unitOfWork.SaveChangesAsync();
        }

    }
}
