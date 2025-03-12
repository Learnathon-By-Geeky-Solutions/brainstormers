using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories;
using System.Data;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Interfaces.Repositories.Common;

namespace TaskForge.Infrastructure.Repositories.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IProjectRepository Projects { get; }
        public ITaskRepository Tasks { get; }
        public IProjectMemberRepository ProjectMembers { get; }
        public IProjectInvitationRepository ProjectInvitations { get; }
        public IUserProfileRepository UserProfiles { get; }

        public UnitOfWork(ApplicationDbContext context, IUserContextService userContextService)
        {
            _context = context;

            Projects = new ProjectRepository(context, userContextService);
            Tasks = new TaskRepository(context, userContextService);
            ProjectMembers = new ProjectMemberRepository(context, userContextService);
            ProjectInvitations = new ProjectInvitationRepository(context, userContextService);
            UserProfiles = new UserProfileRepository(context, userContextService);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return await _context.Database.BeginTransactionAsync(isolationLevel);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
