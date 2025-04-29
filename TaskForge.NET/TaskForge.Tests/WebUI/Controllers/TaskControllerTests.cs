using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.WebUI.Controllers;
using TaskForge.WebUI.Models;
using Xunit;

namespace TaskForge.Tests.WebUI.Controllers
{
    public class TaskControllerTests
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<IProjectMemberService> _projectMemberServiceMock;
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly TaskController _controller;

        public TaskControllerTests()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            _taskServiceMock = new Mock<ITaskService>();
            _projectMemberServiceMock = new Mock<IProjectMemberService>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _userManagerMock = new Mock<UserManager<IdentityUser>>(store.Object,
                null, null, null, null, null, null, null, null);

            _controller = new TaskController(_taskServiceMock.Object,
                _projectMemberServiceMock.Object,
                _userManagerMock.Object);
        }

        [Fact]
        public async Task Create_ReturnsSuccessJson_WhenModelIsValid()
        {
            // Arrange
            var model = new TaskItemCreateViewModel
            {
                ProjectId = 1,
                Title = "Test Task",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(1),
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Attachments = new List<Microsoft.AspNetCore.Http.IFormFile>()
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new IdentityUser { Id = "testuser" });
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync("testuser", model.ProjectId))
                .ReturnsAsync(new ProjectMemberDto { Role = ProjectRole.Contributor });
            _taskServiceMock.Setup(x => x.CreateTaskAsync(It.IsAny<TaskDto>()))
                 .Returns(Task.CompletedTask)
                 .Verifiable();


            // Act
            var result = await _controller.Create(model);

            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.True(success);
            Assert.Equal("Task created successfully.", message);
            _userManagerMock.Verify();
            _projectMemberServiceMock.Verify();
            _taskServiceMock.Verify();
        }
        [Fact]
        public async Task Create_ReturnsFailureJson_WhenModelIsInvalid()
        {
            _controller.ModelState.AddModelError("Title", "Required");

            var model = new TaskItemCreateViewModel();

            var result = await _controller.Create(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("Invalid data.", message);
            _taskServiceMock.Verify(s => s.CreateTaskAsync(It.IsAny<TaskDto>()), Times.Never);
        }
        [Fact]
        public async Task Create_ReturnsFailureJson_WhenServiceThrowsException()
        {
            // Arrange
            var userId = "user123";
            var user = new IdentityUser { Id = userId };
            var model = new TaskItemCreateViewModel
            {
                ProjectId = 1,
                Title = "Test Task",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(1),
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Attachments = new List<Microsoft.AspNetCore.Http.IFormFile>()
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, model.ProjectId))
                                      .ReturnsAsync(new ProjectMemberDto { Role = ProjectRole.Contributor });
            _taskServiceMock.Setup(x => x.CreateTaskAsync(It.IsAny<TaskDto>()))
                .ThrowsAsync(new Exception("Failed to create"));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);

            var serialized = JsonConvert.SerializeObject(jsonResult.Value);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(serialized)!;

            Assert.False((bool)data["success"]);
            Assert.Equal("Failed to create", data["message"]);
        }
        [Fact]
        public async Task Create_ReturnsSuccess_WhenAttachmentsNull()
        {
            // Arrange
            var model = new TaskItemCreateViewModel
            {
                ProjectId = 2,
                Title = "No Attachment Task",
                Status = TaskWorkflowStatus.InProgress,
                Priority = TaskPriority.High,
                Attachments = null
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new IdentityUser { Id = "testuser" });

            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync("testuser", model.ProjectId))
                .ReturnsAsync(new ProjectMemberDto { Role = ProjectRole.Contributor });

            _taskServiceMock.Setup(x => x.CreateTaskAsync(It.IsAny<TaskDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var successProp = value.GetType().GetProperty("success")!;
            var messageProp = value.GetType().GetProperty("message")!;

            var success = (bool)successProp.GetValue(value)!;
            var message = (string)messageProp.GetValue(value)!;

            Assert.True(success);
            Assert.Equal("Task created successfully.", message);

            _taskServiceMock.Verify(s => s.CreateTaskAsync(It.Is<TaskDto>(dto => dto.Attachments == null)), Times.Once);
        }




        [Fact]
        public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync((TaskItem?)null);

            var result = await _controller.GetTask(1);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task GetTask_ReturnsNotFound_WhenTaskIsNull()
        {
            _taskServiceMock.Setup(x => x.GetTaskByIdAsync(It.IsAny<int>())).ReturnsAsync((TaskItem?)null);
            var result = await _controller.GetTask(999);
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task GetTask_InvalidModelState_ReturnsView()
        {
            // Arrange
            _controller.ModelState.AddModelError("AnyKey", "Invalid value");

            // Act
            var result = await _controller.GetTask(1);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;
            Assert.False(success);
            Assert.Equal("Invalid Data", message);
        }
        [Fact]
        public async Task GetTask_TaskNotFound_ReturnsNotFound()
        {
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync((TaskItem?)null);
            var result = await _controller.GetTask(99);
            Assert.IsType<NotFoundResult>(result);
        }



        [Fact]
        public void SetDueDate_DueBeforeStart_ThrowsValidationException()
        {
            var task = new TaskItem
            {
                StartDate = new DateTime(2025, 4, 20, 0, 0, 0, DateTimeKind.Utc)
            };

            var ex = Assert.Throws<ValidationException>(() => task.SetDueDate(new DateTime(2025, 4, 19, 0, 0, 0, DateTimeKind.Utc)));
            Assert.Equal("Due date cannot be earlier than Start date.", ex.Message);
        }
        [Fact]
        public void SetStatus_InProgress_SetsStartDateIfNull()
        {
            var task = new TaskItem
            {
                StartDate = null
            };

            task.SetStatus(TaskWorkflowStatus.InProgress);

            Assert.NotNull(task.StartDate);
            Assert.Equal(TaskWorkflowStatus.InProgress, task.Status);
        }
        [Fact]
        public void SetStatus_InProgress_DoesNotOverrideStartDate()
        {
            var originalStart = new DateTime(2025, 4, 10, 9, 0, 0, DateTimeKind.Utc);
            var task = new TaskItem
            {
                StartDate = originalStart
            };

            task.SetStatus(TaskWorkflowStatus.InProgress);

            Assert.Equal(originalStart, task.StartDate);
            Assert.Equal(TaskWorkflowStatus.InProgress, task.Status);
        }



        [Fact]
        public async Task Update_WhenUserIsNull_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new TaskUpdateDto { Id = 1, Title = "title" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((IdentityUser?)null);

            // Act
            var result = await _controller.Update(dto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task Update_ReturnsFailureJson_WhenModelStateIsInvalid()
        {
            // Arrange
            var dto = new TaskUpdateDto { Id = 1, Title = "title" };
            _controller.ModelState.AddModelError("Title", "Required");
            // Act
            var result = await _controller.Update(dto);
            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("Invalid data.", message);
            _taskServiceMock.Verify(s => s.UpdateTaskAsync(It.IsAny<TaskUpdateDto>()), Times.Never);
        }
        [Fact]
        public async Task Update_ReturnsFailureJson_WhenServiceThrowsException()
        {
            var dto = new TaskUpdateDto
            {
                Id = 1,
                Title = "Update Test",
                Description = "Some desc",
                Priority = 2,
                Status = 1,
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(2),
                AssignedUserIds = new List<int> { 1, 2 },
                Attachments = new List<IFormFile>()
            };

            // Arrange
            var dto = new TaskUpdateDto { Id = 1, Title = "title" };
            var exceptionMessage = "Service error";


            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.Update(dto);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;
            Assert.False(success);
            Assert.Equal(exceptionMessage, message);
        }
        [Fact]
        public async Task Update_ReturnsSuccessJson_WhenUpdateSucceeds()
        {

            // Arrange
            var userId = "user123";
            var user = new IdentityUser { Id = userId };
            var taskId = 1;
            var projectId = 10;

            var dto = new TaskUpdateDto { Id = taskId, Title = "title" };
            var task = new TaskItem { Id = taskId, ProjectId = projectId };
            var projectMember = new ProjectMemberDto { Role = ProjectRole.Admin };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user).Verifiable();
            _taskServiceMock.Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task).Verifiable();
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(userId, projectId))
                .ReturnsAsync(projectMember).Verifiable();
            _taskServiceMock.Setup(x => x.UpdateTaskAsync(dto))
                .Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _controller.Update(dto);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.True(success);
            Assert.Equal("Task updated successfully.", message);
            _userManagerMock.Verify();
            _taskServiceMock.Verify();
            _projectMemberServiceMock.Verify();
            _taskServiceMock.Verify();
        }


        [Fact]
        public async Task Delete_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("id", "Required");

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }
        [Fact]
        public async Task Delete_ReturnsFailureJson_WhenTaskIsNotFound()
        {
            // Arrange
            var user = new IdentityUser { Id = "user123" };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _taskServiceMock.Setup(x => x.GetTaskByIdAsync(It.IsAny<int>())).ReturnsAsync((TaskItem?)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;
            Assert.False(success);
            Assert.Equal("Task not found.", message);
        }
        [Fact]
        public async Task Delete_ReturnsFailureJson_WhenMemberIsNullOrViewer()
        {
            // Arrange
            var user = new IdentityUser { Id = "user123" };
            var task = new TaskItem { Id = 1, ProjectId = 10 };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _taskServiceMock.Setup(x => x.GetTaskByIdAsync(1)).ReturnsAsync(task);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, task.ProjectId)).ReturnsAsync((ProjectMemberDto?)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;
            Assert.False(success);
            Assert.Equal("You do not have permission to delete tasks in this project.", message);
        }
        [Fact]
        public async Task Delete_ReturnsFailureJson_WhenMemberRoleIsViewer()
        {
            // Arrange
            var user = new IdentityUser { Id = "user123" };
            var task = new TaskItem { Id = 1, ProjectId = 10 };
            var viewerMember = new ProjectMemberDto { Role = ProjectRole.Viewer };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _taskServiceMock.Setup(x => x.GetTaskByIdAsync(1)).ReturnsAsync(task);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, task.ProjectId)).ReturnsAsync(viewerMember);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;
            Assert.False(success);
            Assert.Equal("You do not have permission to delete tasks in this project.", message);
        }



        [Fact]
        public async Task DeleteAttachment_ReturnsUnauthorized_WhenUserIsNull()
        {
            // Arrange
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((IdentityUser)null!);

            // Act
            var result = await _controller.DeleteAttachment(10);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task DeleteAttachment_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("AnyKey", "Invalid value");

            // Act
            var result = await _controller.DeleteAttachment(5);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;
            Assert.False(success);
            Assert.Equal("Invalid Data", message);
        }
        [Fact]
        public async Task Delete_ReturnsFailureJson_WhenServiceThrowsException()
        {
            // Arrange
            var id = 2;
            var userId = "user123";
            var user = new IdentityUser { Id = userId };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _taskServiceMock.Setup(x => x.DeleteAttachmentAsync(id, userId)).ThrowsAsync(new Exception("Failed to delete"));

            // Act
            var result = await _controller.DeleteAttachment(id);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;
            Assert.False(success);
            Assert.Equal("Failed to delete", message);
        }
        [Fact]
        public async Task DeleteAttachment_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("id", "Invalid");
            // Act
            var result = await _controller.DeleteAttachment(1);
            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;
            Assert.False(success);
            Assert.Equal("Invalid Data", message);
        }
        [Fact]
        public async Task DeleteAttachment_ReturnsSuccessJson_WhenDeletedSuccessfully()
        {
            // Arrange
            int Id = 123;
            var userId = "test01";
            var user = new IdentityUser { Id = userId };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user).Verifiable();
            _taskServiceMock.Setup(x => x.DeleteAttachmentAsync(Id, userId))
                .Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _controller.DeleteAttachment(Id);

            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.True(success);
            Assert.Equal("TaskAttachment deleted successfully.", message);
            _userManagerMock.Verify();
            _taskServiceMock.Verify();
        }
        [Fact]
        public async Task DeleteAttachment_ReturnsErrorJson_WhenExceptionThrown()
        {
            // Arrange
            var id = 999;
            var userId = "test01";
            var user = new IdentityUser { Id = userId };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _taskServiceMock.Setup(x => x.DeleteAttachmentAsync(id, userId))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _controller.DeleteAttachment(id);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("Something went wrong", message);
            _taskServiceMock.Verify(s => s.DeleteAttachmentAsync(id, userId), Times.Once);
        }

    }
}
