using System.Linq.Expressions;

namespace TaskForge.Application.Interfaces.Repositories.common
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<bool> ExistsAsync(int id);

        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,      // WHERE condition
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, // ORDER BY
            string[]? includes = null,                // Include navigation properties
            int? take = null,                         // Pagination
            int? skip = null                          // Pagination
        );
    }
}
