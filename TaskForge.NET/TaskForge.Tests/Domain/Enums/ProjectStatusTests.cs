using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Enums
{
    public class ProjectStatusTests
    {
        [Theory]
        [InlineData(ProjectStatus.NotStarted, "Not Started")]
        [InlineData(ProjectStatus.InProgress, "In Progress")]
        [InlineData(ProjectStatus.OnHold, "On Hold")]
        [InlineData(ProjectStatus.Completed, "Completed")]
        [InlineData(ProjectStatus.Cancelled, "Cancelled")]
        public void ProjectStatus_DisplayName_ShouldMatch(ProjectStatus status, string expectedName)
        {
            var name = status.GetDisplayName();
            Assert.Equal(expectedName, name);
        }
    }
}
