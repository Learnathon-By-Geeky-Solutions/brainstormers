using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.CodeAnalysis;
using Moq;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.WebUI.Controllers;
using TaskForge.WebUI.Models;
using Xunit;
using System.Linq;

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

            _controller.Url = new UrlHelper(new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ControllerActionDescriptor()
            });
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

            var model = new TaskItemCreateViewModel() { ProjectId = 1};

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
        public async Task Create_ReturnUnauthorized_WhenUserIsNull()
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
                Attachments = []
            };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((IdentityUser?)null);
            // Act
            var result = await _controller.Create(model);
            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task Create_ReturnsFailureJson_WhenMemberIsNull()
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
                Attachments = []
            };
            var user = new IdentityUser { Id = "testuser" };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, model.ProjectId))
                .ReturnsAsync((ProjectMemberDto?)null);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var type = value.GetType();

            var success = (bool)type.GetProperty("success")!.GetValue(value)!;
            var message = (string)type.GetProperty("message")!.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("You do not have permission to create tasks in this project.", message);

            // Verify the service was called with correct parameters
            _projectMemberServiceMock.Verify(
                x => x.GetUserProjectRoleAsync(user.Id, model.ProjectId),
                Times.Once);

            // Alternatively, if you want to explicitly verify that member was null
            // (though the test name already implies this)
            var member = await _projectMemberServiceMock.Object.GetUserProjectRoleAsync(user.Id, model.ProjectId);
            Assert.Null(member);
        }
        [Fact]
        public async Task Create_ReturnsFailureJson_WhenMemberIsViewer()
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
                Attachments = []
            };
            var user = new IdentityUser { Id = "testuser" };
            var viewerMember = new ProjectMemberDto { Role = ProjectRole.Viewer };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, model.ProjectId))
                .ReturnsAsync(viewerMember);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var type = value.GetType();

            var success = (bool)type.GetProperty("success")!.GetValue(value)!;
            var message = (string)type.GetProperty("message")!.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("You do not have permission to create tasks in this project.", message);
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
            // Arrange
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync((TaskItem?)null);
            // Act
            var result = await _controller.GetTask(99);
            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task GetTask_ModelStateInvalid_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("id", "Required");

            // Act
            var result = await _controller.GetTask(1);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var valueType = value.GetType();

            var success = (bool)valueType.GetProperty("success")!.GetValue(value)!;
            var message = valueType.GetProperty("message")!.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("Invalid Data", message);
        }
        [Fact]
        public async Task GetTask_ReturnsJsonResult_WhenTaskExists()
        {
            // Arrange
            int taskId = 1;
            var task = new TaskItem
            {
                Id = taskId,
                Title = "Test Task",
                Description = "Description",
                ProjectId = 100,
                StartDate = new DateTime(2025, 4, 30, 9, 0, 0, DateTimeKind.Utc),
                Status = TaskWorkflowStatus.InProgress,
                Priority = TaskPriority.High,
                Attachments =
                [
                    new TaskAttachment { Id = 1, FileName = "test.pdf", StoredFileName = "abc123.pdf" }
                ],
                AssignedUsers =
                [
                    new TaskAssignment { UserProfileId = 42 }
                ],
                Dependencies =
                [
                    new TaskDependency { DependsOnTaskId = 2 }
                ]
            };
            task.SetDueDate(new DateTime(2025, 5, 5, 17, 0, 0, DateTimeKind.Utc));

            var members = new List<ProjectMemberDto>
            {
                new() { UserProfileId = 42, Name = "Jane Doe" }
            };

            var dependentTaskIds = new List<int> { 5, 6 };

            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(taskId)).ReturnsAsync(task);
            _projectMemberServiceMock.Setup(s => s.GetProjectMembersAsync(task.ProjectId)).ReturnsAsync(members);
            _taskServiceMock.Setup(s => s.GetDependentTaskIdsAsync(taskId, task.Status)).ReturnsAsync(dependentTaskIds);

            // Act
            var result = await _controller.GetTask(taskId);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var type = value.GetType();

            Assert.Equal(task.Id, type.GetProperty("id")!.GetValue(value));
            Assert.Equal(task.Title, type.GetProperty("title")!.GetValue(value));
            Assert.Equal(100, type.GetProperty("projectId")!.GetValue(value));
			Assert.Equal(task.Description, type.GetProperty("description")!.GetValue(value));
            Assert.Equal("2025-04-30T09:00", type.GetProperty("startDate")!.GetValue(value));
            Assert.Equal("2025-05-05T17:00", type.GetProperty("dueDate")!.GetValue(value));
            Assert.Equal((int)task.Status, type.GetProperty("status")!.GetValue(value));
            Assert.Equal((int)task.Priority, type.GetProperty("priority")!.GetValue(value));

            // Attachments
            var attachments = type.GetProperty("attachments")!.GetValue(value) as IEnumerable<object>;
            var attachment = attachments!.First();
            var atType = attachment.GetType();
            Assert.Equal("test.pdf", atType.GetProperty("fileName")!.GetValue(attachment));
            Assert.Equal("/uploads/tasks/abc123.pdf", atType.GetProperty("downloadUrl")!.GetValue(attachment));

            // Assigned Users
            var assignedIdsObj = type.GetProperty("assignedUserIds")!.GetValue(value);
            Assert.NotNull(assignedIdsObj);
            var assignedIds = (assignedIdsObj as IEnumerable<int>) ?? (assignedIdsObj as IEnumerable<object>)?.Cast<int>();
            Assert.NotNull(assignedIds);
            Assert.Contains(42, assignedIds!);

            // All Users
            var allUsers = type.GetProperty("allUsers")!.GetValue(value) as IEnumerable<object>;
            var user = allUsers!.First();
            var userType = user.GetType();
            Assert.Equal(42, userType.GetProperty("id")!.GetValue(user));
            Assert.Equal("Jane Doe", userType.GetProperty("name")!.GetValue(user));

            // Dependencies
            var dependsOnObj = type.GetProperty("dependsOnTaskIds")!.GetValue(value);
            Assert.NotNull(dependsOnObj);
            var dependsOn = (dependsOnObj as IEnumerable<int>) ?? (dependsOnObj as IEnumerable<object>)?.Cast<int>();
            Assert.NotNull(dependsOn);
            Assert.Contains(2, dependsOn!);

            // Dependent Task IDs
            var dependentTaskIdsObj = type.GetProperty("dependentTaskIds")!.GetValue(value);
            Assert.NotNull(dependentTaskIdsObj);
            var dependentTaskIdsList = (dependentTaskIdsObj as IEnumerable<int>) ?? (dependentTaskIdsObj as IEnumerable<object>)?.Cast<int>();
            Assert.NotNull(dependentTaskIdsList);
            Assert.Contains(5, dependentTaskIdsList!);
            Assert.Contains(6, dependentTaskIdsList!);
        }
        [Fact]
        public async Task GetTask_ReturnsEmptyCollections_WhenTaskHasNoRelatedData()
        {
            // Arrange
            int taskId = 1;
            var task = new TaskItem
            {
                Id = taskId,
                Title = "Empty Task"
            };

            var members = new List<ProjectMemberDto>();
            var dependentTaskIds = new List<int>();

            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(taskId)).ReturnsAsync(task);
            _projectMemberServiceMock.Setup(s => s.GetProjectMembersAsync(task.ProjectId)).ReturnsAsync(members);
            _taskServiceMock.Setup(s => s.GetDependentTaskIdsAsync(taskId, task.Status)).ReturnsAsync(dependentTaskIds);

            // Act
            var result = await _controller.GetTask(taskId);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var type = value.GetType();

            Assert.Equal(task.Id, type.GetProperty("id")!.GetValue(value));
            Assert.Equal(task.Title, type.GetProperty("title")!.GetValue(value));
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
			// Arrange
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
        public async Task Update_ReturnsFailureJson_WhenTaskIsNull()
        {
            // Arrange
            var user = new IdentityUser { Id = "test" };
            var dto = new TaskUpdateDto { Id = 1, Title = "title" };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _taskServiceMock.Setup(t => t.GetTaskByIdAsync(1))
                .ReturnsAsync((TaskItem?)null);
            // Act
            var result = await _controller.Update(dto);
            //Assert
            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var type = value.GetType();

            var success = (bool)type.GetProperty("success")!.GetValue(value)!;
            var message = (string)type.GetProperty("message")!.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("Task not found.", message);
        }
        [Fact]
        public async Task Update_ReturnsFailureJson_WhenMemberIsNull()
        {
            // Arrange
            var user = new IdentityUser { Id = "test" };
            var dto = new TaskUpdateDto { Id = 1, Title = "title" };
            var task = new TaskItem { Id = 1, ProjectId = 10 };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _taskServiceMock.Setup(x => x.GetTaskByIdAsync(1))
                .ReturnsAsync(task);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, task.ProjectId))
                .ReturnsAsync((ProjectMemberDto?)null);

            // Act
            var result = await _controller.Update(dto);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var type = value.GetType();
            var success = (bool)type.GetProperty("success")!.GetValue(value)!;
            var message = (string)type.GetProperty("message")!.GetValue(value)!;
            Assert.False(success);
            Assert.Equal("You do not have permission to update tasks in this project.", message);

            // Verify the service was called with correct parameters
            _projectMemberServiceMock.Verify(
                x => x.GetUserProjectRoleAsync(user.Id, task.ProjectId),
                Times.Once);
            var member = await _projectMemberServiceMock.Object.GetUserProjectRoleAsync(user.Id, task.ProjectId);
            Assert.Null(member);
        }
        [Fact]
        public async Task Update_ReturnsFailureJson_WhenMemberIsViewer()
        {
            // Arrange
            var user = new IdentityUser { Id = "test" };
            var dto = new TaskUpdateDto { Id = 1, Title = "title" };
            var task = new TaskItem { Id = 1, ProjectId = 10 };
            var viewerMember = new ProjectMemberDto { Role = ProjectRole.Viewer };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _taskServiceMock.Setup(x => x.GetTaskByIdAsync(1))
                .ReturnsAsync(task);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, task.ProjectId))
                .ReturnsAsync(viewerMember);

            // Act
            var result = await _controller.Update(dto);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var type = value.GetType();
            var success = (bool)type.GetProperty("success")!.GetValue(value)!;
            var message = (string)type.GetProperty("message")!.GetValue(value)!;
            Assert.False(success);
            Assert.Equal("You do not have permission to update tasks in this project.", message);

            // Verify the service was called with correct parameters
            _projectMemberServiceMock.Verify(
                x => x.GetUserProjectRoleAsync(user.Id, task.ProjectId),
                Times.Once);
            var member = await _projectMemberServiceMock.Object.GetUserProjectRoleAsync(user.Id, task.ProjectId);
            Assert.Equal(ProjectRole.Viewer, member!.Role);
        }



        [Fact]
        public async Task Delete_ThrowsException_ReturnsErrorMessage()
        {
            // Arrange
            var taskId = 2;
            var user = new IdentityUser { Id = "user2" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _taskServiceMock.Setup(x => x.GetTaskByIdAsync(taskId))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _controller.Delete(taskId);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;
            Assert.False(success);
            Assert.Equal("Something went wrong", message);
        }
        [Fact]
        public async Task Delete_TaskDeletedSuccessfully_ReturnsSuccessMessage()
        {
            // Arrange
            var taskId = 1;
            var user = new IdentityUser { Id = "user1" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _taskServiceMock.Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(new TaskItem { Id = taskId, ProjectId = 10 });


            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, 10))
                .ReturnsAsync(new ProjectMemberDto { Role = ProjectRole.Admin });

            _taskServiceMock.Setup(x => x.RemoveTaskAsync(taskId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(taskId);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = json.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            var message = (string)value.GetType().GetProperty("message")!.GetValue(value)!;
            Assert.True(success);
            Assert.Equal("Task deleted successfully.", message);
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
