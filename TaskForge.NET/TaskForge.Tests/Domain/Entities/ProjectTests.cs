using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Entities
{
    public class ProjectTests
    {
        [Fact]
        public void Constructor_ShouldInitializeDefaults()
        {
            var project = new Project();

            Assert.Equal(string.Empty, project.Title);
            Assert.Equal(ProjectStatus.NotStarted, project.Status);
            Assert.True((DateTime.UtcNow - project.StartDate).TotalSeconds < 5);
            Assert.NotNull(project.Members);
            Assert.NotNull(project.Invitations);
            Assert.NotNull(project.TaskItems);
        }

        [Fact]
        public void SetEndDate_ShouldSetValidDate()
        {
            var project = new Project { StartDate = DateTime.UtcNow };
            var futureDate = project.StartDate.AddDays(5);

            project.SetEndDate(futureDate);

            Assert.Equal(futureDate, project.EndDate);
        }

        [Fact]
        public void SetEndDate_ShouldThrow_WhenEndDateIsBeforeStartDate()
        {
            var project = new Project { StartDate = DateTime.UtcNow };
            var pastDate = project.StartDate.AddDays(-1);

            var ex = Assert.Throws<ArgumentException>(() => project.SetEndDate(pastDate));
            Assert.Equal("EndDate cannot be earlier than StartDate.", ex.Message);
        }
    }
}
