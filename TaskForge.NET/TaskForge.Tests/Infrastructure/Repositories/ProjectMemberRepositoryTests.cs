using Microsoft.EntityFrameworkCore;
using Moq;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories;
using Xunit;

namespace TaskForge.Tests.Infrastructure.Repositories
{
    public class ProjectMemberRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IUserContextService> _userContextService;
        private bool _disposed;

        public ProjectMemberRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("ProjectMemberRepoTest")
                .Options;

            _context = new ApplicationDbContext(options);
            _userContextService = new Mock<IUserContextService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeRepository()
        {
            // Act
            var repository = new ProjectMemberRepository(_context, _userContextService.Object);

            // Assert
            Assert.NotNull(repository);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
