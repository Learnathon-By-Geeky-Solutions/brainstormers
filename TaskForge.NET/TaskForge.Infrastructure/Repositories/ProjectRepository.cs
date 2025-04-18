using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories.Common;

namespace TaskForge.Infrastructure.Repositories
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        // Pass the context and IUserContextService to the base Repository class
        public ProjectRepository(ApplicationDbContext context, IUserContextService userContextService) 
            : base(context, userContextService)
        {

        }
    }
}
