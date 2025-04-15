using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TaskForge.Domain.Entities.Common;

namespace TaskForge.Application.Interfaces.Repositories.Common
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindByExpressionAsync(
           Expression<Func<T, bool>> predicate,
           Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
           Func<IQueryable<T>, IQueryable<T>>? includes = null,
           int? take = null,
           int? skip = null);
		Task<(IEnumerable<T> Items, int TotalCount)> GetPaginatedListAsync(
			 Expression<Func<T, bool>> predicate,
			 Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
			 Func<IQueryable<T>, IQueryable<T>>? includes = null,
			 int? take = null,
			 int? skip = null);
		Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteByIdAsync(int id);
        Task DeleteByIdsAsync(IEnumerable<int> ids);
        Task RestoreByIdAsync(int id);
        Task RestoreByIdsAsync(IEnumerable<int> ids);
        Task<bool> ExistsAsync(int id);
       
    }
}
