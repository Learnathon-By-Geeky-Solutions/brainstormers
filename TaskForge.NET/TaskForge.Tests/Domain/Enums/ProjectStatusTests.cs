using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Enums
{
    public class ProjectStatusTests
    {
        [Fact]
        public void ProjectStatus_ShouldHaveExactlyFiveValues()
        {
            Assert.Equal(5, Enum.GetValues(typeof(ProjectStatus)).Length);
        }
    }
}
