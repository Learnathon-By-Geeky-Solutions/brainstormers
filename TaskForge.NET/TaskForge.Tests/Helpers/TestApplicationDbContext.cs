using Microsoft.EntityFrameworkCore;
using TaskForge.Infrastructure.Data;
using TaskForge.Tests.TestData.Entities;

namespace TaskForge.Tests.Helpers
{
    public class TestApplicationDbContext : ApplicationDbContext
    {
        public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<FakeEntity> FakeEntities => Set<FakeEntity>();
    }
}
