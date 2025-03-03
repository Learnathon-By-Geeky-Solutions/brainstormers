using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Repositories
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(int id);
    }
}
