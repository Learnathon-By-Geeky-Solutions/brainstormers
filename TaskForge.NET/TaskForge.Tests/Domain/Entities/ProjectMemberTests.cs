using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Entities
{
    public class ProjectMemberTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaults()
        {
            // Act
            var member = new ProjectMember();

            // Assert
            Assert.Equal(ProjectRole.Admin, member.Role);
            Assert.Equal(0, member.ProjectId);
            Assert.Equal(0, member.UserProfileId);

            // These are null by default and must be explicitly initialized
            Assert.Null(member.Project);
            Assert.Null(member.UserProfile);
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var project = new Project { Id = 1, Title = "Test Project" };
            var userProfile = new UserProfile { Id = 2 };

            var member = new ProjectMember
            {
                Id = 10,
                ProjectId = 1,
                Project = project,
                UserProfileId = 2,
                UserProfile = userProfile,
                Role = ProjectRole.Admin,
                CreatedBy = "Tester",
                UpdatedBy = "Admin",
                IsDeleted = true
            };

            // Assert
            Assert.Equal(10, member.Id);
            Assert.Equal(1, member.ProjectId);
            Assert.Equal(project, member.Project);
            Assert.Equal(2, member.UserProfileId);
            Assert.Equal(userProfile, member.UserProfile);
            Assert.Equal(ProjectRole.Admin, member.Role);
            Assert.Equal("Tester", member.CreatedBy);
            Assert.Equal("Admin", member.UpdatedBy);
            Assert.True(member.IsDeleted);
        }
    }
}
