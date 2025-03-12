using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Infrastructure.Repositories;
using TaskForge.Infrastructure.Repositories.Common;
using TaskForge.Infrastructure.Data;
using TaskForge.Application.Interfaces.Services;

namespace TaskForge.Infrastructure.Repositories
{
    public class ProjectInvitationRepository : Repository<ProjectInvitation>, IProjectInvitationRepository
    {
        public ProjectInvitationRepository(ApplicationDbContext context, IUserContextService userContextService) : base(context, userContextService)
        {

        }

    }

}


