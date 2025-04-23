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

        [Fact]
        public void GetDisplayName_ShouldHandleInvalidEnumValue()
        {
            // Test casting an invalid value to the enum
            var invalidRole = (ProjectRole)999;

            // Verify the behavior - will either return a default string or throw an exception
            // Adjust the assertion based on expected behavior
            var exception = Record.Exception(() => invalidRole.GetDisplayName());
            Assert.Null(exception); 
        }
    }
}
