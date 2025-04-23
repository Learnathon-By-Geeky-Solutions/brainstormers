using TaskForge.Domain.Entities;
using Xunit;

namespace TaskForge.Tests.Domain.Entities
{
    public class TaskAssignmentTests
    {
        [Fact]
        public void TaskAssignment_Should_Set_And_Get_Properties()
        {
            var assignment = new TaskAssignment
            {
                Id = 1,
                TaskItemId = 10,
                UserProfileId = 20,
                CreatedBy = "test-user"
            };

            Assert.Equal(1, assignment.Id);
            Assert.Equal(10, assignment.TaskItemId);
            Assert.Equal(20, assignment.UserProfileId);
            Assert.Equal("test-user", assignment.CreatedBy);
        }
    }
}
