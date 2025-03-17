using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities.Common;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Services;

namespace TaskForge.Infrastructure.Repositories.Common
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly DbSet<T> _dbSet;
        private readonly IUserContextService _userContextService;

        public Repository(ApplicationDbContext context, IUserContextService userContextService)
        {
            _dbSet = context.Set<T>();
            _userContextService = userContextService;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.Where(e => !e.IsDeleted).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task AddAsync(T entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            entity.CreatedBy = await _userContextService.GetCurrentUserIdAsync();
            await _dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = await _userContextService.GetCurrentUserIdAsync();
            _dbSet.Update(entity);
        }

        public async Task DeleteByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedBy = await _userContextService.GetCurrentUserIdAsync();
                entity.UpdatedDate = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
        }

        public async Task DeleteByIdsAsync(IEnumerable<int> ids)
        {
            var entities = await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync();
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.UpdatedBy = await _userContextService.GetCurrentUserIdAsync();
                entity.UpdatedDate = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
        }


        public async Task RestoreByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null && entity.IsDeleted)
            {
                entity.IsDeleted = false;
                entity.UpdatedBy = await _userContextService.GetCurrentUserIdAsync();
                entity.UpdatedDate = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
        }

        public async Task RestoreByIdsAsync(IEnumerable<int> ids)
        {
            var entities = await _dbSet.Where(e => ids.Contains(e.Id) && e.IsDeleted).ToListAsync();
            foreach (var entity in entities)
            {
                entity.IsDeleted = false;
                entity.UpdatedBy = await _userContextService.GetCurrentUserIdAsync();
                entity.UpdatedDate = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<IEnumerable<T>> FindByExpressionAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<T>>? includes = null, 
            int? take = null,
            int? skip = null)
        {
            IQueryable<T> query = _dbSet.Where(e => !e.IsDeleted);


            if (includes != null)
            {
                query = includes(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }
    }
}
