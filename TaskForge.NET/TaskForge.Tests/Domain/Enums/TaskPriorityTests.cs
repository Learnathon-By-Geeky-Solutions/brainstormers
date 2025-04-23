using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Enums
{
    public class TaskPriorityTests
    {
        [Theory]
        [InlineData(TaskPriority.Low, "Low")]
        [InlineData(TaskPriority.Medium, "Medium")]
        [InlineData(TaskPriority.High, "High")]
        [InlineData(TaskPriority.Critical, "Critical")]
        public void TaskPriority_DisplayName_ShouldMatch(TaskPriority priority, string expectedName)
        {
            var name = priority.GetDisplayName();
            Assert.Equal(expectedName, name);
        }
    }
}
