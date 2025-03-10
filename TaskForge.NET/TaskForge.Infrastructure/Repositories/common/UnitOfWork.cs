using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories;
using System.Data;

namespace TaskForge.Infrastructure.common.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IProjectRepository Projects { get; }
        public ITaskRepository Tasks { get; }
        public IProjectMemberRepository ProjectMembers { get; }
        public IProjectInvitationRepository ProjectInvitations { get; }
        public IUserProfileRepository UserProfiles { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Projects = new ProjectRepository(context);
            Tasks = new TaskRepository(context);
            ProjectMembers = new ProjectMemberRepository(context);
            ProjectInvitations = new ProjectInvitationRepository(context);
            UserProfiles = new UserProfileRepository(context);
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
