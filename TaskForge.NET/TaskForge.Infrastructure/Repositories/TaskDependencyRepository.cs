using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories.Common;

namespace TaskForge.Infrastructure.Repositories
{
    public class TaskDependencyRepository : Repository<TaskDependency>, ITaskDependencyRepository
    {
        public TaskDependencyRepository(ApplicationDbContext context, IUserContextService userContextService)
            : base(context, userContextService)
        {

        }
    }
}