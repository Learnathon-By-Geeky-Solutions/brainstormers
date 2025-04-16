using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Moq;
using TaskForge.Application.Services;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Application.DTOs;
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
			var task = new TaskItem
			{
				Id = taskId,
				Attachments = null,        // Null collections
				AssignedUsers = null
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
				.ReturnsAsync((TaskAttachment)null);

			// Act & Assert
			await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.DeleteAttachmentAsync(attachmentId));
		}
	}
}

