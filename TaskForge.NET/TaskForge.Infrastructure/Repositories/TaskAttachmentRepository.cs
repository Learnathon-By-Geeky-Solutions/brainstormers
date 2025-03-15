using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories.Common;

namespace TaskForge.Infrastructure.Repositories
{
    public class TaskAttachmentRepository : Repository<TaskAttachment>, ITaskAttachmentRepository
    {
        public TaskAttachmentRepository(ApplicationDbContext context, IUserContextService userContextService)
            : base(context, userContextService)
        {
        }
    }
}