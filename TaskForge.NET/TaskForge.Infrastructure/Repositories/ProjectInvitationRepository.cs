using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Infrastructure.common.Repositories;
using TaskForge.Infrastructure.Data;

namespace TaskForge.Infrastructure.Repositories
{
    public class ProjectInvitationRepository : Repository<ProjectInvitation>, IProjectInvitationRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectInvitationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

    }

}


