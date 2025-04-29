using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.WebUI.Controllers;
using TaskForge.WebUI.Models;
using Xunit;

namespace TaskForge.Tests.WebUI.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UserController(_userServiceMock.Object);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "test@example.com"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithCorrectViewModel()
        {
            // Arrange
            var users = new List<UserListItemDto>
            {
                new UserListItemDto { UserId = "1", Email = "user1@example.com" },
                new UserListItemDto { UserId = "2", Email = "user2@example.com" }
            };
            var paginatedList = new PaginatedList<UserListItemDto>(users, users.Count, 1, 10);
            _userServiceMock.Setup(x => x.GetFilteredUsersAsync(It.IsAny<UserFilterDto>(), 1, 10)).ReturnsAsync(paginatedList);
            _userServiceMock.Setup(x => x.GetAllRolesAsync()).ReturnsAsync(new List<IdentityRole>
            {
                new IdentityRole("Admin"),
                new IdentityRole("User")
            });

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<FilterUserListViewModel>(viewResult.Model);
            Assert.Equal(2, model.Users.Count);
        }
        [Fact]
        public async Task Index_ReturnsViewResult_WhenModelIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("SomeField", "Required");

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }



        [Fact]
        public async Task Create_Get_ReturnsViewWithAvailableRoles()
        {
            // Arrange
            _userServiceMock.Setup(x => x.GetAllRolesAsync()).ReturnsAsync(new List<IdentityRole>
        {
            new IdentityRole("Admin"),
            new IdentityRole("User")
        });

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserCreateViewModel>(viewResult.Model);
            Assert.NotNull(model.AvailableRoles);
            Assert.Equal(2, model.AvailableRoles.Count());
        }
        [Fact]
        public async Task Create_Post_ModelStateInvalid_ReturnsViewWithAvailableRoles()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");
            _userServiceMock.Setup(x => x.GetAllRolesAsync()).ReturnsAsync(new List<IdentityRole>
            {
                new IdentityRole("Admin"),
                new IdentityRole("User")
            });

            var model = new UserCreateViewModel();

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<UserCreateViewModel>(viewResult.Model);
            Assert.NotNull(returnedModel.AvailableRoles);
            Assert.Equal(2, returnedModel.AvailableRoles.Count());
        }
        [Fact]
        public async Task Create_Post_CreationFails_ReturnsViewWithErrors()
        {
            // Arrange
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Error1" });
            _userServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<UserCreateDto>())).ReturnsAsync(identityResult);
            _userServiceMock.Setup(x => x.GetAllRolesAsync()).ReturnsAsync(new List<IdentityRole>
            {
                new IdentityRole("Admin"),
                new IdentityRole("User")
            });

            var model = new UserCreateViewModel
            {
                Email = "test@example.com",
                Password = "password",
                Role = "Admin"
            };

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<UserCreateViewModel>(viewResult.Model);
            Assert.NotNull(returnedModel.AvailableRoles);
            Assert.Single(_controller.ModelState.Values.SelectMany(v => v.Errors));
        }
        [Fact]
        public async Task Create_Post_SuccessfulCreation_RedirectsToIndex()
        {
            // Arrange
            _userServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<UserCreateDto>())).ReturnsAsync(IdentityResult.Success);

            var model = new UserCreateViewModel
            {
                Email = "test@example.com",
                Password = "password",
                Role = "Admin"
            };

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }



        [Fact]
        public async Task Delete_Get_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            _userServiceMock.Setup(x => x.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync((UserListItemDto?)null);

            // Act
            var result = await _controller.Delete("nonexistent");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Delete_Get_UserIsSelf_ReturnsForbid()
        {
            // Arrange
            _userServiceMock.Setup(x => x.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync(new UserListItemDto
            {
                Email = "test@example.com"
            });

            // Act
            var result = await _controller.Delete("user123");

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task Delete_Get_OperatorDeletingNonUser_ReturnsForbid()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Name, "operator@example.com"),
            new Claim(ClaimTypes.Role, "Operator"),
        }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _userServiceMock.Setup(x => x.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync(new UserListItemDto
            {
                Email = "another@example.com",
                Role = "Admin"
            });

            // Act
            var result = await _controller.Delete("user123");

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task Delete_Get_ValidUser_ReturnsViewWithModel()
        {
            // Arrange
            _userServiceMock.Setup(x => x.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync(new UserListItemDto
            {
                UserId = "user123",
                Email = "another@example.com",
                Role = "User"
            });

            // Act
            var result = await _controller.Delete("user123");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserListItemViewModel>(viewResult.Model);
            Assert.Equal("user123", model.UserId);
        }
        [Fact]
        public async Task DeleteConfirmed_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            _userServiceMock.Setup(x => x.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync((UserListItemDto?)null);

            // Act
            var result = await _controller.DeleteConfirmed("nonexistent");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task DeleteConfirmed_UserIsSelf_ReturnsForbid()
        {
            // Arrange
            _userServiceMock.Setup(x => x.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync(new UserListItemDto
            {
                Email = "test@example.com"
            });

            // Act
            var result = await _controller.DeleteConfirmed("user123");

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task DeleteConfirmed_DeletionSuccess_RedirectsToIndex()
        {
            // Arrange
            _userServiceMock.Setup(x => x.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync(new UserListItemDto
            {
                Email = "another@example.com",
                Role = "User"
            });
            _userServiceMock.Setup(x => x.DeleteUserAsync(It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteConfirmed("user123");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
        [Fact]
        public async Task DeleteConfirmed_DeletionFails_RedirectsToDelete()
        {
            // Arrange
            _userServiceMock.Setup(x => x.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync(new UserListItemDto
            {
                Email = "another@example.com",
                Role = "User"
            });
            _userServiceMock.Setup(x => x.DeleteUserAsync(It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteConfirmed("user123");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Delete", redirectResult.ActionName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.Equal("user123", redirectResult.RouteValues["id"]);
        }
    }
}