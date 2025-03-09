using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserProfileService _userProfileService;
        public ProjectService(IUnitOfWork unitOfWork, IUserProfileService userProfileService)
        {
            _unitOfWork = unitOfWork;
            _userProfileService = userProfileService;
        }



        public async Task CreateProjectAsync(CreateProjectDto dto)
        {
            // 1. Map DTO to Project entity
            var project = new Project
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                StartDate = dto.StartDate,
            };
            if (dto.EndDate != null) project.SetEndDate(dto.EndDate);

            await _unitOfWork.Projects.AddAsync(project);

            var projectId = project.Id;
            var userProfileId = (await _unitOfWork.UserProfiles.FindAsync(predicate:up => up.UserId == dto.CreatedBy)).Select(up => up.Id).FirstOrDefault();

            // 2. Add the creator as a member of the project
            var projectMember = new ProjectMember
            {
                ProjectId = projectId,
                UserProfileId = userProfileId,
                Role = ProjectRole.Admin
            };

            await _unitOfWork.ProjectMembers.AddAsync(projectMember);
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

        public async Task<Project?> GetProjectByIdAsync(int projectId)
        {
            return await _unitOfWork.Projects.GetByIdAsync(projectId);
        }


        public async Task<IEnumerable<Project>> GetFilteredProjectsAsync(ProjectFilterDto filter)
        {
            if (filter.UserId == null)
            {
                return Enumerable.Empty<Project>(); // Handle the case when UserId is null, returning an empty collection
            }

            var userProfileId = await _userProfileService.GetByUserIdAsync(filter.UserId); // Safe access to non-nullable UserId
            if (userProfileId == 0) return Enumerable.Empty<Project>(); // Return empty if user profile is not found

            var projectIds = (await _unitOfWork.ProjectMembers.FindAsync(predicate: pm => pm.UserProfileId == userProfileId)).Select(pm => pm.ProjectId);

            if (projectIds == null || !projectIds.Any()) return Enumerable.Empty<Project>(); // Return empty if no projects found for user

            // Define the filtering predicate
            Expression<Func<Project, bool>> _predicate = p =>
                projectIds.Contains(p.Id) && // Filter by the user's project IDs
                (string.IsNullOrWhiteSpace(filter.Title) || p.Title.Contains(filter.Title)) && // Filter by title (if provided)
                (!filter.Status.HasValue || p.Status == filter.Status.Value) && // Filter by status (if provided)
                (!filter.StartDateFrom.HasValue || p.StartDate.Date >= filter.StartDateFrom.Value) && // Filter by start date (if provided)
                (!filter.StartDateTo.HasValue || p.StartDate.Date <= filter.StartDateTo.Value) && // Filter by start date range (if provided)
                (!filter.EndDateFrom.HasValue || (p.EndDate.HasValue && p.EndDate.Value.Date >= filter.EndDateFrom.Value)) && // Filter by end date (if provided)
                (!filter.EndDateTo.HasValue || (p.EndDate.HasValue && p.EndDate.Value.Date <= filter.EndDateTo.Value)); // Filter by end date range (if provided)

            // Define the sorting logic
            Func<IQueryable<Project>, IOrderedQueryable<Project>> _orderBy = query =>
            {
                var sortOrder = filter.SortOrder?.ToLower() ?? "asc"; // Default to ascending order if SortOrder is null

                return filter.SortBy?.ToLower() switch
                {
                    "title" => sortOrder == "asc" ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title),
                    "status" => sortOrder == "asc" ? query.OrderBy(p => p.Status) : query.OrderByDescending(p => p.Status),
                    "startdate" => sortOrder == "asc" ? query.OrderBy(p => p.StartDate) : query.OrderByDescending(p => p.StartDate),
                    "enddate" => sortOrder == "asc" ? query.OrderBy(p => p.EndDate) : query.OrderByDescending(p => p.EndDate),
                    _ => query.OrderBy(p => p.Id) // Default sorting by ID
                };
            };

            // Fetch the filtered and sorted projects
            var filteredProjects = await _unitOfWork.Projects.FindAsync(
                predicate: _predicate,
                orderBy: _orderBy
            );

            return filteredProjects ?? Enumerable.Empty<Project>(); // Return an empty collection if FindAsync returns null
        }


    }
}
