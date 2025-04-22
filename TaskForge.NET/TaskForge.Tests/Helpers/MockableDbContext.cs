using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using TaskForge.Infrastructure.Data;

namespace TaskForge.Tests.Helpers
{
    public class MockableDbContext : TestApplicationDbContext
    {
        private readonly DatabaseFacade _mockedDatabase;

        public MockableDbContext(DbContextOptions<ApplicationDbContext> options, DatabaseFacade mockedDatabase)
            : base(options)
        {
            _mockedDatabase = mockedDatabase;
        }

        public override DatabaseFacade Database => _mockedDatabase;
    }
}
