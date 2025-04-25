using TaskForge.Application.DTOs;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Application.DTOs
{
    public class TaskFilterDtoTests
    {
        [Fact]
        public void Constructor_DefaultValues_ShouldSetSortOrderToAsc()
        {
            // Arrange & Act
            var dto = new TaskFilterDto();

            // Assert
            Assert.Null(dto.UserProfileId);
            Assert.Null(dto.ProjectId);
            Assert.Null(dto.Title);
            Assert.Null(dto.Status);
            Assert.Null(dto.Priority);
            Assert.Null(dto.StartDateFrom);
            Assert.Null(dto.StartDateTo);
            Assert.Null(dto.DueDateFrom);
            Assert.Null(dto.DueDateTo);
            Assert.Null(dto.SortBy);
            Assert.Equal("asc", dto.SortOrder);
        }

        [Fact]
        public void SetProperties_AllProperties_ShouldReturnSameValues()
        {
            // Arrange
            var dto = new TaskFilterDto
            {
                UserProfileId = 10,
                ProjectId = 20,
                Title = "Test task",
                Status = TaskWorkflowStatus.InProgress,
                Priority = TaskPriority.High,
                StartDateFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                StartDateTo = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                DueDateFrom = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                DueDateTo = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc),
                SortBy = "DueDate",
                SortOrder = "desc"
            };

            // Assert
            Assert.Equal(10, dto.UserProfileId);
            Assert.Equal(20, dto.ProjectId);
            Assert.Equal("Test task", dto.Title);
            Assert.Equal(TaskWorkflowStatus.InProgress, dto.Status);
            Assert.Equal(TaskPriority.High, dto.Priority);
            Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), dto.StartDateFrom);
            Assert.Equal(new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc), dto.StartDateTo);
            Assert.Equal(new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc), dto.DueDateFrom);
            Assert.Equal(new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc), dto.DueDateTo);
            Assert.Equal("DueDate", dto.SortBy);
            Assert.Equal("desc", dto.SortOrder);
        }
    }
}
