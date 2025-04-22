using Microsoft.EntityFrameworkCore;
using Moq;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories.Common;
using TaskForge.Tests.Helpers;
using Xunit;

namespace TaskForge.Tests.Infrastructure.Repositories.Common
{
    public class UnitOfWorkTests
    {
        private readonly TestApplicationDbContext _realContext;
        private readonly UnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _realContext = new TestApplicationDbContext(options);

            _unitOfWork = new UnitOfWork(_realContext);
        }

        [Fact]
        public void Constructor_InitializesRepositories()
        {
        }

        [Fact]
        public void Dispose_CallsContextDispose()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var unitOfWork = new UnitOfWork(mockContext.Object);

            // Act
            unitOfWork.Dispose();

            // Assert
            mockContext.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimesSafely()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var unitOfWork = new UnitOfWork(mockContext.Object);

            // Act
            unitOfWork.Dispose();
            unitOfWork.Dispose(); // Call again to check for exceptions

            // Assert
            mockContext.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public async Task SaveChangesAsync_CallsContextSaveChangesAsync()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(42);
            var unitOfWork = new UnitOfWork(mockContext.Object);

            // Act
            var result = await unitOfWork.SaveChangesAsync();

            // Assert
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
            Assert.Equal(42, result);
        }
    }
}
