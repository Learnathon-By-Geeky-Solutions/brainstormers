using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetUserProfileByUserIdAsync(string userId);
    }
}
