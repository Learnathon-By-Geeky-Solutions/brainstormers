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
    public class TaskService: ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }


        public async Task<IEnumerable<TaskItem>> Get(int projectId)
        {
            var filter = new TaskFilterDto();
            filter.ProjectId = projectId;
            return await _taskRepository.GetFilteredAsync(filter);
        }

    }
}
