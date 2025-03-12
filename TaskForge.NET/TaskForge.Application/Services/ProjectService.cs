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
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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
            using var transaction = await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted); // Specify isolation level

            try
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
                await _unitOfWork.SaveChangesAsync(); // Ensure Project.Id is generated

                var projectId = project.Id;

                // 2. Get the UserProfileId for CreatedBy
                var userProfileId = (await _unitOfWork.UserProfiles.FindByExpressionAsync(up => up.UserId == dto.CreatedBy))
                                   .Select(up => up.Id)
                                   .FirstOrDefault();

                if (userProfileId == default)
                    throw new Exception("User profile not found for the provided CreatedBy user ID.");

                // 3. Add the creator as a member of the project
                var projectMember = new ProjectMember
                {
                    ProjectId = projectId,
                    UserProfileId = userProfileId,
                    Role = ProjectRole.Admin
                };

                await _unitOfWork.ProjectMembers.AddAsync(projectMember);
                await _unitOfWork.SaveChangesAsync(); // Commit both entities

                // 4. Commit transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Rollback in case of an error
                throw new Exception("Error creating project: " + ex.Message, ex); // Re-throw with additional context
            }
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


        public async Task<IEnumerable<ProjectWithRoleDto>> GetFilteredProjectsAsync(ProjectFilterDto filter)
        {
            if (filter.UserId == null)
            {
                return Enumerable.Empty<ProjectWithRoleDto>(); // Handle the case when UserId is null, returning an empty collection
            }

            var userProfileId = await _userProfileService.GetByUserIdAsync(filter.UserId); // Safe access to non-nullable UserId
            if (userProfileId == 0) return Enumerable.Empty<ProjectWithRoleDto>(); // Return empty if user profile is not found

            var projectMembers = await _unitOfWork.ProjectMembers.FindByExpressionAsync(
                predicate: pm => pm.UserProfileId == userProfileId,
                orderBy: null, // Optional: Add sorting if needed
                includes: new Expression<Func<ProjectMember, object>>[] { pm => pm.Project }, // Correct include expression
                take: null, // Optional: Add pagination if needed
                skip: null  // Optional: Add pagination if needed
            );


            if (projectMembers == null || !projectMembers.Any())
            {
                return Enumerable.Empty<ProjectWithRoleDto>(); // No projects found
            }

            // Apply filtering
            var filteredProjects = projectMembers
                .Where(pm =>
                    (string.IsNullOrWhiteSpace(filter.Title) || pm.Project.Title.Contains(filter.Title)) &&
                    (!filter.Status.HasValue || pm.Project.Status == filter.Status.Value) &&
                    (!filter.Role.HasValue || pm.Role == filter.Role.Value) &&
                    (!filter.StartDateFrom.HasValue || pm.Project.StartDate.Date >= filter.StartDateFrom.Value) &&
                    (!filter.StartDateTo.HasValue || pm.Project.StartDate.Date <= filter.StartDateTo.Value) &&
                    (!filter.EndDateFrom.HasValue || (pm.Project.EndDate.HasValue && pm.Project.EndDate.Value.Date >= filter.EndDateFrom.Value)) &&
                    (!filter.EndDateTo.HasValue || (pm.Project.EndDate.HasValue && pm.Project.EndDate.Value.Date <= filter.EndDateTo.Value))
                )
                .Select(pm => new ProjectWithRoleDto
                {
                    ProjectId = pm.Project.Id,
                    Title = pm.Project.Title,
                    Status = pm.Project.Status,
                    StartDate = pm.Project.StartDate,
                    EndDate = pm.Project.EndDate,
                    UserRoleInThisProject = pm.Role 
                });


            // Define the sorting logic
            Func<IQueryable<ProjectWithRoleDto>, IOrderedQueryable<ProjectWithRoleDto>> _orderBy = query =>
            {
                var sortOrder = filter.SortOrder?.ToLower() ?? "asc"; // Default to ascending order if SortOrder is null

                return filter.SortBy?.ToLower() switch
                {
                    "title" => sortOrder == "asc" ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title),
                    "status" => sortOrder == "asc" ? query.OrderBy(p => p.Status) : query.OrderByDescending(p => p.Status),
                    "role" => sortOrder == "asc" ? query.OrderBy(p => p.UserRoleInThisProject) : query.OrderByDescending(p => p.UserRoleInThisProject),
                    "startdate" => sortOrder == "asc" ? query.OrderBy(p => p.StartDate) : query.OrderByDescending(p => p.StartDate),
                    "enddate" => sortOrder == "asc" ? query.OrderBy(p => p.EndDate) : query.OrderByDescending(p => p.EndDate),
                    _ => query.OrderBy(p => p.ProjectId) // Default sorting by ID
                };
            };

            // Apply sorting
            return filter.SortBy?.ToLower() switch
            {
                "title" => filter.SortOrder == "desc" ? filteredProjects.OrderByDescending(p => p.Title) : filteredProjects.OrderBy(p => p.Title),
                "status" => filter.SortOrder == "desc" ? filteredProjects.OrderByDescending(p => p.Status) : filteredProjects.OrderBy(p => p.Status),
                "role" => filter.SortOrder == "desc" ? filteredProjects.OrderByDescending(p => p.UserRoleInThisProject) : filteredProjects.OrderBy(p => p.UserRoleInThisProject),
                "startdate" => filter.SortOrder == "desc" ? filteredProjects.OrderByDescending(p => p.StartDate) : filteredProjects.OrderBy(p => p.StartDate),
                "enddate" => filter.SortOrder == "desc" ? filteredProjects.OrderByDescending(p => p.EndDate) : filteredProjects.OrderBy(p => p.EndDate),
                _ => filteredProjects.OrderBy(p => p.ProjectId) // Default sorting by ID
            };
        }


    }
}
