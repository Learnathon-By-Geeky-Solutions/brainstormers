using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System.Data;
using System.Linq.Expressions;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Application.Services
{
    public class ProjectServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserProfileService> _userProfileServiceMock;
        private readonly Mock<IDbContextTransaction> _transactionMock;

        private readonly ProjectService _projectService;

        public ProjectServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userProfileServiceMock = new Mock<IUserProfileService>();
            _transactionMock = new Mock<IDbContextTransaction>();

            _projectService = new ProjectService(_unitOfWorkMock.Object, _userProfileServiceMock.Object);
        }

        [Fact]
        public async Task CreateProjectAsync_CreatesProjectAndAddsCreatorAsAdmin()
        {
            // Arrange
            var createProjectDto = new CreateProjectDto
            {
                Title = "New Project",
                Description = "Test Description",
                Status = ProjectStatus.NotStarted,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(10),
                CreatedBy = "user-123"
            };

            var projectId = 1;
            var userProfileId = 2;

            var userProfiles = new List<UserProfile>
            {
                new UserProfile { Id = userProfileId, UserId = createProjectDto.CreatedBy }
            };

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<IsolationLevel>()))
                .ReturnsAsync(_transactionMock.Object);

            _unitOfWorkMock.Setup(u => u.Projects.AddAsync(It.IsAny<Project>()))
                .Callback<Project>(p => p.Id = projectId)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _unitOfWorkMock.Setup(u => u.UserProfiles.FindByExpressionAsync(
                    It.IsAny<Expression<Func<UserProfile, bool>>>(),
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync(userProfiles);

            _unitOfWorkMock.Setup(u => u.ProjectMembers.AddAsync(It.IsAny<ProjectMember>()))
                .Returns(Task.CompletedTask);

            // Act
            await _projectService.CreateProjectAsync(createProjectDto);

            // Assert
            _unitOfWorkMock.Verify(u => u.Projects.AddAsync(It.Is<Project>(p => p.Title == createProjectDto.Title)), Times.Once);

            _unitOfWorkMock.Verify(u => u.UserProfiles.FindByExpressionAsync(
                It.IsAny<Expression<Func<UserProfile, bool>>>(),
                null, null, null, null), Times.Once);

            _unitOfWorkMock.Verify(u => u.ProjectMembers.AddAsync(It.Is<ProjectMember>(pm =>
                pm.ProjectId == projectId &&
                pm.UserProfileId == userProfileId &&
                pm.Role == ProjectRole.Admin)), Times.Once);

            _transactionMock.Verify(t => t.CommitAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_UpdatesProjectSuccessfully()
        {
            // Arrange
            var projectDto = new Project
            {
                Id = 1,
                Title = "Updated Project",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow,
                Status = ProjectStatus.InProgress
            };

            projectDto.SetEndDate(DateTime.UtcNow.AddDays(5));
            _unitOfWorkMock.Setup(u => u.Projects.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _projectService.UpdateProjectAsync(projectDto);

            // Assert
            _unitOfWorkMock.Verify(u => u.Projects.UpdateAsync(It.Is<Project>(p =>
                p.Title == projectDto.Title &&
                p.Description == projectDto.Description &&
                p.Status == projectDto.Status &&
                p.StartDate == projectDto.StartDate &&
                p.EndDate == projectDto.EndDate &&
                p.UpdatedBy == projectDto.UpdatedBy &&
                p.UpdatedDate == projectDto.UpdatedDate)), Times.Once);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_ThrowsArgumentNullException_WhenProjectIsNull()
        {
#pragma warning disable CS8625
            await Assert.ThrowsAsync<ArgumentNullException>(() => _projectService.UpdateProjectAsync(null));
#pragma warning restore CS8625
        }

        [Fact]
        public async Task CreateProjectAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            // Act & Assert
#pragma warning disable CS8625
            await Assert.ThrowsAsync<ArgumentNullException>(() => _projectService.CreateProjectAsync(null));
#pragma warning restore CS8625
        }

        [Fact]
        public async Task GetProjectStatusOptions_ReturnsAllEnumOptions()
        {
            var result = await _projectService.GetProjectStatusOptions();
            var statusList = result.ToList();

            var expectedStatuses = Enum.GetValues(typeof(ProjectStatus)).Cast<ProjectStatus>().ToList();
            Assert.Equal(expectedStatuses.Count, statusList.Count);

            foreach (var status in expectedStatuses)
            {
                var item = statusList.FirstOrDefault(i => i.Value == status.ToString());
                Assert.NotNull(item);
                Assert.Equal(status.GetDisplayName(), item.Text);
            }
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsProjectWithAllIncludes()
        {
            // Arrange
            var projectId = 1;

            var identityUser = new IdentityUser
            {
                Id = "user-1",
                UserName = "testuser"
            };

            var userProfile = new UserProfile
            {
                Id = 2,
                FullName = "Test User",
                UserId = identityUser.Id,
                User = identityUser
            };

            var projectMember = new ProjectMember
            {
                Id = 3,
                ProjectId = projectId,
                UserProfileId = userProfile.Id,
                UserProfile = userProfile,
                Role = ProjectRole.Contributor
            };

            var taskItem = new TaskItem
            {
                Id = 4,
                Title = "Sample Task",
                ProjectId = projectId
            };

            var invitation = new ProjectInvitation
            {
                Id = 5,
                ProjectId = projectId,
                InvitedUserProfileId = userProfile.Id,
                InvitedUserProfile = userProfile,
                Status = InvitationStatus.Pending
            };

            var expectedProject = new Project
            {
                Id = projectId,
                Title = "Sample Project",
                Members = new List<ProjectMember> { projectMember },
                TaskItems = new List<TaskItem> { taskItem },
                Invitations = new List<ProjectInvitation> { invitation }
            };

            _unitOfWorkMock.Setup(u => u.Projects.FindByExpressionAsync(
                It.IsAny<Expression<Func<Project, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<Project>, IQueryable<Project>>>(),
                null,
                null
            )).ReturnsAsync(new List<Project> { expectedProject });

            // Act
            var result = await _projectService.GetProjectByIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(projectId, result.Id);
            Assert.Single(result.Members);
            Assert.Single(result.TaskItems);
            Assert.Single(result.Invitations);
            Assert.Equal("Test User", result.Members.First().UserProfile.FullName);
            Assert.Equal("testuser", result.Members.First().UserProfile.User.UserName);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenProjectDoesNotExist()
        {
            _unitOfWorkMock.Setup(u => u.Projects.FindByExpressionAsync(
                It.IsAny<Expression<Func<Project, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<Project>, IQueryable<Project>>>(),
                null,
                null))
                .ReturnsAsync(new List<Project>());

            var result = await _projectService.GetProjectByIdAsync(99);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetFilteredProjectsAsync_ReturnsPaginatedProjectList_WhenUserExists()
        {
            // Arrange
            var userId = "user-123";
            var userProfileId = 10;

            var projectFilterDto = new ProjectFilterDto
            {
                UserId = userId,
                Title = "Test",
                Status = ProjectStatus.NotStarted,
                Role = ProjectRole.Admin,
                StartDateFrom = DateTime.UtcNow.AddDays(-1),
                StartDateTo = DateTime.UtcNow.AddDays(1),
                EndDateFrom = DateTime.UtcNow.AddDays(10),
                EndDateTo = DateTime.UtcNow.AddDays(20)
            };

            var pageIndex = 1;
            var pageSize = 2;

            var mockProjects = new List<ProjectMember>
            {
                new ProjectMember
                {
                    Id = 1,
                    Role = ProjectRole.Admin,
                    Project = new Project
                    {
                        Id = 1,
                        Title = "Test Project 1",
                        Status = ProjectStatus.NotStarted,
                        StartDate = DateTime.UtcNow.AddDays(-1)
                    }
                },
                new ProjectMember
                {
                    Id = 2,
                    Role = ProjectRole.Contributor,
                    Project = new Project
                    {
                        Id = 2,
                        Title = "Test Project 2",
                        Status = ProjectStatus.InProgress,
                        StartDate = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            _userProfileServiceMock.Setup(u => u.GetByUserIdAsync(userId))
                .ReturnsAsync(userProfileId);

            _unitOfWorkMock.Setup(u => u.ProjectMembers.GetPaginatedListAsync(
                    It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                    It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync((mockProjects.AsQueryable(), mockProjects.Count));

            // Act
            var result = await _projectService.GetFilteredProjectsAsync(projectFilterDto, pageIndex, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(mockProjects.Count, result.TotalCount);

            var firstItem = result.Items.FirstOrDefault();
            Assert.Equal(1, firstItem?.ProjectId);
            Assert.Equal("Test Project 1", firstItem?.ProjectTitle);
            Assert.Equal(ProjectRole.Admin, firstItem?.UserRoleInThisProject);
        }

        [Fact]
        public async Task GetFilteredProjectsAsync_ReturnsEmptyList_WhenUserIdIsNull()
        {
            // Arrange
            var filter = new ProjectFilterDto { UserId = null };
            var pageIndex = 1;
            var pageSize = 5;

            // Act
            var result = await _projectService.GetFilteredProjectsAsync(filter, pageIndex, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetFilteredProjectsAsync_ReturnsEmptyList_WhenUserProfileIdIsZero()
        {
            // Arrange
            var filter = new ProjectFilterDto { UserId = "user-123" };
            var pageIndex = 1;
            var pageSize = 5;

            _userProfileServiceMock
                .Setup(u => u.GetByUserIdAsync(filter.UserId))
                .ReturnsAsync(0);

            // Act
            var result = await _projectService.GetFilteredProjectsAsync(filter, pageIndex, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetFilteredProjectsAsync_UsesDefaultOrdering_WhenSortByInvalid()
        {
            // Arrange
            var dto = new ProjectFilterDto
            {
                UserId = "user-1",
                SortBy = "invalid_field",
                SortOrder = "asc"
            };

            _userProfileServiceMock.Setup(x => x.GetByUserIdAsync("user-1")).ReturnsAsync(1);

            var mockProjects = new List<ProjectMember>
            {
                new ProjectMember
                {
                    ProjectId = 1,
                    Project = new Project { Title = "Test", StartDate = DateTime.UtcNow },
                    Role = ProjectRole.Contributor
                }
            };

            _unitOfWorkMock.Setup(u => u.ProjectMembers.GetPaginatedListAsync(
                It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).ReturnsAsync((mockProjects.AsQueryable(), mockProjects.Count));

            // Act
            var result = await _projectService.GetFilteredProjectsAsync(dto, 1, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
        }

    }
}
