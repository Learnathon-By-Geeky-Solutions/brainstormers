using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Services
{
    public interface IProjectService
    {
        Task<Project?> GetProjectByIdAsync(int projectId);
		Task<PaginatedList<ProjectWithRoleDto>> GetFilteredProjectsAsync(ProjectFilterDto filter, int pageIndex, int pageSize);
		Task<IEnumerable<SelectListItem>> GetProjectStatusOptions();
        Task CreateProjectAsync(CreateProjectDto dto);
        Task UpdateProjectAsync(Project dto);
    }
}
