using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Entities
{
    public class ProjectTests
    {
        [Fact]
        public async Task Constructor_ShouldInitializeDefaults()
        {
            // Act
            var project = await Task.FromResult(new Project());

            // Assert
            Assert.Equal(string.Empty, project.Title);
            Assert.Null(project.Description);
            Assert.Equal(ProjectStatus.NotStarted, project.Status);
            Assert.NotEqual(default, project.StartDate);
            Assert.True(project.StartDate.Kind == DateTimeKind.Utc);
            Assert.Empty(project.Members);
            Assert.Empty(project.Invitations);
            Assert.Empty(project.TaskItems);
        }

        [Fact]
        public void SetEndDate_ShouldAssignValidDate()
        {
            // Arrange
            var project = new Project();
            var endDate = project.StartDate.AddDays(1);

            // Act
            project.SetEndDate(endDate);

            // Assert
            Assert.Equal(endDate, project.EndDate);
        }

        [Fact]
        public async Task SetEndDate_ShouldSetToNull_IfNullPassed()
        {
            // Arrange
            var project = new Project();

            // Act
            await Task.Run(() => project.SetEndDate(null));

            // Assert
            Assert.Null(project.EndDate);
        }

        [Fact]
        public void SetEndDate_ShouldThrowIfEndDateBeforeStartDate()
        {
            // Arrange
            var project = new Project();
            var invalidEndDate = project.StartDate.AddDays(-1);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                project.SetEndDate(invalidEndDate));
        }

        [Fact]
        public async Task Collections_ShouldAddSuccessfully()
        {
            // Arrange
            var project = new Project();

            var member = new ProjectMember { ProjectId = project.Id };
            var invitation = new ProjectInvitation { ProjectId = project.Id };
            var taskItem = new TaskItem { ProjectId = project.Id };

            // Act
            await Task.Run(() =>
            {
                project.Members.Add(member);
                project.Invitations.Add(invitation);
                project.TaskItems.Add(taskItem);
            });

            // Assert
            Assert.Single(project.Members);
            Assert.Single(project.Invitations);
            Assert.Single(project.TaskItems);
        }

        [Fact]
        public async Task SetEndDate_ShouldVerifyAllScenarios()
        {
            // Arrange
            var startDate = new DateTime(2023, 10, 1, 0, 0, 0, DateTimeKind.Utc);
            var project = new Project { StartDate = startDate };

            // Act
            project.SetEndDate(null);

            // Assert
            Assert.Null(project.EndDate);
            Assert.Equal(startDate, project.StartDate);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                Task.Run(() => project.SetEndDate(startDate.AddDays(-1))));

            project.SetEndDate(startDate.AddDays(1));
            Assert.Equal(startDate.AddDays(1), project.EndDate);
        }

    }
}
