using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Enums
{
    public class TaskPriorityTests
    {
        [Theory]
        [InlineData(TaskPriority.Low)]
        [InlineData(TaskPriority.Medium)]
        [InlineData(TaskPriority.High)]
        [InlineData(TaskPriority.Critical)]
        public void TaskPriority_ShouldHaveValidValues(TaskPriority priority)
        {
            Assert.True((int)priority >= 0 && (int)priority <= 3);
        }
    }
}
