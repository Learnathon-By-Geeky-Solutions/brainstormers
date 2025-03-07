using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories.common;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Repositories
{
    public interface IUserProfileRepository : IRepository<UserProfile>
    {
        Task<int> GetByUserIdAsync(string userId);
        Task CreateAsync(string userId, string FullName);
    }
}
