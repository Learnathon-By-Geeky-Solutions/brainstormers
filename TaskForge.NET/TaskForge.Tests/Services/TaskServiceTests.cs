using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _fileServiceMock = new Mock<IFileService>();
            _taskService = new TaskService(_unitOfWorkMock.Object, _fileServiceMock.Object);
        }

        [Fact]
        public async Task GetTaskListAsync_ReturnsTasksForProject()
        {
            // Arrange
            var projectId = 1;
            var taskList = new List<TaskItem> { new TaskItem { Id = 1, ProjectId = projectId } };

            _unitOfWorkMock.Setup(u => u.Tasks.FindByExpressionAsync(
                    It.IsAny<Expression<Func<TaskItem, bool>>>(),
                    It.IsAny<Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>(),
                    It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .ReturnsAsync(taskList);

            // Act
            var result = await _taskService.GetTaskListAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }




        [Fact]
        public async Task GetTaskByIdAsync_ReturnsTask_WhenTaskExists()
        {
            // Arrange
            var taskId = 1;

            var expectedTask = new TaskItem
            {
                Id = taskId,
                Title = "Sample Task",
                IsDeleted = false,
                Attachments = new List<TaskAttachment>
                {
                    new TaskAttachment { Id = 101, FileName = "file1.pdf", IsDeleted = false },
                    new TaskAttachment { Id = 102, FileName = "file2.pdf", IsDeleted = true }
                },
                AssignedUsers = new List<TaskAssignment>
                {
                    new TaskAssignment
                    {
                        Id = 201,
                        UserProfile = new UserProfile { Id = 1, FullName = "John Doe" }
                    }
                },
                Project = new Project
                {
                    Id = 10,
                    Title = "Project Alpha"
                }
            };

            _unitOfWorkMock.Setup(u => u.Tasks.FindByExpressionAsync(
                    It.IsAny<Expression<Func<TaskItem, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                    null, null))
                .ReturnsAsync(new List<TaskItem> { expectedTask });

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskId, result!.Id);
            Assert.Equal("Sample Task", result.Title);
            Assert.Single(result.Attachments.Where(a => !a.IsDeleted));
            Assert.Single(result.AssignedUsers);
            Assert.Equal("John Doe", result.AssignedUsers.First().UserProfile.FullName);
            Assert.Equal("Project Alpha", result.Project.Title);
        }
        [Fact]
        public async Task GetTaskByIdAsync_ReturnsNull_WhenTaskDoesNotExist()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Tasks.FindByExpressionAsync(
                    It.IsAny<Expression<Func<TaskItem, bool>>>(),
                    It.IsAny<Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>(),
                    null, null, null))
                .ReturnsAsync(new List<TaskItem>()); // no task found

            // Act
            var result = await _taskService.GetTaskByIdAsync(999); // non-existent ID

            // Assert
            Assert.Null(result);
        }




        [Fact]
        public async Task GetUserTaskAsync_ReturnsPaginatedTasks_WhenUserProfileIdIsValid()
        {
            // Arrange
            int userProfileId = 1;
            int pageIndex = 1;
            int pageSize = 2;

            var projectMembers = new List<ProjectMember>
            {
                new ProjectMember { ProjectId = 10, UserProfileId = userProfileId },
                new ProjectMember { ProjectId = 20, UserProfileId = userProfileId }
            };

            var task1 = new TaskItem
            {
                Id = 1,
                Title = "Task 1",
                ProjectId = 10,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Project = new Project { Id = 10, Title = "Project A" }
            };
            task1.SetDueDate(DateTime.UtcNow.AddDays(1));

            var task2 = new TaskItem
            {
                Id = 2,
                Title = "Task 2",
                ProjectId = 20,
                Status = TaskWorkflowStatus.InProgress,
                Priority = TaskPriority.High,
                Project = new Project { Id = 20, Title = "Project B" }
            };
            task2.SetDueDate(DateTime.UtcNow.AddDays(1));

            var tasks = new List<TaskItem> { task1, task2 };

            _unitOfWorkMock.Setup(u => u.ProjectMembers.FindByExpressionAsync(
                It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                null, null, null, null))
                .ReturnsAsync(projectMembers);

            _unitOfWorkMock.Setup(u => u.Tasks.GetPaginatedListAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                pageSize, 0))
                .ReturnsAsync((tasks, tasks.Count));

            // Act
            var result = await _taskService.GetUserTaskAsync(userProfileId, pageIndex, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal("Task 1", result.Items[0].Title);
            Assert.Equal("Project A", result.Items[0].ProjectTitle);
            Assert.Equal("Task 2", result.Items[1].Title);
            Assert.Equal("Project B", result.Items[1].ProjectTitle);
        }
        [Fact]
        public async Task GetUserTaskAsync_ReturnsEmptyPaginatedList_WhenUserProfileIdIsNull()
        {
            // Arrange
            int? userProfileId = null;
            int pageIndex = 1;
            int pageSize = 5;

            // Act
            var result = await _taskService.GetUserTaskAsync(userProfileId, pageIndex, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }




        [Fact]
        public async Task CreateTaskAsync_ThrowsIfAttachmentsExceedLimit()
        {
            // Arrange
            var taskDto = new TaskDto
            {
                Title = "Test",
                Attachments = Enumerable.Range(1, 11)
                    .Select(_ => (IFormFile)new FormFile(Stream.Null, 0, 1, "file", "test.txt"))
                    .ToList()
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.CreateTaskAsync(taskDto));
        }



        [Fact]
        public async Task UpdateTaskAsync_SuccessfullyUpdatesTask()
        {
            // Arrange
            var taskId = 1;
            var userId = 1;
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("testfile.png");
            fileMock.Setup(f => f.Length).Returns(1024);
            var attachments = new List<IFormFile> { fileMock.Object };
            var dto = new TaskUpdateDto
            {
                Id = taskId,
                Title = "Updated Task",
                Description = "Updated Task Description",
                Status = (int)TaskWorkflowStatus.InProgress,
                Priority = (int)TaskPriority.High,
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(5),
                AssignedUserIds = new List<int> { userId },
                Attachments = attachments
            };

            var existingTask = new TaskItem
            {
                Id = taskId,
                IsDeleted = false,
                Title = "Old Task",
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Attachments = new List<TaskAttachment>()
            };

            // Corrected the mock setup to use Expression<Func<TaskItem, bool>> instead of Func<TaskItem, bool>
            _unitOfWorkMock.Setup(r => r.Tasks.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                null, null
            )).ReturnsAsync(new List<TaskItem> { existingTask });

            var user = new UserProfile
            {
                Id = userId,
                FullName = "Test User"
            };

            _unitOfWorkMock.Setup(r => r.UserProfiles.FindByExpressionAsync(It.IsAny<Expression<Func<UserProfile, bool>>>(), null, null, null, null))
                .ReturnsAsync(new List<UserProfile> { user });

            // Act
            await _taskService.UpdateTaskAsync(dto);

            // Assert
            _unitOfWorkMock.Verify(r => r.Tasks.UpdateAsync(It.Is<TaskItem>(t => t.Id == taskId && t.Title == dto.Title)), Times.Once);
        }


        [Fact]
        public async Task UpdateTaskAsync_TaskNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new TaskUpdateDto { Id = 999, Title = "Title1" }; // Non-existing task ID

            _unitOfWorkMock.Setup(r => r.Tasks.FindByExpressionAsync(It.IsAny<Expression<Func<TaskItem, bool>>>(), null, It.IsAny<Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>(), 0, 0))
                              .ReturnsAsync(new List<TaskItem>());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.UpdateTaskAsync(dto));
        }

        [Fact]
        public async Task UpdateTaskAsync_TooManyAttachments_ThrowsInvalidOperationException()
        {
            // Arrange
            var taskId = 1;
            var dto = new TaskUpdateDto
            {
                Id = taskId,
                Title = "Title1",
                Attachments = new List<IFormFile>(new IFormFile[11]) // 11 attachments
            };

            var existingTask = new TaskItem
            {
                Id = taskId,
                IsDeleted = false,
                Attachments = new List<TaskAttachment>()
            };

            _unitOfWorkMock.Setup(r => r.Tasks.FindByExpressionAsync(
                    It.IsAny<Expression<Func<TaskItem, bool>>>(),
                    null, It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                    null, null
                    ))
                .ReturnsAsync(new List<TaskItem> { existingTask });
            _unitOfWorkMock.Setup(r => r.Tasks.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                null, null
            )).ReturnsAsync(new List<TaskItem> { existingTask });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.UpdateTaskAsync(dto));
        }

        [Fact]
        public async Task UpdateTaskAsync_UpdatesAssignedUsersCorrectly()
        {
            // Arrange
            var taskId = 1;
            var userId = 2;

            var dto = new TaskUpdateDto
            {
                Id = taskId,
                Title = "Updated Task",
                AssignedUserIds = new List<int> { userId },
                Attachments = new List<IFormFile>(), // Empty to skip file logic
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(3),
                Status = (int)TaskWorkflowStatus.InProgress,
                Priority = (int)TaskPriority.Medium
            };

            var existingTask = new TaskItem
            {
                Id = taskId,
                IsDeleted = false,
                Title = "Old Task",
                AssignedUsers = new List<TaskAssignment>(),
                Attachments = new List<TaskAttachment>()
            };

            var user = new UserProfile
            {
                Id = userId,
                FullName = "Test User"
            };

            // Setup mock for task fetch (with expression)
            _unitOfWorkMock.Setup(r => r.Tasks.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                null, null
            )).ReturnsAsync(new List<TaskItem> { existingTask });

            // Setup mock for user profile fetch
            _unitOfWorkMock.Setup(r => r.UserProfiles.FindByExpressionAsync(
                It.IsAny<Expression<Func<UserProfile, bool>>>(),
                null, null, null, null
            )).ReturnsAsync(new List<UserProfile> { user });

            _unitOfWorkMock.Setup(r => r.Tasks.UpdateAsync(It.IsAny<TaskItem>()))
                           .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(r => r.SaveChangesAsync())
                           .ReturnsAsync(1);

            // Act
            await _taskService.UpdateTaskAsync(dto);

            // Assert
            _unitOfWorkMock.Verify(r => r.Tasks.UpdateAsync(It.Is<TaskItem>(
                t => t.AssignedUsers.Count == 1 &&
                     t.AssignedUsers.First().UserProfile.Id == userId
            )), Times.Once);
        }




        [Fact]
        public async Task RemoveTaskAsync_DeletesTaskAndAttachmentsAndAssignments()
        {
            // Arrange
            var taskId = 1;
            var attachments = new List<TaskAttachment>
            {
                new TaskAttachment { Id = 101, FilePath = "file1.txt" },
                new TaskAttachment { Id = 102, FilePath = "file2.txt" }
            };
            var assignments = new List<TaskAssignment>
            {
                new TaskAssignment { Id = 201 },
                new TaskAssignment { Id = 202 }
            };

            var task = new TaskItem
            {
                Id = taskId,
                Attachments = attachments,
                AssignedUsers = assignments
            };

            _unitOfWorkMock.Setup(u => u.Tasks.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                null, null))
                .ReturnsAsync(new List<TaskItem> { task });

            _fileServiceMock.Setup(f => f.DeleteFileAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.TaskAttachments.DeleteByIdsAsync(It.IsAny<IEnumerable<int>>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.TaskAssignments.DeleteByIdsAsync(It.IsAny<IEnumerable<int>>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.Tasks.DeleteByIdAsync(taskId)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _taskService.RemoveTaskAsync(taskId);

            // Assert
            _fileServiceMock.Verify(f => f.DeleteFileAsync("file1.txt"), Times.Once);
            _fileServiceMock.Verify(f => f.DeleteFileAsync("file2.txt"), Times.Once);
            _unitOfWorkMock.Verify(u => u.TaskAttachments.DeleteByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(101) && ids.Contains(102))), Times.Once);
            _unitOfWorkMock.Verify(u => u.TaskAssignments.DeleteByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(201) && ids.Contains(202))), Times.Once);
            _unitOfWorkMock.Verify(u => u.Tasks.DeleteByIdAsync(taskId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task RemoveTaskAsync_ThrowsKeyNotFoundException_WhenTaskNotFound()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Tasks.FindByExpressionAsync(
                    It.IsAny<Expression<Func<TaskItem, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                    null, null))
                .ReturnsAsync(new List<TaskItem>()); // No task found

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.RemoveTaskAsync(999));
            Assert.Equal("Task not found", ex.Message);
        }
        [Fact]
        public async Task RemoveTaskAsync_SkipsDeletes_WhenNoAttachmentsOrAssignments()
        {
            // Arrange
            var taskId = 1;
            var task = new TaskItem
            {
                Id = taskId,
                Attachments = new List<TaskAttachment>(),   // No attachments
                AssignedUsers = new List<TaskAssignment>()  // No assignments
            };

            _unitOfWorkMock.Setup(u => u.Tasks.FindByExpressionAsync(
                    It.IsAny<Expression<Func<TaskItem, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                    null, null))
                .ReturnsAsync(new List<TaskItem> { task });

            _unitOfWorkMock.Setup(u => u.Tasks.DeleteByIdAsync(taskId)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _taskService.RemoveTaskAsync(taskId);

            // Assert
            _fileServiceMock.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.TaskAttachments.DeleteByIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.TaskAssignments.DeleteByIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.Tasks.DeleteByIdAsync(taskId), Times.Once);
        }
        [Fact]
        public async Task RemoveTaskAsync_InitializesNullCollections()
        {
            // Arrange
            var taskId = 1;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var task = new TaskItem
            {
                Id = taskId,
                Attachments = null,        // Null collections
                AssignedUsers = null
            };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            _unitOfWorkMock.Setup(u => u.Tasks.FindByExpressionAsync(
                    It.IsAny<Expression<Func<TaskItem, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                    null, null))
                .ReturnsAsync(new List<TaskItem> { task });

            _unitOfWorkMock.Setup(u => u.Tasks.DeleteByIdAsync(taskId)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var ex = await Record.ExceptionAsync(() => _taskService.RemoveTaskAsync(taskId));

            // Assert
            Assert.Null(ex); // Should NOT throw NullReferenceException
        }




        [Fact]
        public async Task DeleteAttachmentAsync_DeletesAttachmentFile()
        {
            // Arrange
            var attachmentId = 1;
            var attachment = new TaskAttachment { Id = attachmentId, FilePath = "somepath/file.txt" };

            _unitOfWorkMock.Setup(u => u.TaskAttachments.GetByIdAsync(attachmentId))
                .ReturnsAsync(attachment);

            // Act
            await _taskService.DeleteAttachmentAsync(attachmentId);

            // Assert
            _fileServiceMock.Verify(f => f.DeleteFileAsync("somepath/file.txt"), Times.Once);
            _unitOfWorkMock.Verify(u => u.TaskAttachments.DeleteByIdAsync(attachmentId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task DeleteAttachmentAsync_ThrowsIfNotFound()
        {
            // Arrange
            var attachmentId = 999;
            _unitOfWorkMock.Setup(u => u.TaskAttachments.GetByIdAsync(attachmentId))
                .ReturnsAsync(value: null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.DeleteAttachmentAsync(attachmentId));
        }
    }
}

