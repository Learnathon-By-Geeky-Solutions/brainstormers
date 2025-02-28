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
        public ProjectService(
            IProjectRepository projectRepository, 
            IUserProfileRepository userProfileRepository, 
            IProjectMemberRepository projectMemberRepository)
        {
            _projectRepository = projectRepository;
            _userProfileRepository = userProfileRepository;
            _projectMemberRepository = projectMemberRepository;
        }

        public async Task CreateProjectAsync(CreateProjectDto dto)
        {
            var projectId = await _projectRepository.AddAsync(dto);
            var userProfileId = await _userProfileRepository.GetUserProfileIdByUserIdAsync(dto.CreatedBy);
            if (userProfileId == null)
                throw new ArgumentException("User profile not found.");
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

        public async Task<Project?> GetByIdAsync(int projectId)
        {
            return await _projectRepository.GetProjectByIdAsync(projectId);
        }

        public async Task<IEnumerable<Project>> GetFilteredProjectsAsync(ProjectFilterDto filter)
        {
            var projects = await GetUserProjectsAsync(filter.UserId);
            if (!projects.Any()) return Enumerable.Empty<Project>();

            var filteredProjects = ApplyFilters(projects.AsQueryable(), filter);
            var sortedProjects = ApplySorting(filteredProjects, filter.SortBy, filter.SortOrder);

            return sortedProjects.ToList();
        }

        // Fetch projects for the user
        private async Task<IEnumerable<Project>> GetUserProjectsAsync(string userId)
        {
            var userProfileId = await _userProfileRepository.GetUserProfileIdByUserIdAsync(userId);
            if (userProfileId == null) return Enumerable.Empty<Project>();

            var projectIds = await _projectMemberRepository.GetProjectIdsByUserProfileIdAsync(userProfileId);
            if (projectIds == null || !projectIds.Any()) return Enumerable.Empty<Project>();

            var projects = new List<Project>();
            foreach (var projectId in projectIds)
            {
                var project = await _projectRepository.GetProjectByIdAsync(projectId);
                if (project != null) projects.Add(project);
            }

            return projects;
        }

        // Filter logic separated
        private IQueryable<Project> ApplyFilters(IQueryable<Project> projects, ProjectFilterDto filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.Title))
                projects = projects.Where(p => p.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));

            if (filter.Status.HasValue)
                projects = projects.Where(p => p.Status == filter.Status.Value);

            if (filter.StartDateFrom.HasValue)
                projects = projects.Where(p => p.StartDate.Date >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                projects = projects.Where(p => p.StartDate.Date <= filter.StartDateTo.Value);

            if (filter.EndDateFrom.HasValue)
                projects = projects.Where(p => p.EndDate.HasValue && p.EndDate.Value.Date >= filter.EndDateFrom.Value);

            if (filter.EndDateTo.HasValue)
                projects = projects.Where(p => p.EndDate.HasValue && p.EndDate.Value.Date <= filter.EndDateTo.Value);

            return projects;
        }

        // Sorting logic separated
        private IQueryable<Project> ApplySorting(IQueryable<Project> projects, string? sortBy, string sortOrder = "asc")
        {
            bool isAscending = sortOrder?.ToLower() == "asc";

            return sortBy?.ToLower() switch
            {
                "title" => isAscending ? projects.OrderBy(p => p.Title) : projects.OrderByDescending(p => p.Title),
                "status" => isAscending ? projects.OrderBy(p => p.Status) : projects.OrderByDescending(p => p.Status),
                "startdate" => isAscending ? projects.OrderBy(p => p.StartDate) : projects.OrderByDescending(p => p.StartDate),
                "enddate" => isAscending ? projects.OrderBy(p => p.EndDate) : projects.OrderByDescending(p => p.EndDate),
                _ => projects.OrderBy(p => p.Id)
            };
        }
    }
}
