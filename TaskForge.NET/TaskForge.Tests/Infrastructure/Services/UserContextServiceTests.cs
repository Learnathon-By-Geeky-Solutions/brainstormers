using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;
using TaskForge.Infrastructure.Services;
using Xunit;

namespace TaskForge.Tests.Infrastructure.Services
{
    public class UserContextServiceTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly UserContextService _userContextService;

        public UserContextServiceTests()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _userContextService = new UserContextService(_userManagerMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task GetCurrentUserIdAsync_ReturnsEmpty_WhenHttpContextUserIsNull()
        {
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

            var result = await _userContextService.GetCurrentUserIdAsync();

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetCurrentUserIdAsync_ReturnsEmpty_WhenUserNotFound()
        {
            var user = new ClaimsPrincipal();
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(contextMock.Object);
            _userManagerMock.Setup(x => x.GetUserAsync(user)).ReturnsAsync((IdentityUser?)null);

            var result = await _userContextService.GetCurrentUserIdAsync();

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetCurrentUserIdAsync_ReturnsUserId_WhenUserFound()
        {
            var user = new ClaimsPrincipal();
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(contextMock.Object);

            var identityUser = new IdentityUser { Id = "user123" };
            _userManagerMock.Setup(x => x.GetUserAsync(user)).ReturnsAsync(identityUser);

            var result = await _userContextService.GetCurrentUserIdAsync();

            Assert.Equal("user123", result);
        }
    }
}
