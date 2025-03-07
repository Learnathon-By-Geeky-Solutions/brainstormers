using Microsoft.AspNetCore.Identity;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Services
{
    public class ProjectInvitationService : IProjectInvitationService
    {
        private readonly IProjectInvitationRepository _invitationRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectInvitationService(
            IProjectInvitationRepository invitationRepository,
            IUserProfileRepository userProfileRepository,
            IProjectMemberRepository projectMemberRepository,
            UserManager<IdentityUser> userManager)
        {
            _invitationRepository = invitationRepository;
            _userProfileRepository = userProfileRepository;
            _projectMemberRepository = projectMemberRepository;
            _userManager = userManager;
        }

        public async Task<ProjectInvitation?> GetByIdAsync(int invitationId)
        {
            return await _invitationRepository.GetByIdAsync(invitationId);
        }

        public async Task<List<ProjectInvitation>> GetInvitationListAsync(int projectId)
        {
            return await _invitationRepository.GetByProjectIdAsync(projectId);
        }

        public async Task<bool> AddAsync(int projectId, string invitedUserEmail, ProjectRole assignedRole)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(invitedUserEmail);
            if (user == null)
            {
                return false; // User not found
            }

            // Find the UserProfile for this user
            var userProfileId = await _userProfileRepository.GetByUserIdAsync(user.Id);
            if (userProfileId == 0)
            {
                return false; // User profile not found
            }

            var member = await _projectMemberRepository.GetUserProjectRoleAsync(user.Id, projectId);
            if (member != null)
            {
                return false; // User already exists.
            }


            // Create a new invitation
            var invitation = new ProjectInvitation
            {
                ProjectId = projectId,
                InvitedUserProfileId = userProfileId,
                Status = InvitationStatus.Pending,
                AssignedRole = assignedRole,
                InvitationSentDate = DateTime.UtcNow,
                CreatedBy = user.Id
            };

            // Save to database
            await _invitationRepository.AddAsync(invitation);
            return true;
        }

        public async Task UpdateInvitationStatusAsync(int id, InvitationStatus status)
        {
            await _invitationRepository.UpdateInvitationStatusAsync(id, status);
        }
    }
}
