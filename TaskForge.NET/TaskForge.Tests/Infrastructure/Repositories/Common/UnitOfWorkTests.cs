using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories.Common;
using Xunit;

namespace TaskForge.Tests.Infrastructure.Repositories.Common
{
    public class UnitOfWorkTests
    {
        [Fact]
        public async Task BeginTransactionAsync_ShouldBeginTransaction()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            await using var context = new ApplicationDbContext(options);
            var unitOfWork = new UnitOfWork(context);

            // Act
            await using var transaction = await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

            // Assert
            Assert.NotNull(transaction);
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
            unitOfWork.Dispose(); // Call again to ensure no exception

            // Assert
            mockContext.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public async Task SaveChangesAsync_CallsContextSaveChangesAsync()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(42);
            var unitOfWork = new UnitOfWork(mockContext.Object);

            // Act
            var result = await unitOfWork.SaveChangesAsync();

            // Assert
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(42, result);
        }
    }
}
