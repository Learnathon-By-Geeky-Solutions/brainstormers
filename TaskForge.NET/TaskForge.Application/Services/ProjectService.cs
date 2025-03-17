using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
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
                       Text = status.GetDisplayName().ToString()
                   }));
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId)
        {
            return (await _unitOfWork.Projects.FindByExpressionAsync(
                    predicate: p => p.Id == projectId,
                    includes: query => query
                        .Include(p => p.Members)
                            .ThenInclude(m => m.UserProfile)
                                .ThenInclude(u => u.User)
                        .Include(p => p.Tasks)
                        .Include(p => p.Invitations)
                            .ThenInclude(i => i.InvitedUserProfile)))
                .FirstOrDefault();
        }


        public async Task<IEnumerable<ProjectWithRoleDto>> GetFilteredProjectsAsync(ProjectFilterDto filter)
        {
            if (filter.UserId == null)
            {
                return Enumerable.Empty<ProjectWithRoleDto>(); // Handle the case when UserId is null, returning an empty collection
            }

            var userProfileId = await _userProfileService.GetByUserIdAsync(filter.UserId); // Safe access to non-nullable UserId
            if (userProfileId == 0) return Enumerable.Empty<ProjectWithRoleDto>(); // Return empty if user profile is not found

            Expression<Func<ProjectMember, bool>> _predicate = pm => 
                    pm.UserProfileId == userProfileId &&
                    (string.IsNullOrWhiteSpace(filter.Title) || pm.Project.Title.Contains(filter.Title)) &&
                    (!filter.Status.HasValue || pm.Project.Status == filter.Status.Value) &&
                    (!filter.Role.HasValue || pm.Role == filter.Role.Value) &&
                    (!filter.StartDateFrom.HasValue || pm.Project.StartDate.Date >= filter.StartDateFrom.Value) &&
                    (!filter.StartDateTo.HasValue || pm.Project.StartDate.Date <= filter.StartDateTo.Value) &&
                    (!filter.EndDateFrom.HasValue || (pm.Project.EndDate.HasValue && pm.Project.EndDate.Value.Date >= filter.EndDateFrom.Value)) &&
                    (!filter.EndDateTo.HasValue || (pm.Project.EndDate.HasValue && pm.Project.EndDate.Value.Date <= filter.EndDateTo.Value));

            // Define the sorting logic
            Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>> _orderBy = query =>
            {
                var sortOrder = filter.SortOrder?.ToLower() ?? "asc"; // Default to ascending order if SortOrder is null

                return filter.SortBy?.ToLower() switch
                {
                    "title" => sortOrder == "asc" ? query.OrderBy(pm => pm.Project.Title) : query.OrderByDescending(pm => pm.Project.Title),
                    "status" => sortOrder == "asc" ? query.OrderBy(pm => pm.Project.Status) : query.OrderByDescending(pm => pm.Project.Status),
                    "role" => sortOrder == "asc" ? query.OrderBy(pm => pm.Role) : query.OrderByDescending(pm => pm.Role),
                    "startdate" => sortOrder == "asc" ? query.OrderBy(pm => pm.Project.StartDate) : query.OrderByDescending(pm => pm.Project.StartDate),
                    "enddate" => sortOrder == "asc" ? query.OrderBy(pm => pm.Project.EndDate) : query.OrderByDescending(pm => pm.Project.EndDate),
                    _ => query.OrderBy(pm => pm.ProjectId) // Default sorting by ID
                };
            };

            var filteredProject = await _unitOfWork.ProjectMembers.FindByExpressionAsync(
                predicate: _predicate,
                orderBy: (Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>?)_orderBy,
                includes: query => query
                    .Include(pm => pm.Project),
                take: null,
                skip: null
            );


            var projectWithRoleDtos = filteredProject.Select(pm => new ProjectWithRoleDto
            {
                ProjectId = pm.Project.Id,
                ProjectTitle = pm.Project.Title,
                ProjectStatus = pm.Project.Status,
                ProjectStartDate = pm.Project.StartDate,
                ProjectEndDate = pm.Project.EndDate,
                UserRoleInThisProject = pm.Role
            });
            return projectWithRoleDtos;
        }

    }
}
