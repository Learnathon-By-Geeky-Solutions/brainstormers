using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Repositories;
using TaskForge.Infrastructure.Repositories.Common;
using TaskForge.Infrastructure.Data;
using TaskForge.Application.Interfaces.Services;

namespace TaskForge.Infrastructure.Repositories
{
    public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
    {
        public UserProfileRepository(ApplicationDbContext context, IUserContextService userContextService) : base(context, userContextService)
        {

        }

    }
}
