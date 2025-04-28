using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
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
            var model = new TaskItemCreateViewModel
            {
                ProjectId = 1,
                Title = "Test Task",
                Description = "Test Description",
                StartDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(1),
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Attachments = new List<Microsoft.AspNetCore.Http.IFormFile>()
            };

            var result = await _controller.Create(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.True(success);
            Assert.Equal("Task created successfully.", message);
            _taskServiceMock.Verify(s => s.CreateTaskAsync(It.IsAny<TaskDto>()), Times.Once);
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
            var model = new TaskItemCreateViewModel
            {
                ProjectId = 1,
                Title = "Test Task",
                Description = "Test Description",
                StartDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(1),
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Attachments = new List<Microsoft.AspNetCore.Http.IFormFile>()
            };

            _taskServiceMock
                .Setup(s => s.CreateTaskAsync(It.IsAny<TaskDto>()))
                .ThrowsAsync(new Exception("Something went wrong"));

            var result = await _controller.Create(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("Something went wrong", message);
            _taskServiceMock.Verify(s => s.CreateTaskAsync(It.IsAny<TaskDto>()), Times.Once);
        }
        [Fact]
        public async Task Create_ReturnsSuccess_WhenAttachmentsNull()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var model = new TaskItemCreateViewModel
            {
                ProjectId = 2,
                Title = "No Attachment Task",
                Status = TaskWorkflowStatus.InProgress,
                Priority = TaskPriority.High,
                Attachments = null
            };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            var result = await _controller.Create(model);

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
        public async Task GetTask_ReturnsView_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("id", "Invalid");

            var result = await _controller.GetTask(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
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
            _controller.ModelState.AddModelError("Error", "Invalid model");
            var result = await _controller.GetTask(1);
            Assert.IsType<ViewResult>(result);
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
        public async Task GetTask_ValidTask_ReturnsJsonResult()
        {
            var taskId = 1;
            var taskItem = new TaskItem
            {
                Id = taskId,
                Title = "Sample Task",
                Description = "Desc",
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                ProjectId = 1,
                StartDate = new DateTime(2024, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                Attachments = new List<TaskAttachment>
                {
                    new TaskAttachment { Id = 1, FileName = "file1.png", StoredFileName = "file1_stored.png" }
                },
                AssignedUsers = new List<TaskAssignment>
                {
                    new TaskAssignment { UserProfileId = 2 }
                }
            };

            var projectMembers = new List<ProjectMemberDto>
            {
                new ProjectMemberDto { UserProfileId = 2, Name = "Jane", Email = "jane@example.com", ProjectId = 1, Role = ProjectRole.Viewer }
            };

            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(taskId)).ReturnsAsync(taskItem);
            _projectMemberServiceMock.Setup(s => s.GetProjectMembersAsync(taskItem.ProjectId))
                                     .ReturnsAsync(projectMembers);

            var result = await _controller.GetTask(taskId) as JsonResult;

            Assert.NotNull(result);
            var data = result.Value!.GetType().GetProperty("title")?.GetValue(result.Value);
            Assert.Equal("Sample Task", data);
        }
        [Fact]
        public async Task GetTask_ValidTask_ReturnsCompleteJsonData()
        {
            var taskId = 1;
            var taskItem = new TaskItem
            {
                Id = taskId,
                Title = "Test Task",
                Description = "Task Description",
                Status = TaskWorkflowStatus.InProgress,
                Priority = TaskPriority.High,
                ProjectId = 42,
                StartDate = new DateTime(2025, 4, 20, 10, 0, 0, DateTimeKind.Utc),
                Attachments = new List<TaskAttachment>
                {
                    new TaskAttachment { Id = 100, FileName = "report.pdf", StoredFileName = "report_123.pdf", IsDeleted = false }
                },
                AssignedUsers = new List<TaskAssignment>
                {
                    new TaskAssignment { UserProfileId = 99 }
                }
            };
            taskItem.SetDueDate(new DateTime(2025, 4, 25, 18, 0, 0, DateTimeKind.Utc));
            taskItem.SetStatus(TaskWorkflowStatus.InProgress);

            var projectMembers = new List<ProjectMemberDto>
            {
                new ProjectMemberDto
                {
                    Id = 10,
                    ProjectId = 42,
                    UserProfileId = 99,
                    Name = "Alice",
                    Email = "alice@example.com",
                    Role = ProjectRole.Admin
                }
            };

            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(taskId)).ReturnsAsync(taskItem);
            _projectMemberServiceMock.Setup(s => s.GetProjectMembersAsync(42)).ReturnsAsync(projectMembers);

            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
            _controller.ControllerContext = new ControllerContext(actionContext);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.Content("~/uploads/tasks/report_123.pdf"))
                         .Returns("/uploads/tasks/report_123.pdf");
            _controller.Url = urlHelperMock.Object;

            var result = await _controller.GetTask(taskId) as JsonResult;

            Assert.NotNull(result);

            var json = JsonSerializer.Serialize(result!.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal(taskItem.Id, root.GetProperty("id").GetInt32());
            Assert.Equal(taskItem.Title, root.GetProperty("title").GetString());
            Assert.Equal(taskItem.Description, root.GetProperty("description").GetString());
            Assert.Equal("2025-04-20T10:00", root.GetProperty("startDate").GetString());
            Assert.Equal("2025-04-25T18:00", root.GetProperty("dueDate").GetString());
            Assert.Equal((int)taskItem.Status, root.GetProperty("status").GetInt32());
            Assert.Equal((int)taskItem.Priority, root.GetProperty("priority").GetInt32());

            var attachments = root.GetProperty("attachments").EnumerateArray().ToList();
            Assert.Single(attachments);
            var attachment = attachments.FirstOrDefault();
            Assert.Equal(100, attachment.GetProperty("id").GetInt32());
            Assert.Equal("report.pdf", attachment.GetProperty("fileName").GetString());
            Assert.Equal("/uploads/tasks/report_123.pdf", attachment.GetProperty("downloadUrl").GetString());

            var assignedUserIds = root.GetProperty("assignedUserIds").EnumerateArray().ToList();
            Assert.Single(assignedUserIds);
            Assert.Equal(99, assignedUserIds.FirstOrDefault().GetInt32());

            var users = root.GetProperty("allUsers").EnumerateArray().ToList();
            Assert.Single(users);
            var user = users.FirstOrDefault();
            Assert.Equal(99, user.GetProperty("id").GetInt32());
            Assert.Equal("Alice", user.GetProperty("name").GetString());
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
        public async Task Update_ReturnsFailureJson_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Title", "Required");
            var dto = new TaskUpdateDto
            {
                Id = 1,
                Title = "Invalid Task"
            };

            var result = await _controller.Update(dto);

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
                StartDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(2),
                AssignedUserIds = new List<int> { 1, 2 },
                Attachments = new List<IFormFile>()
            };

            _taskServiceMock
                .Setup(s => s.UpdateTaskAsync(It.IsAny<TaskUpdateDto>()))
                .ThrowsAsync(new Exception("Update failed"));

            var result = await _controller.Update(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("Update failed", message);
            _taskServiceMock.Verify(s => s.UpdateTaskAsync(dto), Times.Once);
        }
        [Fact]
        public async Task Update_ReturnsSuccessJson_WhenUpdateSucceeds()
        {
            var dto = new TaskUpdateDto
            {
                Id = 1,
                Title = "Valid Task",
                Description = "Valid Desc",
                Priority = 1,
                Status = 0,
                StartDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(1),
                AssignedUserIds = new List<int> { 1, 2 },
                Attachments = new List<IFormFile>()
            };

            _taskServiceMock
                .Setup(s => s.UpdateTaskAsync(It.IsAny<TaskUpdateDto>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.Update(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.True(success);
            Assert.Equal("Task updated successfully.", message);
            _taskServiceMock.Verify(s => s.UpdateTaskAsync(dto), Times.Once);
        }



        [Fact]
        public async Task Delete_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("id", "Invalid");

            var result = await _controller.Delete(1);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            _taskServiceMock.Verify(s => s.RemoveTaskAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async Task Delete_ReturnsSuccessJson_WhenTaskDeletedSuccessfully()
        {
            int taskId = 10;

            _taskServiceMock
                .Setup(s => s.RemoveTaskAsync(taskId))
                .Returns(Task.CompletedTask);

            var result = await _controller.Delete(taskId);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.True(success);
            Assert.Equal("Task deleted successfully.", message);
            _taskServiceMock.Verify(s => s.RemoveTaskAsync(taskId), Times.Once);
        }
        [Fact]
        public async Task Delete_ReturnsFailureJson_WhenServiceThrowsException()
        {
            int taskId = 99;

            _taskServiceMock
                .Setup(s => s.RemoveTaskAsync(taskId))
                .ThrowsAsync(new Exception("Delete failed"));

            var result = await _controller.Delete(taskId);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("Delete failed", message);
            _taskServiceMock.Verify(s => s.RemoveTaskAsync(taskId), Times.Once);
        }



        [Fact]
        public async Task DeleteAttachment_ReturnsView_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("id", "Invalid");

            var result = await _controller.DeleteAttachment(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            _taskServiceMock.Verify(s => s.DeleteAttachmentAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);

            Assert.False(viewResult.ViewData.ModelState.IsValid);
        }
        [Fact]
        public async Task DeleteAttachment_ReturnsSuccessJson_WhenDeletedSuccessfully()
        {
            int attachmentId = 123;
            var userId = "test01";
            _taskServiceMock.Setup(s => s.DeleteAttachmentAsync(attachmentId, userId)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteAttachment(attachmentId);

            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.True(success);
            Assert.Equal("TaskAttachment deleted successfully.", message);
            _taskServiceMock.Verify(s => s.DeleteAttachmentAsync(attachmentId, userId), Times.Once);
        }
        [Fact]
        public async Task DeleteAttachment_ReturnsErrorJson_WhenExceptionThrown()
        {
            int attachmentId = 999;
            var userId = "test01";
            _taskServiceMock
                .Setup(s => s.DeleteAttachmentAsync(attachmentId, userId))
                .ThrowsAsync(new Exception("Something went wrong"));

            var result = await _controller.DeleteAttachment(attachmentId);

            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("Something went wrong", message);
            _taskServiceMock.Verify(s => s.DeleteAttachmentAsync(attachmentId, userId), Times.Once);
        }

    }
}
