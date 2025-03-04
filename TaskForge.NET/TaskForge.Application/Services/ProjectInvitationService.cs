using Microsoft.AspNetCore.Identity;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Services;

namespace TaskForge.Application.Services
{
    public class ProjectInvitationService : IProjectInvitationService
    {
        private readonly IProjectInvitationRepository _invitationRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectInvitationService(
            IProjectInvitationRepository invitationRepository,
            IUserProfileRepository userProfileRepository,
            UserManager<IdentityUser> userManager)
        {
            _invitationRepository = invitationRepository;
            _userProfileRepository = userProfileRepository;
            _userManager = userManager;
        }

        public async Task<bool> SendInvitationAsync(int projectId, string invitedUserEmail, ProjectRole assignedRole)
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
    }
}
