using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Enums
{
    public class TaskWorkflowStatusTests
    {
        [Theory]
        [InlineData(TaskWorkflowStatus.ToDo, "To Do")]
        [InlineData(TaskWorkflowStatus.InProgress, "In Progress")]
        [InlineData(TaskWorkflowStatus.Done, "Done")]
        [InlineData(TaskWorkflowStatus.Blocked, "Blocked")]
        public void TaskWorkflowStatus_DisplayName_ShouldMatch(TaskWorkflowStatus status, string expectedName)
        {
            var name = status.GetDisplayName();
            Assert.Equal(expectedName, name);
        }
    }
}
