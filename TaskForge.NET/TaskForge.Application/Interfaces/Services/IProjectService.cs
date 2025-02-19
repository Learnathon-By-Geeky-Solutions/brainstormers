using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>?> GetAllProjectsAsync(string userId);
    }
}
