using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories.Common;

namespace TaskForge.Infrastructure.Repositories
{
    public class ProjectInvitationRepository : Repository<ProjectInvitation>, IProjectInvitationRepository
    {
        // This interface is intentionally left empty as a marker for repository types
        public ProjectInvitationRepository(ApplicationDbContext context, IUserContextService userContextService) 
            : base(context, userContextService)
        {

        }
    }
}
