using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskForge.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TaskService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TaskItem>> GetTaskListAsync(int projectId)
        {
            return await _unitOfWork.Tasks.FindByExpressionAsync(
                t => t.ProjectId == projectId,  // Filtering by ProjectId
                orderBy: q => q.OrderBy(t => t.DueDate) // Example sorting
			);
        }

        public async Task CreateTaskAsync(TaskDto taskDto)
        {
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
	}
}
