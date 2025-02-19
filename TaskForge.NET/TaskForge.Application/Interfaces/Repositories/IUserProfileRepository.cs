using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Repositories
{
    public interface IUserProfileRepository
    {
        Task<int> GetUserProfileIdByUserIdAsync(string userId);
        Task CreateAsync(string userId, string FullName);
    }
}
