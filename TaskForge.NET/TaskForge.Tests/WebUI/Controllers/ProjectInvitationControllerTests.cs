using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.WebUI.Controllers;
using TaskForge.WebUI.Models;
using Xunit;

namespace TaskForge.Tests.WebUI.Controllers
{
    public class ProjectInvitationControllerTests
    {
        private readonly Mock<IProjectInvitationService> _invitationServiceMock = new();
        private readonly Mock<IProjectMemberService> _projectMemberServiceMock = new();
        private readonly Mock<IUserProfileService> _userProfileServiceMock = new();
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly ProjectInvitationController _controller;

        public ProjectInvitationControllerTests()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _userManagerMock = new Mock<UserManager<IdentityUser>>(store.Object,
                null, null, null, null, null, null, null, null);

            _controller = new ProjectInvitationController(
                _invitationServiceMock.Object,
                _projectMemberServiceMock.Object,
                _userProfileServiceMock.Object,
                _userManagerMock.Object);

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        [Fact]
        public async Task Index_ReturnsRedirect_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("Any", "Error");

            var result = await _controller.Index();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
        [Fact]
        public async Task Index_ReturnsRedirect_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("PageIndex", "Invalid");

            // Act
            var result = await _controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
        [Fact]
        public async Task Index_RedirectsToHome_WhenUserProfileIsNull()
        {
            var user = new IdentityUser { Id = "user123" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _userProfileServiceMock.Setup(x => x.GetByUserIdAsync(user.Id))
                .ReturnsAsync((int?)null);

            var result = await _controller.Index();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal("User profile not found.", _controller.TempData["ErrorMessage"]);
        }
        [Fact]
        public async Task Index_ReturnsViewWithNoInvitationsMessage_WhenNoInvitationsExist()
        {
            var user = new IdentityUser { Id = "user123" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _userProfileServiceMock.Setup(x => x.GetByUserIdAsync(user.Id))
                .ReturnsAsync(5);

            var paginatedList = new PaginatedList<ProjectInvitation>(new List<ProjectInvitation>(), 0, 1, 10);
            _invitationServiceMock.Setup(x => x.GetInvitationsForUserAsync(5, 1, 10))
                .ReturnsAsync(paginatedList);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectInvitationListViewModel>(viewResult.Model);
            Assert.Empty(model.Invitations);
            Assert.Equal("You have no pending invitations.", viewResult.ViewData["NoInvitationsMessage"]);
        }
        [Fact]
        public async Task Index_InvalidModelState_RedirectsToIndex()
        {
            _controller.ModelState.AddModelError("test", "error");

            var result = await _controller.Index();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        [Fact]
        public async Task Index_ValidInvitations_ReturnsViewWithModel()
        {
            var user = new IdentityUser { Id = "user123" };
            _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _userProfileServiceMock.Setup(s => s.GetByUserIdAsync("user123"))
                .ReturnsAsync(1);

            var invitation = new ProjectInvitation
            {
                Id = 5,
                Project = new Project { Title = "Test Project" },
                Status = InvitationStatus.Pending,
                AssignedRole = ProjectRole.Admin,
                InvitationSentDate = DateTime.UtcNow
            };
            var list = new PaginatedList<ProjectInvitation>(new[] { invitation }, 1, 1, 10);

            _invitationServiceMock.Setup(s => s.GetInvitationsForUserAsync(1, 1, 10))
                .ReturnsAsync(list);

            var result = await _controller.Index(1, 10);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectInvitationListViewModel>(viewResult.Model);
            Assert.Single(model.Invitations);
            Assert.Equal("Test Project", model.Invitations[0].ProjectTitle);
        }
        [Fact]
        public async Task Index_ReturnsViewWithInvitations_WhenInvitationsExist()
        {
            var user = new IdentityUser { Id = "user123" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _userProfileServiceMock.Setup(x => x.GetByUserIdAsync(user.Id))
                .ReturnsAsync(10);

            var invitations = new List<ProjectInvitation>
            {
                new ProjectInvitation
                {
                    Id = 1,
                    Project = new Project { Title = "Project A" },
                    Status = InvitationStatus.Pending,
                    AssignedRole = ProjectRole.Contributor,
                    InvitationSentDate = DateTime.UtcNow
                }
            };

            var paginatedList = new PaginatedList<ProjectInvitation>(invitations, 1, 2, 10);
            _invitationServiceMock.Setup(x => x.GetInvitationsForUserAsync(10, 2, 10))
                .ReturnsAsync(paginatedList);

            var result = await _controller.Index(pageIndex: 2, pageSize: 10);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectInvitationListViewModel>(viewResult.Model);

            Assert.Single(model.Invitations);
            Assert.Equal(10, model.PageSize);
            Assert.Equal(2, model.PageIndex);
            Assert.Equal(1, model.TotalItems);
            Assert.Equal(1, model.TotalPages);
            Assert.Equal("Project A", model.Invitations[0].ProjectTitle);
            Assert.Equal("Pending", model.Invitations[0].Status);
            Assert.Equal("Contributor", model.Invitations[0].Role);
        }



        [Fact]
        public async Task Create_ReturnsRedirect_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("InvitedUserEmail", "Required");

            var viewModel = new InviteViewModel
            {
                ProjectId = 1,
                InvitedUserEmail = "",
                AssignedRole = ProjectRole.Contributor
            };

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
        [Fact]
        public async Task Create_ReturnsUnauthorized_WhenUserIsNull()
        {
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((IdentityUser?)null);

            var viewModel = new InviteViewModel
            {
                ProjectId = 1,
                InvitedUserEmail = "user@example.com",
                AssignedRole = ProjectRole.Viewer
            };

            var result = await _controller.Create(viewModel);

            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task Create_ReturnsForbid_WhenUserIsNotAdmin()
        {
            var user = new IdentityUser { Id = "user123" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, 1))
                .ReturnsAsync(new ProjectMemberDto { Role = ProjectRole.Contributor });

            var viewModel = new InviteViewModel
            {
                ProjectId = 1,
                InvitedUserEmail = "user@example.com",
                AssignedRole = ProjectRole.Viewer
            };

            var result = await _controller.Create(viewModel);

            Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task Create_SetsErrorMessageAndRedirects_WhenInvitationFails()
        {
            var user = new IdentityUser { Id = "user123" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, 1))
                .ReturnsAsync(new ProjectMemberDto { Role = ProjectRole.Admin });

            _invitationServiceMock.Setup(x => x.AddAsync(1, "fail@example.com", ProjectRole.Viewer))
                .ReturnsAsync(ServiceResult.FailureResult("Email not found"));

            var viewModel = new InviteViewModel
            {
                ProjectId = 1,
                InvitedUserEmail = "fail@example.com",
                AssignedRole = ProjectRole.Viewer
            };

            var result = await _controller.Create(viewModel);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageMembers", redirect.ActionName);
            Assert.Equal("Project", redirect.ControllerName);
            Assert.Equal("Failed to send invitation: Email not found", _controller.TempData["ErrorMessage"]);
        }
        [Fact]
        public async Task Create_SetsSuccessMessageAndRedirects_WhenInvitationSucceeds()
        {
            var user = new IdentityUser { Id = "user123" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, 1))
                .ReturnsAsync(new ProjectMemberDto { Role = ProjectRole.Admin });

            _invitationServiceMock.Setup(x => x.AddAsync(1, "success@example.com", ProjectRole.Viewer))
                .ReturnsAsync(new ServiceResult { Success = true });

            var viewModel = new InviteViewModel
            {
                ProjectId = 1,
                InvitedUserEmail = "success@example.com",
                AssignedRole = ProjectRole.Viewer
            };

            var result = await _controller.Create(viewModel);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageMembers", redirect.ActionName);
            Assert.Equal("Project", redirect.ControllerName);
            Assert.Equal("Invitation sent to success@example.com successfully.", _controller.TempData["SuccessMessage"]);
        }



        [Fact]
        public async Task Edit_ReturnsRedirectToAction_WhenModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "Required");

            var model = new InvitationApprovalViewModel();

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Also check that TempData["ErrorMessage"] is set
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
            var errorMessage = _controller.TempData["ErrorMessage"] as string;
            Assert.Contains("Required", errorMessage);
        }
        [Fact]
        public async Task Edit_ReturnsNotFound_WhenInvitationIsNull()
        {
            var viewModel = new InvitationApprovalViewModel
            {
                Id = 1,
                ProjectId = 1,
                Status = InvitationStatus.Accepted,
            };

            _invitationServiceMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((ProjectInvitation?)null);

            var result = await _controller.Edit(viewModel);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Invitation not found.", notFound.Value);
        }
        [Theory]
        [InlineData(InvitationStatus.Accepted)]
        [InlineData(InvitationStatus.Declined)]
        [InlineData(InvitationStatus.Canceled)]
        public async Task Edit_ReturnsBadRequest_WhenInvitationStatusIsNotPending(InvitationStatus status)
        {
            var viewModel = new InvitationApprovalViewModel
            {
                Id = 1,
                ProjectId = 1,
                Status = InvitationStatus.Accepted,
            };

            _invitationServiceMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new ProjectInvitation { Id = 1, Status = status });

            var result = await _controller.Edit(viewModel);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Cannot update an invitation that is already {status.ToString().ToLower()}.", badRequest.Value);
        }
        [Fact]
        public async Task Edit_CallsUpdateAndRedirects_WhenValid()
        {
            var viewModel = new InvitationApprovalViewModel
            {
                Id = 1,
                ProjectId = 1,
                Status = InvitationStatus.Accepted,
            };

            _invitationServiceMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new ProjectInvitation { Id = 1, Status = InvitationStatus.Pending });

            var result = await _controller.Edit(viewModel);

            _invitationServiceMock.Verify(x => x.UpdateInvitationStatusAsync(1, InvitationStatus.Accepted), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

    }
}
