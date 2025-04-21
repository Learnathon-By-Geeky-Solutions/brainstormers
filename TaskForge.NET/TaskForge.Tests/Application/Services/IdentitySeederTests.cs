using Microsoft.AspNetCore.Identity;
using Moq;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Services;
using Xunit;

namespace TaskForge.Tests.Application.Services
{
    public class IdentitySeederTests
    {
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<IUserProfileService> _userProfileServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<IdentitySeeder>> _loggerMock;

        private readonly IdentitySeeder _identitySeeder;

        public IdentitySeederTests()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);

            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            _userProfileServiceMock = new Mock<IUserProfileService>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<IdentitySeeder>>();

            _identitySeeder = new IdentitySeeder(
                _roleManagerMock.Object,
                _userManagerMock.Object,
                _userProfileServiceMock.Object,
                _configurationMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task SeedRolesAndSuperUser_ShouldCreateMissingRoles()
        {
            _roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            _configurationMock.Setup(c => c["SuperAdmin:Email"])
                .Returns("admin@test.com");
            _configurationMock.Setup(c => c["SuperAdmin:Password"])
                .Returns("Pass@123");

            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(null as IdentityUser);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<IdentityUser>(), "Admin"))
                .ReturnsAsync(IdentityResult.Success);
            _userProfileServiceMock.Setup(u => u.GetByUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync(0);

            await _identitySeeder.SeedRolesAndSuperUser();

            _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Exactly(3));
        }

        [Fact]
        public async Task SeedRolesAndSuperUser_ShouldNotCreateRoleIfExists()
        {
            _roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            await _identitySeeder.SeedRolesAndSuperUser();

            _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
        }

        [Fact]
        public async Task SeedRolesAndSuperUser_ShouldLogWarning_WhenSuperAdminCredentialsMissing()
        {
            _configurationMock.Setup(c => c["SuperAdmin:Email"]).Returns((string?)null);
            _configurationMock.Setup(c => c["SuperAdmin:Password"]).Returns((string?)null);

            await _identitySeeder.SeedRolesAndSuperUser();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _loggerMock.Verify(
                static x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Super admin email or password is not set")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Fact]
        public async Task SeedRolesAndSuperUser_ShouldCreateSuperAdmin_WhenUserNotExists()
        {
            var email = "admin@test.com";
            var password = "Password@123";

            _configurationMock.Setup(c => c["SuperAdmin:Email"])
                .Returns(email);
            _configurationMock.Setup(c => c["SuperAdmin:Password"])
                .Returns(password);
            _roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock.Setup(u => u.FindByEmailAsync(email)).ReturnsAsync(null as IdentityUser);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<IdentityUser>(), "Admin"))
                .ReturnsAsync(IdentityResult.Success);
            _userProfileServiceMock.Setup(u => u.GetByUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync(0);

            await _identitySeeder.SeedRolesAndSuperUser();

            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<IdentityUser>(), password), Times.Once);
            _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<IdentityUser>(), "Admin"), Times.Once);
            _userProfileServiceMock.Verify(u => u.CreateUserProfileAsync(It.IsAny<string>(), "Super Admin"), Times.Once);
        }

        [Fact]
        public async Task SeedRolesAndSuperUser_ShouldLog_WhenSuperAdminAlreadyExists()
        {
            var existingUser = new IdentityUser { Id = "admin-id", Email = "admin@test.com" };

            _configurationMock.Setup(c => c["SuperAdmin:Email"]).Returns(existingUser.Email);
            _configurationMock.Setup(c => c["SuperAdmin:Password"]).Returns("Password@123");

            _roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock.Setup(u => u.FindByEmailAsync(existingUser.Email))
                .ReturnsAsync(existingUser);
            _userProfileServiceMock.Setup(u => u.GetByUserIdAsync(existingUser.Id))
                .ReturnsAsync(1);

            await _identitySeeder.SeedRolesAndSuperUser();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Super admin user already exists.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            _userProfileServiceMock.Verify(u => u.CreateUserProfileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SeedRolesAndSuperUser_ShouldCreateUserProfile_WhenMissing()
        {
            var user = new IdentityUser { Id = "123", Email = "admin@test.com" };

            _configurationMock.Setup(c => c["SuperAdmin:Email"])
                .Returns(user.Email);
            _configurationMock.Setup(c => c["SuperAdmin:Password"])
                .Returns("Password@123");
            _roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock.Setup(u => u.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            _userProfileServiceMock.Setup(u => u.GetByUserIdAsync(user.Id))
                .ReturnsAsync(0);

            await _identitySeeder.SeedRolesAndSuperUser();

            _userProfileServiceMock.Verify(u => u.CreateUserProfileAsync(user.Id, "Super Admin"), Times.Once);
        }
    }
}
