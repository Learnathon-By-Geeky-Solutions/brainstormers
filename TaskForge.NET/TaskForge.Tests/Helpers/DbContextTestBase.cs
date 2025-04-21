using Microsoft.EntityFrameworkCore;
using TaskForge.Infrastructure.Data;

namespace TaskForge.Tests.Helpers
{
    public abstract class DbContextTestBase
    {
        protected static DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }
    }
}
