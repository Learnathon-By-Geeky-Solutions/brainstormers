using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.WebUI.Controllers;
using TaskForge.WebUI.Models;
using Xunit;

namespace TaskForge.Tests.WebUI.Controllers
{
    public class UserProfileControllerTests
    {
        private readonly Mock<IUserProfileService> _userProfileServiceMock;
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly UserProfileController _controller;

        public UserProfileControllerTests()
        {
            _userProfileServiceMock = new Mock<IUserProfileService>();

            var store = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                store.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<IdentityUser>>(),
                Array.Empty<IUserValidator<IdentityUser>>(),
                Array.Empty<IPasswordValidator<IdentityUser>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<IdentityUser>>>()
            );

            _controller = new UserProfileController(_userProfileServiceMock.Object, _userManagerMock.Object);

            // Set up controller context with ClaimsPrincipal
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task Setup_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            // Arrange
            _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

            // Act
            var result = await _controller.Setup();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task Setup_ReturnsUnauthorized_WhenProfileIsNull()
        {
            // Arrange
            _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-user-id");
            _userProfileServiceMock.Setup(s => s.GetByUserIdAsync("test-user-id"))
                .ReturnsAsync((UserProfile?)null);

            // Act
            var result = await _controller.Setup();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task Setup_ReturnsViewResult_WithExpectedModel()
        {
            // Arrange
            var userId = "test-user-it";
            var viewModel = new UserProfileEditViewModel
            {
                FullName = "Test User",
                PhoneNumber = "1234567890",
                AvatarUrl = "/avatar.jpg",
                Location = "Test City",
                JobTitle = "Developer",
                Company = "Test Inc",
                ProfessionalSummary = "Experienced dev",
                LinkedInProfile = "https://linkedin.com/in/test",
                WebsiteUrl = "https://test.com"
            };
            var profile = new UserProfile
            {
                Id = 1,
                FullName = viewModel.FullName,
                PhoneNumber = viewModel.PhoneNumber,
                AvatarUrl = viewModel.AvatarUrl,
                Location = viewModel.Location,
                JobTitle = viewModel.JobTitle,
                Company = viewModel.Company,
                ProfessionalSummary = viewModel.ProfessionalSummary,
                LinkedInProfile = viewModel.LinkedInProfile,
                WebsiteUrl = viewModel.WebsiteUrl
            };

            _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(userId).Verifiable();
            _userProfileServiceMock.Setup(s => s.GetByUserIdAsync(userId))
                .ReturnsAsync(profile).Verifiable();

            // Act
            var result = await _controller.Setup();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserProfileEditViewModel>(viewResult.Model);

            Assert.Equal(viewModel.FullName, model.FullName);
            Assert.Equal(viewModel.PhoneNumber, model.PhoneNumber);
            Assert.Equal(viewModel.AvatarUrl, model.AvatarUrl);
            Assert.Equal(viewModel.Location, model.Location);
            Assert.Equal(viewModel.JobTitle, model.JobTitle);
            Assert.Equal(viewModel.Company, model.Company);
            Assert.Equal(viewModel.ProfessionalSummary, model.ProfessionalSummary);
            Assert.Equal(viewModel.LinkedInProfile, model.LinkedInProfile);
            Assert.Equal(viewModel.WebsiteUrl, model.WebsiteUrl);

            _userManagerMock.Verify(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _userProfileServiceMock.Verify(s => s.GetByUserIdAsync(userId), Times.Once);
        }
        [Fact]



        public async Task Setup_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new UserProfileEditViewModel
            {
                FullName = "Test User",
                PhoneNumber = "1234567890",
                AvatarUrl = "/avatar.jpg",
                Location = "Test City",
                JobTitle = "Developer",
                Company = "Test Inc",
                ProfessionalSummary = "Experienced dev",
                LinkedInProfile = "https://linkedin.com/in/test",
                WebsiteUrl = "https://test.com"
            };

            _controller.ModelState.AddModelError("FullName", "Required");

            // Act
            var result = await _controller.Setup(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }
        [Fact]
        public async Task Setup_Post_ReturnsRedirectToAction_WhenProfileIsNull()
        {
            // Arrange
            var model = new UserProfileEditViewModel
            {
                FullName = "Test User",
                PhoneNumber = "1234567890",
                AvatarUrl = "/avatar.jpg",
                Location = "Test City",
                JobTitle = "Developer",
                Company = "Test Inc",
                ProfessionalSummary = "Experienced dev",
                LinkedInProfile = "https://linkedin.com/in/test",
                WebsiteUrl = "https://test.com"
            };

            _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("test-user-id");
            _userProfileServiceMock.Setup(s => s.GetByUserIdAsync("test-user-id"))
                .ReturnsAsync((UserProfile?)null);

            // Act
            var result = await _controller.Setup(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Setup_Post_UpdatesProfileAndRedirects()
        {
            // Arrange
            var model = new UserProfileEditViewModel
            {
                FullName = "Test User",
                PhoneNumber = "1234567890",
                AvatarUrl = "/avatar.jpg",
                Location = "Test City",
                JobTitle = "Developer",
                Company = "Test Inc",
                ProfessionalSummary = "Experienced dev",
                LinkedInProfile = "https://linkedin.com/in/test",
                WebsiteUrl = "https://test.com"
            };

            var profile = new UserProfile
            {
                Id = 1,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                AvatarUrl = model.AvatarUrl,
                Location = model.Location,
                JobTitle = model.JobTitle,
                Company = model.Company,
                ProfessionalSummary = model.ProfessionalSummary,
                LinkedInProfile = model.LinkedInProfile,
                WebsiteUrl = model.WebsiteUrl
            };

            _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("test-user-id");
            _userProfileServiceMock.Setup(s => s.GetByUserIdAsync("test-user-id"))
                .ReturnsAsync(profile);

            // Act
            var result = await _controller.Setup(model);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            _userProfileServiceMock.Verify(s => s.UpdateAsync(profile), Times.Once);
        }
        [Fact]
        public async Task Setup_Post_UserIdIsNull_RedirectsToLogin()
        {
            // Arrange
            var model = new UserProfileEditViewModel();
            _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

            // Act
            var result = await _controller.Setup(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
        }
        [Fact]
        public async Task Setup_Post_AvatarTooLarge_ReturnsModelError()
        {
            // Arrange
            var model = new UserProfileEditViewModel
            {
                AvatarImage = MockFormFile("test.jpg", length: 11 * 1024 * 1024)
            };

            _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");
            _userProfileServiceMock.Setup(s => s.GetByUserIdAsync("user123")).ReturnsAsync(new UserProfile());

            // Act
            var result = await _controller.Setup(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var solution = Assert.IsType<UserProfileEditViewModel>(viewResult.Model);
            Assert.Equal(solution, viewResult.Model);
            Assert.True(_controller.ModelState.ContainsKey("AvatarImage"));
        }
        [Fact]
        public async Task Setup_Post_ProfileNotFound_ReturnsNotFound()
        {
            // Arrange
            _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");
            _userProfileServiceMock.Setup(s => s.GetByUserIdAsync("user123")).ReturnsAsync((UserProfile?)null);

            // Act
            var result = await _controller.Setup(new UserProfileEditViewModel());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Setup_Post_InvalidAvatarExtension_ReturnsModelError()
        {
            // Arrange
            var model = new UserProfileEditViewModel
            {
                AvatarImage = MockFormFile("malware.exe", length: 1024)
            };

            _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");
            _userProfileServiceMock.Setup(s => s.GetByUserIdAsync("user123")).ReturnsAsync(new UserProfile());

            // Act
            var result = await _controller.Setup(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var solution = Assert.IsType<UserProfileEditViewModel>(viewResult.Model);
            Assert.Equal(solution, viewResult.Model);
            Assert.True(_controller.ModelState.ContainsKey("AvatarImage"));
        }
        [Fact]
        public async Task Setup_Post_ThrowsException_ReturnsJsonError()
        {
            // Arrange
            var model = new UserProfileEditViewModel
            {
                FullName = "Test User",
                PhoneNumber = "1234567890",
                AvatarUrl = "/avatar.jpg",
                Location = "Test City",
                JobTitle = "Developer",
                Company = "Test Inc",
                ProfessionalSummary = "Experienced dev",
                LinkedInProfile = "https://linkedin.com/in/test",
                WebsiteUrl = "https://test.com"
            };

            var profile = new UserProfile
            {
                Id = 1,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                AvatarUrl = model.AvatarUrl,
                Location = model.Location,
                JobTitle = model.JobTitle,
                Company = model.Company,
                ProfessionalSummary = model.ProfessionalSummary,
                LinkedInProfile = model.LinkedInProfile,
                WebsiteUrl = model.WebsiteUrl
            };

            _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("test-user-id");
            _userProfileServiceMock.Setup(s => s.GetByUserIdAsync("test-user-id"))
                .ReturnsAsync(profile);
            _userProfileServiceMock.Setup(s => s.UpdateAsync(profile))
                .ThrowsAsync(new Exception("Update failed"));

            // Act
            var result = await _controller.Setup(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("Update failed", jsonResult.Value);
        }
        [Fact]
        public async Task Setup_Post_ValidAvatar_UpdatesProfileAndRedirects()
        {
            // Arrange
            var userId = "user123";
            var profile = new UserProfile { Id = 1 };
            var file = MockFormFile("avatar.jpg", length: 1024);

            var model = new UserProfileEditViewModel
            {
                FullName = "New User",
                AvatarImage = file
            };

            _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            _userProfileServiceMock.Setup(s => s.GetByUserIdAsync(userId)).ReturnsAsync(profile);

            // Act
            var result = await _controller.Setup(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Setup", redirect.ActionName);
            Assert.Equal("UserProfile", redirect.ControllerName);
            _userProfileServiceMock.Verify(s => s.UpdateAsync(It.Is<UserProfile>(p => p.FullName == "New User")), Times.Once);
        }


        private static IFormFile MockFormFile(string fileName, long length)
        {
            var content = new MemoryStream(new byte[length]);
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(length);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns(Task.CompletedTask);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.OpenReadStream()).Returns(content);
            return fileMock.Object;
        }

    }
}
