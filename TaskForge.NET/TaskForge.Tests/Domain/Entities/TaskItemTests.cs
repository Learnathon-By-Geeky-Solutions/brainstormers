using System.ComponentModel.DataAnnotations;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Entities
{
    public class TaskItemTests
    {
        [Fact]
        public void TaskItem_Should_Set_And_Get_Properties()
        {
            var task = new TaskItem
            {
                Id = 1,
                Title = "Implement login",
                Description = "Create login functionality",
                ProjectId = 101,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.High,
                StartDate = DateTime.UtcNow,
                CreatedBy = "dev"
            };

            Assert.Equal(1, task.Id);
            Assert.Equal("Implement login", task.Title);
            Assert.Equal("Create login functionality", task.Description);
            Assert.Equal(101, task.ProjectId);
            Assert.Equal(TaskWorkflowStatus.ToDo, task.Status);
            Assert.Equal(TaskPriority.High, task.Priority);
            Assert.NotNull(task.StartDate);
            Assert.Equal("dev", task.CreatedBy);
        }

        [Fact]
        public void SetDueDate_Should_Set_DueDate_When_Valid()
        {
            var task = new TaskItem
            {
                StartDate = DateTime.UtcNow
            };

            var dueDate = task.StartDate.Value.AddDays(2);
            task.SetDueDate(dueDate);

            Assert.Equal(dueDate, task.DueDate);
        }

        [Fact]
        public void SetDueDate_Should_Throw_If_DueDate_Before_StartDate()
        {
            var task = new TaskItem
            {
                StartDate = DateTime.UtcNow
            };

            var earlierDate = task.StartDate.Value.AddDays(-1);

            Assert.Throws<ValidationException>(() => task.SetDueDate(earlierDate));
        }

        [Fact]
        public void SetStatus_To_InProgress_Should_Set_StartDate_If_Null()
        {
            var task = new TaskItem();

            task.SetStatus(TaskWorkflowStatus.InProgress);

            Assert.Equal(TaskWorkflowStatus.InProgress, task.Status);
            Assert.NotNull(task.StartDate);
        }

        [Fact]
        public void SetStatus_To_Done_Should_Not_Change_StartDate_If_Already_Set()
        {
            var startDate = DateTime.UtcNow.AddDays(-2);
            var task = new TaskItem { StartDate = startDate };

            task.SetStatus(TaskWorkflowStatus.Done);

            Assert.Equal(TaskWorkflowStatus.Done, task.Status);
            Assert.Equal(startDate, task.StartDate);
        }
    }
}
