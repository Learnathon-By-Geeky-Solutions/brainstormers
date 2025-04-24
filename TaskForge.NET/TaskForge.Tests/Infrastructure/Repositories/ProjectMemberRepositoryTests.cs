using Microsoft.EntityFrameworkCore;
using Moq;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories;
using Xunit;
namespace TaskForge.Tests.Infrastructure.Repositories
{
    public class ProjectMemberRepositoryTests
    {
        [Fact]
        public void ShouldInitializeRepository()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("ProjectMemberRepoTest")
                .Options;

            var context = new ApplicationDbContext(options);
            var userContextService = new Mock<IUserContextService>();

            // Act
            var repository = new ProjectMemberRepository(context, userContextService.Object);

            // Assert
            Assert.NotNull(repository);
        }
    }
}
