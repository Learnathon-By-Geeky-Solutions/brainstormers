using TaskForge.Domain.Entities;
using Xunit;

namespace TaskForge.Tests.Domain.Entities
{
    public class TaskDependencyTests
    {
        [Fact]
        public void TaskDependency_Should_Set_And_Get_Properties()
        {
            var dependency = new TaskDependency
            {
                Id = 1,
                TaskId = 11,
                DependsOnTaskId = 22,
                CreatedBy = "admin"
            };

            Assert.Equal(1, dependency.Id);
            Assert.Equal(11, dependency.TaskId);
            Assert.Equal(22, dependency.DependsOnTaskId);
            Assert.Equal("admin", dependency.CreatedBy);
        }
    }
}
