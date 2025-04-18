﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories.Common;
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

        public async Task<PaginatedList<ProjectInvitation>> GetInvitationListAsync(int projectId, int pageIndex, int pageSize)
        {
            var (projectInvitationList, totalCount) = await _unitOfWork.ProjectInvitations.GetPaginatedListAsync(
                predicate: pi => pi.ProjectId == projectId,
                includes: query => query
                    .Include(pi => pi.InvitedUserProfile) // Include UserProfile
                        .ThenInclude(pi => pi.User), // Include User inside UserProfile
                skip: (pageIndex - 1) * pageSize,
                take: pageSize
               );

            return new PaginatedList<ProjectInvitation>(projectInvitationList, totalCount, pageIndex, pageSize);
        }

        public async Task<ServiceResult> AddAsync(int projectId, string invitedUserEmail, ProjectRole assignedRole)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(invitedUserEmail);
            if (user == null)
            {
                return ServiceResult.FailureResult("User not found.");
            }

            // Find the UserProfile for this user
            var userProfile = await _unitOfWork.UserProfiles
                .FindByExpressionAsync(up => up.UserId == user.Id);

            var userProfileId = userProfile.FirstOrDefault()?.Id;
            if (!userProfileId.HasValue || userProfileId.Value == 0)
            {
                return ServiceResult.FailureResult("User profile not found.");
            }

            // Check if the user is already a member of the project
            var member = await _unitOfWork.ProjectMembers.FindByExpressionAsync(pm => pm.UserProfile.UserId == user.Id && pm.ProjectId == projectId);
            if (member != null && member.Any())
            {
                return ServiceResult.FailureResult("User is already a member of this project.");
            }

            // Check if an invitation already exists
            var existingInvitation = await _unitOfWork.ProjectInvitations.FindByExpressionAsync(i => i.InvitedUserProfileId == userProfileId.Value
                                            && i.ProjectId == projectId && i.Status == InvitationStatus.Pending);
            if (existingInvitation.Any())
            {
                return ServiceResult.FailureResult("An invitation has already been sent to this user.");
            }

            // Create a new invitation
            var invitation = new ProjectInvitation
            {
                ProjectId = projectId,
                InvitedUserProfileId = userProfileId.Value,
                Status = InvitationStatus.Pending,
                AssignedRole = assignedRole,
                InvitationSentDate = DateTime.UtcNow,
            };

            // Save to database
            await _unitOfWork.ProjectInvitations.AddAsync(invitation);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult.SuccessResult("Invitation sent successfully.");
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

            await _unitOfWork.ProjectInvitations.UpdateAsync(invitation);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<PaginatedList<ProjectInvitation>> GetInvitationsForUserAsync(int? userProfileId, int pageIndex, int pageSize)
        {
            if (!userProfileId.HasValue) return new PaginatedList<ProjectInvitation>(new List<ProjectInvitation>(), 0, pageIndex, pageSize);

            var (projectInvitaionList, totalCount) = await _unitOfWork.ProjectInvitations.GetPaginatedListAsync(predicate: pi => pi.InvitedUserProfileId == userProfileId,
                 orderBy: null,
                 includes: query => query.Include(pi => pi.Project),
                 skip: (pageIndex - 1) * pageSize,
                 take: pageSize
             );

            return new PaginatedList<ProjectInvitation>(projectInvitaionList, totalCount, pageIndex, pageSize);
        }
    }
}
