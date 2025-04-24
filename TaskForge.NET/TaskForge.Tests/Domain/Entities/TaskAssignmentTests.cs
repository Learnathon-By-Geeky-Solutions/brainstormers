using TaskForge.Domain.Entities;
using Xunit;

namespace TaskForge.Tests.Domain.Entities
{
    public class TaskAssignmentTests
    {
        [Fact]
        public void TaskAssignment_Should_Set_And_Get_Properties()
        {
            var taskItem = new TaskItem
            {
                Id = 10,
                Title = "Test Task",
                Description = "Test Description",
                CreatedBy = "test-user"
            };
            var userProfile = new UserProfile
            {
                Id = 20
            };
            var assignment = new TaskAssignment
            {
                Id = 1,
                TaskItemId = taskItem.Id,
                TaskItem = taskItem,
                UserProfileId = 20,
                CreatedBy = "test-user",
                UserProfile = userProfile
            };

            Assert.Equal(1, assignment.Id);
            Assert.NotNull(assignment.TaskItem);
            Assert.Equal(taskItem, assignment.TaskItem);
            Assert.Equal(userProfile, assignment.UserProfile);
            Assert.Equal(10, assignment.TaskItemId);
            Assert.Equal(20, assignment.UserProfileId);
            Assert.Equal("test-user", assignment.CreatedBy);
        }
    }
}
