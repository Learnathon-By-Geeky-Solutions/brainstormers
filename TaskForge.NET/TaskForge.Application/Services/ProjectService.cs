using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Services
{
    public class ProjectService
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

        public async Task<IEnumerable<Project>?> GetAllProjectsAsync(string Id)
        {
            var userProfile = await _userProfileRepository.GetUserProfileByUserIdAsync(Id);
            if (userProfile == null)
            {
                throw new KeyNotFoundException("User profile not found.");
            }
            var projectIds = await _projectMemberRepository.GetProjectIdsByUserProfileIdAsync(userProfile.Id);
            var projects = new List<Project>();
            foreach (var projectId in projectIds)
            {
                var project = await _projectRepository.GetProjectByIdAsync(projectId);
                if (project == null) continue;
                projects.Add(project);
            }

            return projects;
        }
    }
}
