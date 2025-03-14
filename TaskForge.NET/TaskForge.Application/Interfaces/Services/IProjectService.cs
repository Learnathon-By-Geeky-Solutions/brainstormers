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
    public interface IProjectService
    {
        Task<Project?> GetProjectByIdAsync(int projectId);
        Task<IEnumerable<ProjectWithRoleDto>> GetFilteredProjectsAsync(ProjectFilterDto filter);
        Task<IEnumerable<SelectListItem>> GetProjectStatusOptions();
        Task CreateProjectAsync(CreateProjectDto dto);
    }
}
