using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories.Common;
using TaskForge.Tests.Helpers;
using TaskForge.Tests.TestData.Entities;
using Xunit;

namespace TaskForge.Tests.Infrastructure.Repositories.Common
{
    public class RepositoryTests
    {
        private readonly TestApplicationDbContext _dbContext;
        private readonly Repository<FakeEntity> _repository;
        private readonly Mock<IUserContextService> _userContextServiceMock;

        public RepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;


            _dbContext = new TestApplicationDbContext(options);

            _userContextServiceMock = new Mock<IUserContextService>();
            _userContextServiceMock.Setup(s => s.GetCurrentUserIdAsync()).ReturnsAsync("TestUser");

            _repository = new Repository<FakeEntity>(_dbContext, _userContextServiceMock.Object);

            SeedData();
        }

        private void SeedData()
        {
            var entities = new List<FakeEntity>
            {
                new FakeEntity { Id = 1, Name = "Entity 1", IsDeleted = false },
                new FakeEntity { Id = 2, Name = "Entity 2", IsDeleted = true },
                new FakeEntity { Id = 3, Name = "Entity 3", IsDeleted = false }
            };

            _dbContext.Set<FakeEntity>().AddRange(entities);
            _dbContext.SaveChanges();
        }
        
        [Fact]
        public async Task AddAsync_AddsEntityToDatabase()
        {
            var newEntity = new FakeEntity { Name = "New Entity" };
            await _repository.AddAsync(newEntity);

            var entityInDb = await _dbContext.Set<FakeEntity>().FindAsync(newEntity.Id);
            Assert.NotNull(entityInDb);
            Assert.Equal("New Entity", entityInDb!.Name);
        }

        [Fact]
        public async Task DeleteByIdAsync_SetsIsDeletedToTrue()
        {
            var entity = await _dbContext.Set<FakeEntity>().FirstAsync();
            await _repository.DeleteByIdAsync(entity.Id);

            var deleted = await _dbContext.Set<FakeEntity>().FindAsync(entity.Id);
            Assert.NotNull(deleted);
            Assert.True(deleted!.IsDeleted);
        }

        [Fact]
        public async Task DeleteByIdsAsync_SetsIsDeletedToTrueForAll()
        {
            var ids = await _dbContext.Set<FakeEntity>()
                .Where(e => !e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync();

            await _repository.DeleteByIdsAsync(ids);

            foreach (var id in ids)
            {
                var entity = await _dbContext.Set<FakeEntity>().FindAsync(id);
                Assert.NotNull(entity);
                Assert.True(entity!.IsDeleted);
            }
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrueIfEntityExists()
        {
            var existing = await _dbContext.Set<FakeEntity>().FirstAsync();
            var exists = await _repository.ExistsAsync(existing.Id);

            Assert.True(exists);
        }

        [Fact]
        public async Task FindByExpressionAsync_ReturnsMatchingEntities()
        {
            var result = await _repository.FindByExpressionAsync(e => e.Name.Contains("1"));
            Assert.Single(result);
            Assert.Contains(result, e => e.Name == "Entity 1");
        }
        [Fact]
        public async Task FindByExpressionAsync_WithIncludes_StillReturnsResults()
        {
            var result = await _repository.FindByExpressionAsync(
                e => true,
                includes: q => q);

            Assert.NotEmpty(result);
        }
        [Fact]
        public async Task FindByExpressionAsync_WithOrderBy_AppliesOrdering()
        {
            var result = await _repository.FindByExpressionAsync(
                e => true,
                orderBy: q => q.OrderByDescending(e => e.Id));

            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.True(list[0].Id > list[1].Id);
        }
        [Fact]
        public async Task FindByExpressionAsync_WithSkipAndTake_SlicesCorrectly()
        {
            var result = await _repository.FindByExpressionAsync(
                e => true,
                skip: 1,
                take: 1);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEntitiesWhereIsDeletedIsFalse()
        {
            var result = await _repository.GetAllAsync();

            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.All(list, e => Assert.False(e.IsDeleted));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsEntity_WhenEntityExists()
        {
            var existing = await _dbContext.Set<FakeEntity>().FirstOrDefaultAsync();
            var result = await _repository.GetByIdAsync(existing!.Id);

            Assert.NotNull(result);
            Assert.Equal(existing.Id, result!.Id);
        }

        [Fact]
        public async Task GetPaginatedListAsync_ReturnsCorrectItemsAndTotalCount()
        {
            var (items, totalCount) = await _repository.GetPaginatedListAsync(
                e => true,
                orderBy: q => q.OrderBy(e => e.Id),
                skip: 1,
                take: 1);

            Assert.Equal(2, totalCount);
            Assert.Single(items);
        }
        [Fact]
        public async Task GetPaginatedListAsync_ReturnsCorrectPageWithTotalCount()
        {
            Expression<Func<FakeEntity, bool>> predicate = e => !e.IsDeleted;
            Func<IQueryable<FakeEntity>, IOrderedQueryable<FakeEntity>> orderBy = q => q.OrderBy(e => e.Id);

            int take = 1;
            int skip = 1;

            var (items, totalCount) = await _repository.GetPaginatedListAsync(predicate, orderBy, null, take, skip);

            Assert.Equal(2, totalCount);
            Assert.Single(items);
            Assert.Equal(3, items.First().Id);
        }
        [Fact]
        public async Task GetPaginatedListAsync_WithIncludes_StillReturnsResults()
        {
            var (items, totalCount) = await _repository.GetPaginatedListAsync(
                e => true,
                includes: q => q);

            Assert.NotEmpty(items);
            Assert.Equal(2, totalCount);
        }
        
        [Fact]
        public async Task RestoreByIdAsync_DoesNothing_WhenEntityIsNotDeleted()
        {
            var notDeleted = await _dbContext.Set<FakeEntity>().FirstAsync(e => !e.IsDeleted);

            await _repository.RestoreByIdAsync(notDeleted.Id);

            var entity = await _dbContext.Set<FakeEntity>().FindAsync(notDeleted.Id);
            Assert.NotNull(entity);
            Assert.False(entity!.IsDeleted);
            Assert.Null(entity.UpdatedBy);
            Assert.Null(entity.UpdatedDate);
        }
        [Fact]
        public async Task RestoreByIdAsync_DoesNothing_WhenEntityIsNull()
        {
            int nonExistentId = 9999;

            await _repository.RestoreByIdAsync(nonExistentId);
            Assert.Null(await _dbContext.Set<FakeEntity>().FindAsync(nonExistentId));
        }
        [Fact]
        public async Task RestoreByIdAsync_SetsIsDeletedToFalse()
        {
            var deleted = await _dbContext.Set<FakeEntity>().FirstAsync(e => e.IsDeleted);
            await _repository.RestoreByIdAsync(deleted.Id);

            var restored = await _dbContext.Set<FakeEntity>().FindAsync(deleted.Id);
            Assert.False(restored!.IsDeleted);
        }

        [Fact]
        public async Task RestoreByIdsAsync_DoesNothing_WhenEntitiesAreNotDeleted()
        {
            var notDeletedIds = await _dbContext.Set<FakeEntity>()
                .Where(e => !e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync();

            await _repository.RestoreByIdsAsync(notDeletedIds);

            foreach (var id in notDeletedIds)
            {
                var entity = await _dbContext.Set<FakeEntity>().FindAsync(id);
                Assert.False(entity!.IsDeleted);
                Assert.NotNull(entity);
                Assert.Null(entity.UpdatedBy);
                Assert.Null(entity.UpdatedDate);
            }
        }
        [Fact]
        public async Task RestoreByIdsAsync_DoesNothing_WhenIdsExistButNotDeleted()
        {
            var idsNotDeleted = await _dbContext.Set<FakeEntity>()
                .Where(e => !e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync();

            await _repository.RestoreByIdsAsync(idsNotDeleted);

            Assert.NotEmpty(idsNotDeleted);
            Assert.NotNull(idsNotDeleted);

            foreach (var id in idsNotDeleted)
            {
                var entity = await _dbContext.Set<FakeEntity>().FindAsync(id);
                Assert.NotNull(entity);
                Assert.False(entity!.IsDeleted);
                Assert.Null(entity.UpdatedBy);
                Assert.Null(entity.UpdatedDate);
                Assert.Equal(id, entity.Id);
            }
        }
        [Fact]
        public async Task RestoreByIdsAsync_DoesNothing_WhenNoEntitiesAreDeleted()
        {
            var idsOfNotDeleted = await _dbContext.Set<FakeEntity>()
                .Where(e => !e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync();

            await _repository.RestoreByIdsAsync(idsOfNotDeleted);

            foreach (var id in idsOfNotDeleted)
            {
                var entity = await _dbContext.Set<FakeEntity>().FindAsync(id);
                Assert.False(entity!.IsDeleted);
                Assert.Null(entity.UpdatedBy);
                Assert.Null(entity.UpdatedDate);
            }
        }
        [Fact]
        public async Task RestoreByIdsAsync_SetsIsDeletedToFalseForAll()
        {
            var deletedIds = await _dbContext.Set<FakeEntity>()
                .Where(e => e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync();
            await _repository.RestoreByIdsAsync(deletedIds);

            foreach (var id in deletedIds)
            {
                var entity = await _dbContext.Set<FakeEntity>().FindAsync(id);
                Assert.False(entity!.IsDeleted);
            }
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingEntity()
        {
            var entity = await _dbContext.Set<FakeEntity>().FirstAsync();
            entity.Name = "Updated Name";

            await _repository.UpdateAsync(entity);

            var updated = await _dbContext.Set<FakeEntity>().FindAsync(entity.Id);
            Assert.Equal("Updated Name", updated!.Name);
        }
    }
}
