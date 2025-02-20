using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
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
    public class ProjectService: IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        public ProjectService(IProjectRepository projectRepository, IUserProfileRepository userProfileRepository, IProjectMemberRepository projectMemberRepository)
        {
            _projectRepository = projectRepository;
            _userProfileRepository = userProfileRepository;
            _projectMemberRepository = projectMemberRepository;
        }

        public async Task<IEnumerable<Project>> GetFilteredProjectsAsync(ProjectFilterDto filter)
        {
            var userProfileId = await _userProfileRepository.GetUserProfileIdByUserIdAsync(filter.UserId);
            if (userProfileId == null) return Enumerable.Empty<Project>();

            var projectIds = await _projectMemberRepository.GetProjectIdsByUserProfileIdAsync(userProfileId);
            if (projectIds == null || !projectIds.Any()) return Enumerable.Empty<Project>();

            var projects = new List<Project>();

            foreach (var projectId in projectIds)
            {
                var project = await _projectRepository.GetProjectByIdAsync(projectId);
                if (project != null)
                {
                    projects.Add(project);
                }
            }

            // Apply filters
            var filteredProjects = projects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Title))
                filteredProjects = filteredProjects.Where(p => p.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));

            if (filter.Status.HasValue)
                filteredProjects = filteredProjects.Where(p => p.Status == filter.Status.Value);

            if (filter.StartDateFrom.HasValue)
                filteredProjects = filteredProjects.Where(p => p.StartDate.Date >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                filteredProjects = filteredProjects.Where(p => p.StartDate.Date <= filter.StartDateTo.Value);

            if (filter.EndDateFrom.HasValue)
                filteredProjects = filteredProjects.Where(p => p.EndDate.HasValue && p.EndDate.Value.Date >= filter.EndDateFrom.Value);

            if (filter.EndDateTo.HasValue)
                filteredProjects = filteredProjects.Where(p => p.EndDate.HasValue && p.EndDate.Value.Date <= filter.EndDateTo.Value);

            return filteredProjects.ToList();
        }


        public async Task CreateProjectAsync(CreateProjectDto dto)
        {
            var projectId = await _projectRepository.AddAsync(dto);
            var userProfileId = await _userProfileRepository.GetUserProfileIdByUserIdAsync(dto.CreatedBy);
            await _projectMemberRepository.AddAsync(projectId, userProfileId);
        }

        public Task<IEnumerable<SelectListItem>> GetProjectStatusOptions()
        {
            return Task.FromResult(Enum.GetValues(typeof(ProjectStatus))
                   .Cast<ProjectStatus>()
                   .Select(status => new SelectListItem
                   {
                       Value = status.ToString(),
                       Text = status.ToString()
                   }));
        }
    }
}
