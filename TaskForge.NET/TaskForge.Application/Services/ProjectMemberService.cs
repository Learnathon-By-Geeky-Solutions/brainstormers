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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskForge.Application.Services
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProjectMemberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProjectMember?> GetByIdAsync(int memberId)
        {
            return await _unitOfWork.ProjectMembers.GetByIdAsync(memberId);
        }

        public async Task<ProjectMemberDto?> GetUserProjectRoleAsync(string userId, int projectId)
        {
            var projectMember = await _unitOfWork.ProjectMembers.FindByExpressionAsync(
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
            var projectMembers = await _unitOfWork.ProjectMembers.FindByExpressionAsync(
                pm => pm.ProjectId == projectId,
                includes: query => query
                    .Include(pm => pm.UserProfile)
                        .ThenInclude(pm => pm.User)
                );

            return projectMembers.Select(pm => new ProjectMemberDto
            {
                Id = pm.Id,
                Name = pm.UserProfile.FullName ?? "Unknown User",
                Email = pm.UserProfile.User?.UserName ?? "No Email",
                Role = pm.Role
            }).ToList();
        }

        public async Task RemoveAsync(int memberId)
        {
            await _unitOfWork.ProjectMembers.DeleteByIdAsync(memberId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetUserProjectCountAsync(int? userProfileId)
        {
            var projectMembers = await _unitOfWork.ProjectMembers.FindByExpressionAsync(
                predicate: pm => pm.UserProfileId == userProfileId
            );

            return projectMembers.Select(pm => pm.ProjectId).Distinct().Count();
        }

    }
}