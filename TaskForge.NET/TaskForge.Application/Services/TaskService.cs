using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories;
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
            return await _unitOfWork.Tasks.FindAsync(
                t => t.ProjectId == projectId,  // Filtering by ProjectId
                orderBy: q => q.OrderBy(t => t.DueDate) // Example sorting
            );
        }

        public async Task CreateTaskAsync(TaskDto taskDto)
        {
            var taskItem = new TaskItem
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                ProjectId = taskDto.ProjectId,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = taskDto.CreatedBy
            };
            if (taskDto.DueDate != null) taskItem.SetDueDate(taskDto.DueDate);

            await _unitOfWork.Tasks.AddAsync(taskItem);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
