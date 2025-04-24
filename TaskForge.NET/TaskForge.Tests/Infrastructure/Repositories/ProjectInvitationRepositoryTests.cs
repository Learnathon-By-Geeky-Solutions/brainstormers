using Microsoft.EntityFrameworkCore;
using Moq;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories;
using Xunit;

namespace TaskForge.Tests.Infrastructure.Repositories
{
    public class ProjectInvitationRepositoryTests
    {
        [Fact]
        public void Constructor_ShouldInitializeRepository()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("ProjectInvitationRepoTest")
                .Options;

            var context = new ApplicationDbContext(options);
            var userContextService = new Mock<IUserContextService>();

            // Act
            var repository = new ProjectInvitationRepository(context, userContextService.Object);

            // Assert
            Assert.NotNull(repository); // Forces constructor to run
        }
    }
}
