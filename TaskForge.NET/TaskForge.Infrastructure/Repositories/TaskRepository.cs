using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.common.Repositories;
using TaskForge.Infrastructure.Data;

namespace TaskForge.Infrastructure.Repositories
{
    public class TaskRepository : Repository<TaskItem>, ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.TaskItems.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<TaskItem>> GetFilteredAsync(TaskFilterDto filter)
        {
            var query = _context.TaskItems.AsQueryable();

            query = ApplyFiltering(query, filter); // Filtering at database level
            query = ApplySorting(query, filter.SortBy, filter.SortOrder); // Sorting at database level

            return await query.ToListAsync(); // Executes single optimized query in SQL
        }

        private IQueryable<TaskItem> ApplyFiltering(IQueryable<TaskItem> query, TaskFilterDto filter)
        {
            if (filter.UserProfileId.HasValue)
                query = query.Where(t => t.AssignedUsers.Any(a => a.UserProfileId == filter.UserProfileId));

            if (filter.ProjectId.HasValue)
                query = query.Where(t => t.ProjectId == filter.ProjectId);

            if (!string.IsNullOrWhiteSpace(filter.Title))
                query = query.Where(t => t.Title.Contains(filter.Title));

            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);

            if (filter.Priority.HasValue)
                query = query.Where(t => t.Priority == filter.Priority.Value);

            if (filter.StartDateFrom.HasValue)
                query = query.Where(t => t.StartDate >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                query = query.Where(t => t.StartDate <= filter.StartDateTo.Value);

            if (filter.DueDateFrom.HasValue)
                query = query.Where(t => t.DueDate >= filter.DueDateFrom.Value);

            if (filter.DueDateTo.HasValue)
                query = query.Where(t => t.DueDate <= filter.DueDateTo.Value);

            return query;
        }

        private IQueryable<TaskItem> ApplySorting(IQueryable<TaskItem> query, string? sortBy, string sortOrder)
        {
            bool isAscending = sortOrder?.ToLowerInvariant() == "asc";

            return sortBy?.ToLowerInvariant() switch
            {
                "title" => isAscending ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title),
                "status" => isAscending ? query.OrderBy(t => t.Status) : query.OrderByDescending(t => t.Status),
                "priority" => isAscending ? query.OrderBy(t => t.Priority) : query.OrderByDescending(t => t.Priority),
                "startdate" => isAscending ? query.OrderBy(t => t.StartDate) : query.OrderByDescending(t => t.StartDate),
                "duedate" => isAscending ? query.OrderBy(t => t.DueDate) : query.OrderByDescending(t => t.DueDate),
                _ => query.OrderBy(t => t.Id)
            };
        }


    }
}
