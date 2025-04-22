using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.Services
{
	public class ProjectInvitationService : IProjectInvitationService
	{
		private readonly IProjectInvitationRepository _projectInvitationRepo;
		private readonly IProjectMemberRepository _projectMemberRepo;
		private readonly IUserProfileRepository _userProfileRepo;
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<IdentityUser> _userManager;

		public ProjectInvitationService(
			IProjectInvitationRepository projectInvitationRepo,
			IProjectMemberRepository projectMemberRepo,
			IUserProfileRepository userProfileRepo,
			IUnitOfWork unitOfWork,
			UserManager<IdentityUser> userManager)
		{
			_projectInvitationRepo = projectInvitationRepo;
			_projectMemberRepo = projectMemberRepo;
			_userProfileRepo = userProfileRepo;
			_unitOfWork = unitOfWork;
			_userManager = userManager;
		}

		public async Task<ProjectInvitation?> GetByIdAsync(int invitationId)
		{
			return await _projectInvitationRepo.GetByIdAsync(invitationId);
		}

		public async Task<PaginatedList<ProjectInvitation>> GetInvitationListAsync(int projectId, int pageIndex, int pageSize)
		{
			var (projectInvitationList, totalCount) = await _projectInvitationRepo.GetPaginatedListAsync(
				predicate: pi => pi.ProjectId == projectId,
				includes: query => query
					.Include(pi => pi.InvitedUserProfile)
						.ThenInclude(pi => pi.User),
				skip: (pageIndex - 1) * pageSize,
				take: pageSize
			);

			return new PaginatedList<ProjectInvitation>(projectInvitationList, totalCount, pageIndex, pageSize);
		}

		public async Task<ServiceResult> AddAsync(int projectId, string invitedUserEmail, ProjectRole assignedRole)
		{
			var user = await _userManager.FindByEmailAsync(invitedUserEmail);
			if (user == null)
			{
				return ServiceResult.FailureResult("User not found.");
			}

			var userProfile = await _userProfileRepo.FindByExpressionAsync(up => up.UserId == user.Id);
			var userProfileId = userProfile.FirstOrDefault()?.Id;

			if (!userProfileId.HasValue || userProfileId.Value == 0)
			{
				return ServiceResult.FailureResult("User profile not found.");
			}

			var member = await _projectMemberRepo.FindByExpressionAsync(pm =>
				pm.UserProfile.UserId == user.Id && pm.ProjectId == projectId);
			if (member.Any())
			{
				return ServiceResult.FailureResult("User is already a member of this project.");
			}

			var existingInvitation = await _projectInvitationRepo.FindByExpressionAsync(i =>
				i.InvitedUserProfileId == userProfileId.Value &&
				i.ProjectId == projectId &&
				i.Status == InvitationStatus.Pending);

			if (existingInvitation.Any())
			{
				return ServiceResult.FailureResult("An invitation has already been sent to this user.");
			}

			var invitation = new ProjectInvitation
			{
				ProjectId = projectId,
				InvitedUserProfileId = userProfileId.Value,
				Status = InvitationStatus.Pending,
				AssignedRole = assignedRole,
				InvitationSentDate = DateTime.UtcNow,
			};

			await _projectInvitationRepo.AddAsync(invitation);
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

				var projectMember = new ProjectMember
				{
					ProjectId = invitation.ProjectId,
					UserProfileId = invitation.InvitedUserProfileId,
					Role = invitation.AssignedRole,
				};

				await _projectMemberRepo.AddAsync(projectMember);
			}
			else if (status == InvitationStatus.Declined)
			{
				invitation.DeclinedDate = DateTime.UtcNow;
				invitation.AcceptedDate = null;
			}

			await _projectInvitationRepo.UpdateAsync(invitation);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<PaginatedList<ProjectInvitation>> GetInvitationsForUserAsync(int? userProfileId, int pageIndex, int pageSize)
		{
			if (!userProfileId.HasValue)
			{
				return new PaginatedList<ProjectInvitation>(new List<ProjectInvitation>(), 0, pageIndex, pageSize);
			}

			var (projectInvitationList, totalCount) = await _projectInvitationRepo.GetPaginatedListAsync(
				predicate: pi => pi.InvitedUserProfileId == userProfileId,
				includes: query => query.Include(pi => pi.Project),
				skip: (pageIndex - 1) * pageSize,
				take: pageSize
			);

			return new PaginatedList<ProjectInvitation>(projectInvitationList, totalCount, pageIndex, pageSize);
		}
	}
}
