using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Repositories
{
    public interface IProjectRepository
    {
        Task<Project?> GetProjectByIdAsync(int id);
        Task<int> AddAsync(CreateProjectDto dto);
    }
}
