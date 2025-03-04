using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Services
{
    public interface IProjectMemberService
    {
        Task<bool> IsUserAssignedToProjectAsync(string userId, int projectId);
        Task<List<ProjectMemberDto>> GetProjectMembersAsync(int projectId);
    }
}
