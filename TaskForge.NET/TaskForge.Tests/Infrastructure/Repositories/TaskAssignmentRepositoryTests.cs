using Microsoft.EntityFrameworkCore;
using Moq;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories;
using Xunit;
namespace TaskForge.Tests.Infrastructure.Repositories
{
    public class TaskAssignmentRepositoryTests
    {
        [Fact]
        public void Constructor_ShouldInitializeRepository()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TaskAssignmentRepoTest")
                .Options;

            var context = new ApplicationDbContext(options);
            var userContextService = new Mock<IUserContextService>();

            // Act
            var repository = new TaskAssignmentRepository(context, userContextService.Object);

            // Assert
            Assert.NotNull(repository);
        }
    }
}
