using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Enums
{
    public class ProjectRoleTests
    {
        [Theory]
        [InlineData(ProjectRole.Admin)]
        [InlineData(ProjectRole.Contributor)]
        [InlineData(ProjectRole.Viewer)]
        public void ProjectRole_ShouldHaveValidValues(ProjectRole role)
        {
            Assert.True((int)role >= 0 && (int)role <= 2);
        }
    }
}
