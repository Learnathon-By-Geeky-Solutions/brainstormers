using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Enums
{
    public class ProjectRoleTests
    {
        [Theory]
        [InlineData(ProjectRole.Admin, "Admin")]
        [InlineData(ProjectRole.Contributor, "Contributor")]
        [InlineData(ProjectRole.Viewer, "Viewer")]
        public void ProjectRole_DisplayName_ShouldMatch(ProjectRole role, string expectedName)
        {
            var name = role.GetDisplayName();
            Assert.Equal(expectedName, name);
        }
    }
}
