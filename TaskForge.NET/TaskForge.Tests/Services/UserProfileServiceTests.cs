using Moq;
using System.Linq.Expressions;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Services;
using TaskForge.Domain.Entities;
using Xunit;

namespace TaskForge.Tests.Services
{
    public class UserProfileServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserProfileRepository> _mockUserProfileRepo;
        private readonly UserProfileService _service;

        public UserProfileServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserProfileRepo = new Mock<IUserProfileRepository>();

            _mockUnitOfWork.Setup(u => u.UserProfiles).Returns(_mockUserProfileRepo.Object);

            _service = new UserProfileService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task CreateUserProfileAsync_AddsAndSavesProfile()
        {
            // Arrange
            var userId = "user-123";
            var fullName = "John Doe";

            // Act
            await _service.CreateUserProfileAsync(userId, fullName);

            // Assert
            _mockUserProfileRepo.Verify(r =>
                r.AddAsync(It.Is<UserProfile>(p =>
                    p.UserId == userId && p.FullName == fullName)),
                Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsId_WhenUserProfileExists()
        {
            // Arrange
            var userId = "user-456";
            var expectedId = 99;

            var userProfiles = new List<UserProfile>
            {
                new UserProfile { Id = expectedId, UserId = userId, FullName = "Test User" }
            };

            _mockUserProfileRepo
                .Setup(r => r.FindByExpressionAsync(It.IsAny<Expression<Func<UserProfile, bool>>>(), null, null, null, null))
                .ReturnsAsync(userProfiles);

            // Act
            var result = await _service.GetByUserIdAsync(userId);

            // Assert
            Assert.Equal(expectedId, result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsNull_WhenNoUserProfileExists()
        {
            // Arrange
            var userId = "nonexistent-user";

            _mockUserProfileRepo
                .Setup(r => r.FindByExpressionAsync(It.IsAny<Expression<Func<UserProfile, bool>>>(), null, null, null, null))
                .ReturnsAsync(new List<UserProfile>());

            // Act
            var result = await _service.GetByUserIdAsync(userId);

            // Assert
            Assert.Null(result);
        }
    }
}
