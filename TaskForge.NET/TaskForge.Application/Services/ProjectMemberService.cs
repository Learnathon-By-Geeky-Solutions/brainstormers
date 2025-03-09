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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskForge.Application.Services
{
    public class ProjectMemberService: IProjectMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProjectMemberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProjectMember?> GetByIdAsync(int memberId)
        {
            return await _projectMemberRepository.GetByIdAsync(memberId);
        }

        public async Task<ProjectMemberDto?> GetUserProjectRoleAsync(string userId, int projectId)
        {
            return await _unitOfWork.ProjectMembers.GetUserProjectRoleAsync(userId, projectId);
        }

        public async Task<List<ProjectMemberDto>> GetProjectMembersAsync(int projectId)
        {
            return await _unitOfWork.ProjectMembers.GetProjectMembersAsync(projectId);
        }

        public async Task RemoveAsync(int memberId)
        {
            var member = await GetByIdAsync(memberId);
            if (member != null)
            {
                await _projectMemberRepository.RemoveAsync(member);
            }
        }
    }
}
