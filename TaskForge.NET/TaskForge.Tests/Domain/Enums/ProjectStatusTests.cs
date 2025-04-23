using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Enums
{
    public class ProjectStatusTests
    {
        [Theory]
        [InlineData(ProjectStatus.NotStarted)]
        [InlineData(ProjectStatus.InProgress)]
        [InlineData(ProjectStatus.OnHold)]
        [InlineData(ProjectStatus.Completed)]
        [InlineData(ProjectStatus.Cancelled)]
        public void ProjectStatus_ShouldHaveValidValues(ProjectStatus status)
        {
            Assert.True((int)status >= 0 && (int)status <= 4);
        }
    }
}
