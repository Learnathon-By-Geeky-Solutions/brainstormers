using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<Project>?> GetAllProjectsAsync(string userId)
        {
            var userProfileId = await _userProfileRepository.GetUserProfileIdByUserIdAsync(userId);
            var projectIds = await _projectMemberRepository.GetProjectIdsByUserProfileIdAsync(userProfileId);
            var projects = new List<Project>();
            foreach (var projectId in projectIds)
            {
                var project = await _projectRepository.GetProjectByIdAsync(projectId);
                if (project == null) continue;
                projects.Add(project);
            }

            return projects;
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

        public async Task<Project?> GetProjectByIdAsync(int projectId)
        {
            return await _projectRepository.GetProjectByIdAsync(projectId);
        }
    }
}
