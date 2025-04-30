using Microsoft.EntityFrameworkCore;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Services
{
	public class ProjectMemberService : IProjectMemberService
	{
		private readonly IProjectMemberRepository _projectMemberRepository;
		private readonly IUnitOfWork _unitOfWork;

		public ProjectMemberService(IProjectMemberRepository projectMemberRepository, IUnitOfWork unitOfWork)
		{
			_projectMemberRepository = projectMemberRepository;
			_unitOfWork = unitOfWork;
		}


		public async Task<ProjectMember?> GetByIdAsync(int memberId)
		{
			return await _projectMemberRepository.GetByIdAsync(memberId);
		}

		public async Task<ProjectMemberDto?> GetUserProjectRoleAsync(string userId, int projectId)
		{
			var projectMember = await _projectMemberRepository.FindByExpressionAsync(
				pm => pm.UserProfile.UserId == userId && pm.ProjectId == projectId,
				includes: query => query
					.Include(pm => pm.UserProfile)
						.ThenInclude(pm => pm.User)
				);

			var member = projectMember.FirstOrDefault();
			if (member == null)
				return null;

			return new ProjectMemberDto
			{
				Id = member.Id,
				Name = member.UserProfile.FullName ?? "Unknown User",
				Email = member.UserProfile.User?.UserName ?? "No Email",
				Role = member.Role
			};
		}

		public async Task<List<ProjectMemberDto>> GetProjectMembersAsync(int projectId)
		{
			var projectMembers = await _projectMemberRepository.FindByExpressionAsync(
				predicate: pm => pm.ProjectId == projectId,
				includes: query => query
					.Include(pm => pm.UserProfile)
						.ThenInclude(pm => pm.User),
				orderBy: query => query.OrderBy(pm => pm.Role)
			);

			return projectMembers.Select(pm => new ProjectMemberDto
			{
				Id = pm.Id,
				ProjectId = pm.ProjectId,
				UserProfileId = pm.UserProfileId,
				UserId = pm.UserProfile.UserId,
                Name = pm.UserProfile.FullName ?? "Unknown User",
				Email = pm.UserProfile.User?.UserName ?? "No Email",
				Role = pm.Role
			}).ToList();
		}

		public async Task<int> GetUserProjectCountAsync(int? userProfileId)
		{
			var projectMembers = await _projectMemberRepository.FindByExpressionAsync(
				predicate: pm => pm.UserProfileId == userProfileId
			);

			return projectMembers.Select(pm => pm.ProjectId).Distinct().Count();
		}


		public async Task RemoveAsync(int memberId)
		{
			await _projectMemberRepository.DeleteByIdAsync(memberId);
			await _unitOfWork.SaveChangesAsync();
		}

	}
}
