using Microsoft.AspNetCore.Identity;
using Moq;
using System.Linq.Expressions;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Application.Services
{
    public class ProjectMemberServiceTests
    {
	    private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
		private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ProjectMemberService _projectMemberService;

        public ProjectMemberServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
			_projectMemberService = new ProjectMemberService(_projectMemberRepositoryMock.Object,_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProjectMember_WhenValidIdProvided()
        {
            // Arrange
            var memberId = 1;
            var expectedMember = new ProjectMember { Id = memberId, IsDeleted = false };

            _projectMemberRepositoryMock.Setup(u => u.GetByIdAsync(memberId))
                .ReturnsAsync(expectedMember);

            // Act
            var result = await _projectMemberService.GetByIdAsync(memberId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(memberId, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
        {
            // Arrange
            var invalidId = 999;

            _projectMemberRepositoryMock.Setup(u => u.GetByIdAsync(invalidId))
                .ReturnsAsync((ProjectMember?)null);

            // Act
            var result = await _projectMemberService.GetByIdAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenMemberIsDeleted()
        {
            // Arrange
            var memberId = 2;

            _projectMemberRepositoryMock.Setup(u => u.GetByIdAsync(memberId))
                .ReturnsAsync((ProjectMember?)null); // Simulating soft-delete logic

            // Act
            var result = await _projectMemberService.GetByIdAsync(memberId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var memberId = 3;

            _projectMemberRepositoryMock.Setup(u => u.GetByIdAsync(memberId))
                .ThrowsAsync(new Exception("Database failure"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _projectMemberService.GetByIdAsync(memberId));
            Assert.Equal("Database failure", ex.Message);
        }


        [Fact]
        public async Task GetUserProjectRoleAsync_ReturnsDtoWithUserAndProfile_WhenUserExistsInProject()
        {
            // Arrange
            var userId = "user-1";
            var projectId = 10;

            var identityUser = new IdentityUser
            {
                Id = userId,
                UserName = "testuser@example.com"
            };

            var userProfile = new UserProfile
            {
                Id = 2,
                UserId = userId,
                FullName = "Test User",
                User = identityUser
            };

            var projectMember = new ProjectMember
            {
                Id = 3,
                ProjectId = projectId,
                UserProfileId = userProfile.Id,
                UserProfile = userProfile,
                Role = ProjectRole.Admin
            };

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                    null, null))
                .ReturnsAsync(new List<ProjectMember> { projectMember });

            // Act
            var result = await _projectMemberService.GetUserProjectRoleAsync(userId, projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(projectMember.Id, result.Id);
            Assert.Equal("Test User", result.Name);
            Assert.Equal("testuser@example.com", result.Email);
            Assert.Equal(ProjectRole.Admin, result.Role);
        }

        [Fact]
        public async Task GetUserProjectRoleAsync_ShouldReturnNull_WhenNoMatchingMemberFound()
        {
            // Arrange
            string userId = "nonexistent-user";
            int projectId = 123;

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                    null,
                    null))
                .ReturnsAsync(new List<ProjectMember>());

            // Act
            var result = await _projectMemberService.GetUserProjectRoleAsync(userId, projectId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserProjectRoleAsync_ShouldUseDefaultNameAndEmail_WhenFieldsAreNull()
        {
            // Arrange
            string userId = "user-1";
            int projectId = 1;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var member = new ProjectMember
            {
                Id = 1,
                Role = ProjectRole.Viewer,
                UserProfile = new UserProfile
                {
                    FullName = null,
                    User = new IdentityUser { UserName = null }
                }
            };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                    null,
                    null))
                .ReturnsAsync(new List<ProjectMember> { member });

            // Act
            var result = await _projectMemberService.GetUserProjectRoleAsync(userId, projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Unknown User", result.Name);
            Assert.Equal("No Email", result.Email);
        }

        [Fact]
        public async Task GetUserProjectRoleAsync_ShouldPropagateException_WhenRepositoryThrows()
        {
            // Arrange
            string userId = "user-1";
            int projectId = 1;

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                    null,
                    null))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _projectMemberService.GetUserProjectRoleAsync(userId, projectId));
        }

        [Fact]
        public async Task GetUserProjectRoleAsync_ShouldMatchPredicateProperly()
        {
            // Arrange
            string userId = "user-123";
            int projectId = 999;

            var matchingMember = new ProjectMember
            {
                Id = 1,
                ProjectId = projectId,
                Role = ProjectRole.Admin,
                UserProfile = new UserProfile
                {
                    UserId = userId,
                    FullName = "Exact Match",
                    User = new IdentityUser { UserName = "match@example.com" }
                }
            };

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.Is<Expression<Func<ProjectMember, bool>>>(predicate =>
                        predicate.Compile()(matchingMember)),
                    null,
                    It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                    null,
                    null))
                .ReturnsAsync(new List<ProjectMember> { matchingMember });

            // Act
            var result = await _projectMemberService.GetUserProjectRoleAsync(userId, projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Exact Match", result.Name);
            Assert.Equal("match@example.com", result.Email);
            Assert.Equal(ProjectRole.Admin, result.Role);
        }


        [Fact]
        public async Task GetProjectMembersAsync_ShouldReturnMappedDtos_WhenMembersExist()
        {
            // Arrange
            int projectId = 1;

            var identityUser = new IdentityUser { Id = "user-1", UserName = "testuser" };
            var userProfile = new UserProfile { Id = 2, FullName = "Test User", User = identityUser };
            var member = new ProjectMember
            {
                Id = 3,
                ProjectId = projectId,
                UserProfileId = userProfile.Id,
                UserProfile = userProfile,
                Role = ProjectRole.Contributor
            };

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                    null,
                    null))
                .ReturnsAsync(new List<ProjectMember> { member });

            // Act
            var result = await _projectMemberService.GetProjectMembersAsync(projectId);

            // Assert
            Assert.Single(result);
            var dto = result.FirstOrDefault();
            Assert.Equal(member.Id, dto?.Id);
            Assert.Equal(member.ProjectId, dto?.ProjectId);
            Assert.Equal(member.UserProfileId, dto?.UserProfileId);
            Assert.Equal("Test User", dto?.Name);
            Assert.Equal("testuser", dto?.Email);
            Assert.Equal(ProjectRole.Contributor, dto?.Role);
        }

        [Fact]
        public async Task GetProjectMembersAsync_ShouldReturnEmptyList_WhenNoMembersExist()
        {
            // Arrange
            int projectId = 999;

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                    null,
                    null))
                .ReturnsAsync(new List<ProjectMember>());

            // Act
            var result = await _projectMemberService.GetProjectMembersAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProjectMembersAsync_ShouldReturnDefaultValues_WhenFieldsAreNull()
        {
            // Arrange
            int projectId = 1;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var member = new ProjectMember
            {
                Id = 10,
                ProjectId = projectId,
                UserProfileId = 5,
                Role = ProjectRole.Viewer,
                UserProfile = new UserProfile
                {
                    FullName = null,
                    User = new IdentityUser { UserName = null }
                }
            };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                    null,
                    null))
                .ReturnsAsync(new List<ProjectMember> { member });

            // Act
            var result = await _projectMemberService.GetProjectMembersAsync(projectId);

            // Assert
            var dto = result.FirstOrDefault();
            Assert.Equal("Unknown User", dto?.Name);
            Assert.Equal("No Email", dto?.Email);
        }

        [Fact]
        public async Task GetProjectMembersAsync_ShouldOrderMembersByRole()
        {
            // Arrange
            int projectId = 1;

            var members = new List<ProjectMember>
            {
                new ProjectMember
                {
                    Id = 1,
                    ProjectId = projectId,
                    Role = ProjectRole.Contributor,
                    UserProfile = new UserProfile { FullName = "Alice", User = new IdentityUser { UserName = "alice@example.com" } }
                },
                new ProjectMember
                {
                    Id = 2,
                    ProjectId = projectId,
                    Role = ProjectRole.Admin,
                    UserProfile = new UserProfile { FullName = "Bob", User = new IdentityUser { UserName = "bob@example.com" } }
                }
            };

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                    null,
                    null))
                .ReturnsAsync(members.OrderBy(m => m.Role).ToList());

            // Act
            var result = await _projectMemberService.GetProjectMembersAsync(projectId);

            // Assert
            var firstMember = result.FirstOrDefault();
            Assert.NotNull(firstMember);
            Assert.Equal(2, result.Count);
            Assert.Equal(ProjectRole.Admin, firstMember.Role);

        }

        [Fact]
        public async Task GetProjectMembersAsync_ShouldPropagateException_WhenRepositoryFails()
        {
            // Arrange
            int projectId = 1;

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                    null,
                    null))
                .ThrowsAsync(new Exception("Database failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _projectMemberService.GetProjectMembersAsync(projectId));
        }


        [Fact]
        public async Task RemoveAsync_ShouldMarkProjectMemberAsDeleted_WhenMemberExists()
        {
            // Arrange
            var memberId = 1;
            var mockMember = new ProjectMember { Id = memberId, IsDeleted = false };

            _projectMemberRepositoryMock.Setup(u => u.DeleteByIdAsync(memberId))
                .Returns(async () =>
                {
                    mockMember.IsDeleted = true;
                    mockMember.UpdatedBy = "mock-user";
                    mockMember.UpdatedDate = DateTime.UtcNow;
                    await Task.CompletedTask;
                });

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _projectMemberService.RemoveAsync(memberId);

            // Assert
            Assert.True(mockMember.IsDeleted);
            _projectMemberRepositoryMock.Verify(u => u.DeleteByIdAsync(memberId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_ShouldNotThrow_WhenMemberDoesNotExist()
        {
            // Arrange
            var memberId = 99;

            _projectMemberRepositoryMock.Setup(u => u.DeleteByIdAsync(memberId))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _projectMemberService.RemoveAsync(memberId));
            Assert.Null(exception);
            _projectMemberRepositoryMock.Verify(u => u.DeleteByIdAsync(memberId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_ShouldPropagateException_WhenDeleteByIdFails()
        {
            // Arrange
            var memberId = 1;

            _projectMemberRepositoryMock.Setup(u => u.DeleteByIdAsync(memberId))
                .ThrowsAsync(new Exception("Deletion failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _projectMemberService.RemoveAsync(memberId));
            _projectMemberRepositoryMock.Verify(u => u.DeleteByIdAsync(memberId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task RemoveAsync_ShouldPropagateException_WhenSaveChangesFails()
        {
            // Arrange
            var memberId = 1;

            _projectMemberRepositoryMock.Setup(u => u.DeleteByIdAsync(memberId))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new Exception("DB save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _projectMemberService.RemoveAsync(memberId));
            _projectMemberRepositoryMock.Verify(u => u.DeleteByIdAsync(memberId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task GetUserProjectCountAsync_ShouldReturnCorrectCount_WhenUserHasProjects()
        {
            // Arrange
            int userProfileId = 5;
            var mockData = new List<ProjectMember>
            {
                new ProjectMember { ProjectId = 1, UserProfileId = userProfileId },
                new ProjectMember { ProjectId = 2, UserProfileId = userProfileId },
                new ProjectMember { ProjectId = 1, UserProfileId = userProfileId } // duplicate project
            };

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                null, null, null, null))
                .ReturnsAsync(mockData);

            // Act
            var result = await _projectMemberService.GetUserProjectCountAsync(userProfileId);

            // Assert
            Assert.Equal(2, result); // Distinct projects: 1 and 2
        }

        [Fact]
        public async Task GetUserProjectCountAsync_ShouldReturnZero_WhenUserHasNoProjects()
        {
            // Arrange
            int userProfileId = 10;
            var mockData = new List<ProjectMember>(); // empty list

            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                null, null, null, null))
                .ReturnsAsync(mockData);

            // Act
            var result = await _projectMemberService.GetUserProjectCountAsync(userProfileId);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetUserProjectCountAsync_ShouldPropagateException_WhenRepositoryThrows()
        {
            // Arrange
            _projectMemberRepositoryMock.Setup(u => u.FindByExpressionAsync(
                It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                null, null, null, null))
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _projectMemberService.GetUserProjectCountAsync(1));
        }

    }
}
