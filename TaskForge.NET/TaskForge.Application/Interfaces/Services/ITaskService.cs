using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> Get(int projectId);
        Task CreateTaskAsync(TaskDto taskDto);
    }
}
