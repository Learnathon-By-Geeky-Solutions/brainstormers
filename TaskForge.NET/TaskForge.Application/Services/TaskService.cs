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

        public async Task<IEnumerable<TaskItem>> Get(int projectId)
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

            await _unitOfWork.Tasks.AddAsync(taskItem);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<TaskDto>> GetUserTaskAsync(int? userProfileId)
        {
            if (userProfileId == null)
            {
                return new List<TaskDto>();
            }

            // Retrieve project IDs where the user is a member
            var userProjects = await _unitOfWork.ProjectMembers
                .FindByExpressionAsync(pm => pm.UserProfileId == userProfileId,
                                       orderBy: null,
                                       includes: query => query.Include(pm => pm.Project),
                                       take: null,
                                       skip: null);

            var userProjectIds = userProjects.Select(pm => pm.ProjectId).ToList();


            if (!userProjectIds.Any()) return new List<TaskDto>();

            // Define filtering expression for tasks belonging to those projects
            Expression<Func<TaskItem, bool>> _predicate = t => userProjectIds.Contains(t.ProjectId);

            // Define sorting logic
            Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>> _orderBy = query =>
                query.OrderBy(t => t.DueDate); // Sort tasks by DueDate

            // Fetch filtered tasks
            var tasks = await _unitOfWork.Tasks.FindByExpressionAsync(
                predicate: _predicate,
                orderBy: _orderBy,
                includes: null,
                take: null,
                skip: null
            );

            // Convert tasks to DTOs
            var taskDtos = tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                ProjectId = t.ProjectId,
                DueDate = t.DueDate,
                Status = t.Status,
                Priority = t.Priority
            }).ToList();

            return taskDtos;
        }
    }
}
