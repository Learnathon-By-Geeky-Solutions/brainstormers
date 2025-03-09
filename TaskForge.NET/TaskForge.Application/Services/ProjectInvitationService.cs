using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Services
{
    public class ProjectInvitationService : IProjectInvitationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectInvitationService(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ProjectInvitation?> GetByIdAsync(int invitationId)
        {
            return await _unitOfWork.ProjectInvitations.GetByIdAsync(invitationId);
        }

        public async Task<List<ProjectInvitation>> GetInvitationListAsync(int projectId)
        {
            return (await _unitOfWork.ProjectInvitations.FindAsync(pi => pi.ProjectId == projectId)).ToList();
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
            var userProfile = await _unitOfWork.UserProfiles
                .FindAsync(up => up.UserId == user.Id); // Query using the UserId predicate

            var userProfileId = userProfile.FirstOrDefault()?.Id; // Safe access to Id
            if (!userProfileId.HasValue || userProfileId.Value == 0) // Check if the Id is null or 0
            {
                return false; // User profile not found
            }

            var member = await _unitOfWork.ProjectMembers.FindAsync(pm => pm.UserProfile.UserId == user.Id && pm.ProjectId == projectId);
            if (member != null)
            {
                return false; // User already exists.
            }

            // Create a new invitation
            var invitation = new ProjectInvitation
            {
                ProjectId = projectId,
                InvitedUserProfileId = userProfileId.Value, // Access the Value of the nullable int
                Status = InvitationStatus.Pending,
                AssignedRole = assignedRole,
                InvitationSentDate = DateTime.UtcNow,
                CreatedBy = user.Id
            };

            // Save to database
            await _unitOfWork.ProjectInvitations.AddAsync(invitation);
            return true;
        }


        public async Task UpdateInvitationStatusAsync(int id, InvitationStatus status)
        {
            var invitation = await GetByIdAsync(id);
            if (invitation == null)
            {
                return;
            }

            invitation.Status = status;

            if (status == InvitationStatus.Accepted)
            {
                invitation.AcceptedDate = DateTime.UtcNow;
                invitation.DeclinedDate = null;

                // Create a ProjectMember entry for the user
                var projectMember = new ProjectMember
                {
                    ProjectId = invitation.ProjectId,
                    UserProfileId = invitation.InvitedUserProfileId, // Assign the invited user
                    Role = invitation.AssignedRole, // Assign the role from invitation
                };

                await _unitOfWork.ProjectMembers.AddAsync(projectMember);
            }
            else if (status == InvitationStatus.Declined)
            {
                invitation.DeclinedDate = DateTime.UtcNow;
                invitation.AcceptedDate = null;
            }
            
            _unitOfWork.ProjectInvitations.Update(invitation);
        }
    }
}
