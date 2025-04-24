using Microsoft.EntityFrameworkCore;
using Moq;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories;
using Xunit;
namespace TaskForge.Tests.Infrastructure.Repositories
{
    public class TaskDependencyRepositoryTests
    {
        [Fact]
        public void ShouldInitializeRepository()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TaskDependencyRepoTest")
                .Options;

            var context = new ApplicationDbContext(options);
            var userContextService = new Mock<IUserContextService>();

            // Act
            var repository = new TaskDependencyRepository(context, userContextService.Object);

            // Assert
            Assert.NotNull(repository);
        }
    }
}
