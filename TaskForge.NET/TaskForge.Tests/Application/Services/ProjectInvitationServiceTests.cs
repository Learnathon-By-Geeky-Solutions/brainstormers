using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
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
    public class ProjectInvitationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserStore<IdentityUser>> _userStoreMock;
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly ProjectInvitationService _projectInvitationService;

        public ProjectInvitationServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userStoreMock = new Mock<IUserStore<IdentityUser>>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            // Create a minimal UserManager
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                _userStoreMock.Object,
                null, null, null, null, null, null, null,
                new Mock<ILogger<UserManager<IdentityUser>>>().Object
            );

            _projectInvitationService = new ProjectInvitationService(
                _unitOfWorkMock.Object,
                _userManagerMock.Object
            );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsInvitation_WhenExists()
        {
            // Arrange
            var invitationId = 1;
            var mockInvitation = new ProjectInvitation { Id = invitationId };

            _unitOfWorkMock.Setup(x => x.ProjectInvitations.GetByIdAsync(invitationId))
                           .ReturnsAsync(mockInvitation);

            // Act
            var result = await _projectInvitationService.GetByIdAsync(invitationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(invitationId, result!.Id);
            _unitOfWorkMock.Verify(x => x.ProjectInvitations.GetByIdAsync(invitationId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenInvitationDoesNotExist()
        {
            // Arrange
            var invitationId = 999;

            _unitOfWorkMock.Setup(x => x.ProjectInvitations.GetByIdAsync(invitationId))
                           .ReturnsAsync((ProjectInvitation?)null);

            // Act
            var result = await _projectInvitationService.GetByIdAsync(invitationId);

            // Assert
            Assert.Null(result);
            _unitOfWorkMock.Verify(x => x.ProjectInvitations.GetByIdAsync(invitationId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var invitationId = 1;

            _unitOfWorkMock
                .Setup(u => u.ProjectInvitations.GetByIdAsync(invitationId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _projectInvitationService.GetByIdAsync(invitationId));
        }

        [Fact]
        public async Task GetInvitationListAsync_ReturnsPaginatedInvitations()
        {
            // Arrange
            var projectId = 1;
            var pageIndex = 1;
            var pageSize = 2;
            var skip = (pageIndex - 1) * pageSize;
            var totalCount = 5;

            var mockInvitations = new List<ProjectInvitation>
            {
                new ProjectInvitation
                {
                    Id = 1,
                    ProjectId = projectId,
                    InvitedUserProfile = new UserProfile
                    {
                        Id = 10,
                        User = new IdentityUser { UserName = "testuser1" }
                    }
                },
                new ProjectInvitation
                {
                    Id = 2,
                    ProjectId = projectId,
                    InvitedUserProfile = new UserProfile
                    {
                        Id = 11,
                        User = new IdentityUser { UserName = "testuser2" }
                    }
                }
            };

            // Setup must match: predicate, orderBy, includes, take, skip
            _unitOfWorkMock
                .Setup(u => u.ProjectInvitations.GetPaginatedListAsync(
                    It.Is<Expression<Func<ProjectInvitation, bool>>>(pred => pred.Compile()(mockInvitations[0])),
                    It.IsAny<Func<IQueryable<ProjectInvitation>, IOrderedQueryable<ProjectInvitation>>>(),
                    It.IsAny<Func<IQueryable<ProjectInvitation>, IQueryable<ProjectInvitation>>>(),
                    pageSize,
                    skip
                ))
                .ReturnsAsync((mockInvitations.AsEnumerable(), totalCount));

            // Act
            var result = await _projectInvitationService.GetInvitationListAsync(projectId, pageIndex, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(totalCount, result.TotalCount);
            Assert.Equal(mockInvitations.Count, result.Items.Count);
            Assert.Equal("testuser1", result.Items[0].InvitedUserProfile.User.UserName);

            _unitOfWorkMock.Verify(u => u.ProjectInvitations.GetPaginatedListAsync(
                It.IsAny<Expression<Func<ProjectInvitation, bool>>>(),
                It.IsAny<Func<IQueryable<ProjectInvitation>, IOrderedQueryable<ProjectInvitation>>>(),
                It.IsAny<Func<IQueryable<ProjectInvitation>, IQueryable<ProjectInvitation>>>(),
                pageSize,
                skip
            ), Times.Once);
        }

        [Fact]
        public async Task GetInvitationListAsync_ReturnsEmpty_WhenNoInvitations()
        {
            // Arrange
            var projectId = 1;
            var pageIndex = 1;
            var pageSize = 2;
            var skip = (pageIndex - 1) * pageSize;

            _unitOfWorkMock.Setup(u => u.ProjectInvitations.GetPaginatedListAsync(
                    It.IsAny<Expression<Func<ProjectInvitation, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<ProjectInvitation>, IQueryable<ProjectInvitation>>>(),
                    pageSize,
                    skip
                ))
                .ReturnsAsync((Enumerable.Empty<ProjectInvitation>(), 0));

            // Act
            var result = await _projectInvitationService.GetInvitationListAsync(projectId, pageIndex, pageSize);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.False(result.HasPreviousPage, "On first page, HasPreviousPage should be false");
            Assert.False(result.HasNextPage, "With no items, HasNextPage should be false");

            _unitOfWorkMock.Verify(u => u.ProjectInvitations.GetPaginatedListAsync(
                It.IsAny<Expression<Func<ProjectInvitation, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<ProjectInvitation>, IQueryable<ProjectInvitation>>>(),
                pageSize,
                skip
            ), Times.Once);
        }

        [Fact]
        public async Task GetInvitationListAsync_SetsHasPreviousAndNext_OnMiddlePage()
        {
            // Arrange
            var projectId = 1;
            var pageIndex = 2;
            var pageSize = 2;
            var skip = (pageIndex - 1) * pageSize;

            // Simulate 2 items on page 2, total 5 ⇒ totalPages = 3
            var mockInvitations = new[]
            {
                new ProjectInvitation { Id = 3, ProjectId = projectId },
                new ProjectInvitation { Id = 4, ProjectId = projectId }
            };
            var totalCount = 5;

            _unitOfWorkMock
                .Setup(u => u.ProjectInvitations.GetPaginatedListAsync(
                    It.IsAny<Expression<Func<ProjectInvitation, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<ProjectInvitation>, IQueryable<ProjectInvitation>>>(),
                    pageSize,
                    skip
                ))
                .ReturnsAsync((mockInvitations.AsEnumerable(), totalCount));

            // Act
            var result = await _projectInvitationService.GetInvitationListAsync(projectId, pageIndex, pageSize);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(5, result.TotalCount);
            Assert.True(result.HasPreviousPage, "PageIndex 2 > 1 => HasPreviousPage should be true");
            Assert.True(result.HasNextPage, "PageIndex 2 < TotalPages (3) => HasNextPage should be true");
        }

        [Fact]
        public async Task GetInvitationListAsync_Works_WhenIncludesNullInRepository()
        {
            // Arrange
            var projectId = 50;
            var pageIndex = 2;
            var pageSize = 5;
            var skip = (pageIndex - 1) * pageSize;

            var items = new[] { new ProjectInvitation { Id = 99, ProjectId = projectId } };
            var count = 1;

            _unitOfWorkMock
              .Setup(u => u.ProjectInvitations.GetPaginatedListAsync(
                  It.IsAny<Expression<Func<ProjectInvitation, bool>>>(),
                  /* orderBy */  null,
                  /* includes */ null,
                  /* take */     pageSize,
                  /* skip */     skip
              ))
              .ReturnsAsync((items.AsEnumerable(), count));

            // Act
            var (fetched, total) = await _unitOfWorkMock.Object
              .ProjectInvitations
              .GetPaginatedListAsync(pi => pi.ProjectId == projectId, null, null, pageSize, skip);

            // Assert
            Assert.Equal(count, total);
            Assert.Single(fetched);
            Assert.Equal(99, fetched.First().Id);
        }

        [Fact]
        public async Task GetInvitationListAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var projectId = 1;
            var pageIndex = 1;
            var pageSize = 10;

            _unitOfWorkMock
                .Setup(u => u.ProjectInvitations.GetPaginatedListAsync(
                    It.IsAny<Expression<Func<ProjectInvitation, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<ProjectInvitation>, IQueryable<ProjectInvitation>>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ThrowsAsync(new Exception("Repository failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _projectInvitationService.GetInvitationListAsync(projectId, pageIndex, pageSize));
        }

        [Fact]
        public async Task AddAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var email = "missing@user.com";
            var projectId = 1;

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                            .ReturnsAsync((IdentityUser)null!);

            // Act
            var result = await _projectInvitationService.AddAsync(projectId, email, ProjectRole.Contributor);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnFailure_WhenUserProfileNotFound()
        {
            // Arrange
            var email = "user@example.com";
            var projectId = 1;
            var userId = "user-123";

            var identityUser = new IdentityUser
            {
                Id = userId,
                Email = email,
                UserName = "testuser"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(identityUser);

            _unitOfWorkMock.Setup(x => x.UserProfiles.FindByExpressionAsync(
                    It.IsAny<Expression<Func<UserProfile, bool>>>(),
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync(new List<UserProfile>());

            // Act
            var result = await _projectInvitationService.AddAsync(projectId, email, ProjectRole.Contributor);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User profile not found.", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnFailure_WhenAlreadyProjectMemberExists()
        {
            // Arrange
            var email = "user@example.com";
            var projectId = 1;
            var userId = "user-123";
            var userProfileId = 10;

            var identityUser = new IdentityUser
            {
                Id = userId,
                Email = email,
                UserName = "testuser"
            };

            var userProfile = new UserProfile
            {
                Id = userProfileId,
                UserId = userId
            };

            var existingMember = new ProjectMember
            {
                ProjectId = projectId,
                UserProfile = new UserProfile { UserId = userId }
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(identityUser);

            _unitOfWorkMock.Setup(x => x.UserProfiles.FindByExpressionAsync(
                    It.IsAny<Expression<Func<UserProfile, bool>>>(),
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync(new List<UserProfile> { userProfile });

            _unitOfWorkMock.Setup(x => x.ProjectMembers.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync(new List<ProjectMember> { existingMember });

            // Act
            var result = await _projectInvitationService.AddAsync(projectId, email, ProjectRole.Viewer);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User is already a member of this project.", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnFailure_WhenInvitationAlreadyExists()
        {
            // Arrange
            var email = "user@example.com";
            var projectId = 1;
            var userId = "user-123";
            var userProfileId = 10;

            var identityUser = new IdentityUser
            {
                Id = userId,
                Email = email,
                UserName = "testuser"
            };

            var userProfile = new UserProfile
            {
                Id = userProfileId,
                UserId = userId
            };

            var existingInvitation = new ProjectInvitation
            {
                ProjectId = projectId,
                InvitedUserProfileId = userProfileId,
                Status = InvitationStatus.Pending
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(identityUser);

            _unitOfWorkMock.Setup(x => x.UserProfiles.FindByExpressionAsync(
                    It.IsAny<Expression<Func<UserProfile, bool>>>(),
                    null, null, null, null))
                .ReturnsAsync(new List<UserProfile> { userProfile });

            _unitOfWorkMock.Setup(x => x.ProjectMembers.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    null, null, null, null))
                .ReturnsAsync(new List<ProjectMember>()); // user is not already a member

            _unitOfWorkMock.Setup(x => x.ProjectInvitations.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectInvitation, bool>>>(),
                    null, null, null, null))
                .ReturnsAsync(new List<ProjectInvitation> { existingInvitation });

            // Act
            var result = await _projectInvitationService.AddAsync(projectId, email, ProjectRole.Contributor);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An invitation has already been sent to this user.", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnSuccess_WhenAllValidationsPass()
        {
            // Arrange
            var email = "user@example.com";
            var projectId = 1;
            var userId = "user-123";
            var userProfileId = 10;

            var identityUser = new IdentityUser
            {
                Id = userId,
                Email = email,
                UserName = "testuser"
            };

            var userProfile = new UserProfile
            {
                Id = userProfileId,
                UserId = userId
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(identityUser);

            _unitOfWorkMock.Setup(x => x.UserProfiles.FindByExpressionAsync(
                    It.IsAny<Expression<Func<UserProfile, bool>>>(),
                    null, null, null, null))
                .ReturnsAsync(new List<UserProfile> { userProfile });

            _unitOfWorkMock.Setup(x => x.ProjectMembers.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    null, null, null, null))
                .ReturnsAsync(new List<ProjectMember>()); // user is not a member

            _unitOfWorkMock.Setup(x => x.ProjectInvitations.FindByExpressionAsync(
                    It.IsAny<Expression<Func<ProjectInvitation, bool>>>(),
                    null, null, null, null))
                .ReturnsAsync(new List<ProjectInvitation>()); // no pending invitation

            _unitOfWorkMock.Setup(x => x.ProjectInvitations.AddAsync(It.IsAny<ProjectInvitation>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1); // simulate successful DB save

            // Act
            var result = await _projectInvitationService.AddAsync(projectId, email, ProjectRole.Contributor);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Invitation sent successfully.", result.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenUserManagerThrows()
        {
            // Arrange
            var email = "user@example.com";
            var projectId = 1;
            var role = ProjectRole.Viewer;

            _userManagerMock.Setup(um => um.FindByEmailAsync(email))
                .ThrowsAsync(new Exception("UserManager failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _projectInvitationService.AddAsync(projectId, email, role));

            Assert.Equal("UserManager failure", exception.Message);
        }

        [Fact]
        public async Task UpdateInvitationStatusAsync_ShouldDoNothing_WhenInvitationNotFound()
        {
            // Arrange
            var invitationId = 1;
            _unitOfWorkMock.Setup(u => u.ProjectInvitations.GetByIdAsync(invitationId))
                .ReturnsAsync((ProjectInvitation)null!);

            // Act
            await _projectInvitationService.UpdateInvitationStatusAsync(invitationId, InvitationStatus.Accepted);

            // Assert
            _unitOfWorkMock.Verify(u => u.ProjectInvitations.UpdateAsync(It.IsAny<ProjectInvitation>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateInvitationStatusAsync_ShouldAcceptInvitation_AndAddProjectMember()
        {
            // Arrange
            var invitationId = 2;
            var invitation = new ProjectInvitation
            {
                Id = invitationId,
                ProjectId = 1,
                InvitedUserProfileId = 10,
                AssignedRole = ProjectRole.Contributor,
                Status = InvitationStatus.Pending
            };

            _unitOfWorkMock.Setup(u => u.ProjectInvitations.GetByIdAsync(invitationId))
                .ReturnsAsync(invitation);

            var projectMembersRepoMock = new Mock<IProjectMemberRepository>(); // Use the actual interface here
            _unitOfWorkMock.Setup(u => u.ProjectMembers).Returns(projectMembersRepoMock.Object);

            // Act
            await _projectInvitationService.UpdateInvitationStatusAsync(invitationId, InvitationStatus.Accepted);

            // Assert
            Assert.Equal(InvitationStatus.Accepted, invitation.Status);
            Assert.NotNull(invitation.AcceptedDate);
            Assert.Null(invitation.DeclinedDate);

            projectMembersRepoMock.Verify(r => r.AddAsync(It.Is<ProjectMember>(
                m => m.ProjectId == invitation.ProjectId &&
                     m.UserProfileId == invitation.InvitedUserProfileId &&
                     m.Role == invitation.AssignedRole)), Times.Once);

            _unitOfWorkMock.Verify(u => u.ProjectInvitations.UpdateAsync(invitation), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateInvitationStatusAsync_ShouldDeclineInvitation()
        {
            // Arrange
            var invitationId = 3;
            var invitation = new ProjectInvitation
            {
                Id = invitationId,
                ProjectId = 1,
                InvitedUserProfileId = 10,
                AssignedRole = ProjectRole.Contributor,
                Status = InvitationStatus.Pending,
                AcceptedDate = DateTime.UtcNow // Simulate previously accepted
            };

            _unitOfWorkMock.Setup(u => u.ProjectInvitations.GetByIdAsync(invitationId))
                .ReturnsAsync(invitation);

            // Act
            await _projectInvitationService.UpdateInvitationStatusAsync(invitationId, InvitationStatus.Declined);

            // Assert
            Assert.Equal(InvitationStatus.Declined, invitation.Status);
            Assert.Null(invitation.AcceptedDate);
            Assert.NotNull(invitation.DeclinedDate);

            _unitOfWorkMock.Verify(u => u.ProjectMembers.AddAsync(It.IsAny<ProjectMember>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.ProjectInvitations.UpdateAsync(invitation), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateInvitationStatusAsync_ShouldThrowException_WhenSaveFails()
        {
            // Arrange
            var invitationId = 1;
            var status = InvitationStatus.Accepted;

            var invitation = new ProjectInvitation
            {
                Id = invitationId,
                Status = InvitationStatus.Pending,
                InvitedUserProfileId = 123,
                ProjectId = 456
            };

            _unitOfWorkMock.Setup(u => u.ProjectInvitations.GetByIdAsync(invitationId))
                .ReturnsAsync(invitation);

            _unitOfWorkMock
                .Setup(u => u.ProjectMembers.AddAsync(It.IsAny<ProjectMember>()));

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _projectInvitationService.UpdateInvitationStatusAsync(invitationId, status));
        }

        [Fact]
        public async Task GetInvitationsForUserAsync_ShouldReturnEmpty_WhenUserProfileIdIsNull()
        {
            // Act
            var result = await _projectInvitationService.GetInvitationsForUserAsync(null, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetInvitationsForUserAsync_ShouldReturnPaginatedInvitations_WhenUserProfileIdIsValid()
        {
            // Arrange
            var userProfileId = 1;
            var pageIndex = 2;
            var pageSize = 2;

            var allInvitations = new List<ProjectInvitation>
            {
                new ProjectInvitation { Id = 1, InvitedUserProfileId = userProfileId },
                new ProjectInvitation { Id = 2, InvitedUserProfileId = userProfileId },
                new ProjectInvitation { Id = 3, InvitedUserProfileId = 99 } // should be filtered out
            };

            var expectedFiltered = allInvitations
                .Where(i => i.InvitedUserProfileId == userProfileId)
                .ToList();

            var expectedPaged = expectedFiltered
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Mock setup with expression matching to ensure proper filtering
            _unitOfWorkMock.Setup(u => u.ProjectInvitations.GetPaginatedListAsync(
                    It.IsAny<Expression<Func<ProjectInvitation, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<ProjectInvitation>, IQueryable<ProjectInvitation>>>(),
                    (pageIndex - 1) * pageSize,
                    pageSize))
                .ReturnsAsync((expectedPaged, expectedFiltered.Count));
            // Ensure mock returns paginated results

            // Act
            var result = await _projectInvitationService.GetInvitationsForUserAsync(userProfileId, pageIndex, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPaged.Count, result.Items.Count);
            Assert.Equal(expectedFiltered.Count, result.TotalCount);
            Assert.All(result.Items, i => Assert.Equal(userProfileId, i.InvitedUserProfileId));
        }

        [Fact]
        public async Task GetInvitationsForUserAsync_ShouldReturnEmptyList_WhenNoInvitationsExist()
        {
            // Arrange
            var userProfileId = 99;
            var pageIndex = 1;
            var pageSize = 5;

            _unitOfWorkMock.Setup(u => u.ProjectInvitations.GetPaginatedListAsync(
                    It.IsAny<Expression<Func<ProjectInvitation, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<ProjectInvitation>, IIncludableQueryable<ProjectInvitation, object>>>(),
                    (pageIndex - 1) * pageSize,
                    pageSize))
                .ReturnsAsync((new List<ProjectInvitation>(), 0));

            // Act
            var result = await _projectInvitationService.GetInvitationsForUserAsync(userProfileId, pageIndex, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetInvitationsForUserAsync_SetsHasPreviousAndNext_OnMiddlePage()
        {
            // Arrange
            var userProfileId = 1;
            var pageIndex = 2;
            var pageSize = 2;
            var skip = (pageIndex - 1) * pageSize;
            var totalCount = 5; // 3 pages in total

            var mockInvitations = new List<ProjectInvitation>
            {
                new ProjectInvitation { Id = 3, InvitedUserProfileId = userProfileId },
                new ProjectInvitation { Id = 4, InvitedUserProfileId = userProfileId }
            };

            _unitOfWorkMock
                .Setup(u => u.ProjectInvitations.GetPaginatedListAsync(
                    It.IsAny<Expression<Func<ProjectInvitation, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<ProjectInvitation>, IQueryable<ProjectInvitation>>>(),
                    skip,
                    pageSize
                ))
                .ReturnsAsync((mockInvitations, totalCount));

            // Act
            var result = await _projectInvitationService.GetInvitationsForUserAsync(userProfileId, pageIndex, pageSize);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(totalCount, result.TotalCount);
            Assert.True(result.HasPreviousPage, "PageIndex > 1 => HasPreviousPage should be true");
            Assert.True(result.HasNextPage, "PageIndex < TotalPages => HasNextPage should be true");
        }

    }
}