using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Enums;
using TaskForge.WebUI.Controllers;
using TaskForge.WebUI.Models;
using Xunit;
namespace TaskForge.Tests.WebUILayer
{
    public class HomeControllerTests
    {
        private readonly Mock<IProjectMemberService> _projectMemberServiceMock = new();
        private readonly Mock<IUserProfileService> _userProfileServiceMock = new();
        private readonly Mock<ITaskService> _taskServiceMock = new();
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;

        public HomeControllerTests()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _userManagerMock = new Mock<UserManager<IdentityUser>>(store.Object,
                null, null, null, null, null, null, null, null);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        private HomeController CreateController(ClaimsPrincipal? user = null)
        {
            var controller = new HomeController(
                _projectMemberServiceMock.Object,
                _userProfileServiceMock.Object,
                _taskServiceMock.Object,
                _userManagerMock.Object
            );

            if (user != null)
            {
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                };
            }

            return controller;
        }

        [Fact]
        public async Task Index_UserNotAuthenticated_ReturnsWelcomeView()
        {
            var controller = CreateController(new ClaimsPrincipal(new ClaimsIdentity()));

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Welcome", viewResult.ViewName);
        }

        [Fact]
        public async Task Index_ModelStateInvalid_RedirectsToIndex()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "mock"));
            var controller = CreateController(user);
            controller.ModelState.AddModelError("Error", "Some error");

            var result = await controller.Index();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Index_UserIsNull_RedirectsToLogin()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "mock"));
            var controller = CreateController(user);

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((IdentityUser?)null);

            var result = await controller.Index();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Index_UserProfileNotFound_ReturnsNotFound()
        {
            var user = new IdentityUser { Id = "user1" };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "mock"));
            var controller = CreateController(principal);

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _userProfileServiceMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync((int?)null);

            var result = await controller.Index();

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Index_ValidUser_ReturnsDashboardViewWithFullTaskDtoCoverage()
        {
            var user = new IdentityUser { Id = "user1" };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "mock"));

            var controller = CreateController(principal);

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _userProfileServiceMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(5);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectCountAsync(5)).ReturnsAsync(3);

            var now = DateTime.UtcNow;

            var taskList = new PaginatedList<TaskDto>(
                new List<TaskDto>
                {
            new TaskDto
            {
                Id = 101,
                ProjectId = 202,
                ProjectTitle = "Project X",
                Title = "Fix Bug #42",
                Description = "Fix the login bug",
                StartDate = now,
                DueDate = now.AddDays(2),
                Status = TaskWorkflowStatus.Done,
                Priority = TaskPriority.High,
                Attachments = new List<IFormFile>()
            },
            new TaskDto
            {
                Id = 102,
                ProjectId = 203,
                ProjectTitle = "Project Y",
                Title = "Write Tests",
                Description = "Write unit tests for HomeController",
                StartDate = now.AddDays(-1),
                DueDate = now.AddDays(3),
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Attachments = new List<IFormFile>()
            }
                },
                totalCount: 2,
                pageIndex: 1,
                pageSize: 10
            );

            _taskServiceMock.Setup(x => x.GetUserTaskAsync(5, 1, 10)).ReturnsAsync(taskList);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HomeViewModel>(viewResult.Model);

            Assert.Equal(3, model.TotalProjects);
            Assert.Equal(2, model.TotalTasks);
            Assert.Equal(1, model.CompletedTasks);
            Assert.Equal(1, model.PageIndex);
            Assert.Equal(10, model.PageSize);
            Assert.Equal(2, model.TotalItems);
            Assert.Equal(1, model.TotalPages);
            Assert.Equal(2, model.UserTasks?.Count);

            var firstTask = model.UserTasks![0];
            Assert.Equal(101, firstTask.Id);
            Assert.Equal(202, firstTask.ProjectId);
            Assert.Equal("Project X", firstTask.ProjectTitle);
            Assert.Equal("Fix Bug #42", firstTask.Title);
            Assert.Equal("Fix the login bug", firstTask.Description);
            Assert.Equal(now, firstTask.StartDate);
            Assert.Equal(now.AddDays(2), firstTask.DueDate);
            Assert.Equal(TaskWorkflowStatus.Done, firstTask.Status);
            Assert.Equal(TaskPriority.High, firstTask.Priority);
            Assert.NotNull(firstTask.Attachments);

            var secondTask = model.UserTasks[1];
            Assert.Equal(102, secondTask.Id);
            Assert.Equal(203, secondTask.ProjectId);
            Assert.Equal("Project Y", secondTask.ProjectTitle);
            Assert.Equal("Write Tests", secondTask.Title);
            Assert.Equal("Write unit tests for HomeController", secondTask.Description);
            Assert.Equal(now.AddDays(-1), secondTask.StartDate);
            Assert.Equal(now.AddDays(3), secondTask.DueDate);
            Assert.Equal(TaskWorkflowStatus.ToDo, secondTask.Status);
            Assert.Equal(TaskPriority.Medium, secondTask.Priority);
            Assert.NotNull(secondTask.Attachments);
        }
    }

}
