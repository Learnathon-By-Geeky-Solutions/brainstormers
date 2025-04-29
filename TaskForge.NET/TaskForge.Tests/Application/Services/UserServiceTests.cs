using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Services;
using TaskForge.Domain.Entities;
using Xunit;

namespace TaskForge.Tests.Services
{
	public class UserServiceTests
	{
		private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
		private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
		private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;
		private readonly Mock<IUnitOfWork> _unitOfWorkMock;
		private readonly UserService _userService;
		private readonly Mock<IUserRepository> _userRepositoryMock;

		public UserServiceTests()
		{
			var userStoreMock = new Mock<IUserStore<IdentityUser>>();
			_userManagerMock = new Mock<UserManager<IdentityUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

			var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
			_roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null, null, null, null);

			_userRepositoryMock = new Mock<IUserRepository>();
			_userProfileRepositoryMock = new Mock<IUserProfileRepository>();
			_unitOfWorkMock = new Mock<IUnitOfWork>();

			_userService = new UserService(
				_userManagerMock.Object,
				_roleManagerMock.Object,
				_userRepositoryMock.Object,
				_userProfileRepositoryMock.Object,
				_unitOfWorkMock.Object
			);
		}

		[Fact]
		public async Task GetUserByIdAsync_UserExists_ReturnsUser()
		{
			// Arrange
			var userId = "test-user-id";
			var identityUser = new IdentityUser
			{
				Id = userId,
				Email = "test@example.com",
			};

			var userProfile = new UserProfile
			{
				UserId = userId,
				FullName = "Test User",
				User = identityUser // Set User properly!
			};

			_userProfileRepositoryMock
				.Setup(repo => repo.FindByExpressionAsync(
					It.IsAny<Expression<Func<UserProfile, bool>>>(),
					It.IsAny<Func<IQueryable<UserProfile>, IOrderedQueryable<UserProfile>>>(),
					It.IsAny<Func<IQueryable<UserProfile>, IQueryable<UserProfile>>>(),
					null,
					null))
				.ReturnsAsync(new List<UserProfile> { userProfile });

			_userManagerMock
				.Setup(um => um.GetRolesAsync(identityUser))
				.ReturnsAsync(new List<string> { "Admin" });

			// Act
			var result = await _userService.GetUserByIdAsync(userId);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(userId, result.UserId);
			Assert.Equal("test@example.com", result.Email);
			Assert.Equal("Test User", result.FullName);
			Assert.Equal("Admin", result.Role);
		}


		[Fact]
		public async Task GetUserByIdAsync_UserDoesNotExist_ReturnsNull()
		{
			// Arrange
			_userProfileRepositoryMock
				.Setup(repo => repo.FindByExpressionAsync(It.IsAny<Expression<Func<UserProfile, bool>>>(), null, null, null, null))
				.ReturnsAsync(new List<UserProfile>());

			// Act
			var result = await _userService.GetUserByIdAsync("non-existing-id");

			// Assert
			Assert.Null(result);
		}



		[Fact]
		public async Task CreateUserAsync_ValidUser_ReturnsSuccess()
		{
			// Arrange
			var userCreateDto = new UserCreateDto
			{
				Email = "newuser@example.com",
				FullName = "New User",
				Password = "Password123!",
				PhoneNumber = "1234567890",
				Role = "User"
			};

			_userManagerMock
				.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Success);

			_userManagerMock
				.Setup(um => um.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Success);

			// Act
			var result = await _userService.CreateUserAsync(userCreateDto);

			// Assert
			Assert.True(result.Succeeded);
			_userProfileRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<UserProfile>()), Times.Once);
			_unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
		}



		[Fact]
		public async Task DeleteUserAsync_UserExists_ReturnsTrue()
		{
			// Arrange
			var user = new IdentityUser { Id = "user-id" };

			_userManagerMock
				.Setup(um => um.FindByIdAsync(user.Id))
				.ReturnsAsync(user);

			_userManagerMock
				.Setup(um => um.DeleteAsync(user))
				.ReturnsAsync(IdentityResult.Success);

			// Act
			var result = await _userService.DeleteUserAsync(user.Id);

			// Assert
			Assert.True(result);
		}
		[Fact]
		public async Task DeleteUserAsync_UserDoesNotExist_ReturnsFalse()
		{
			// Arrange
			_userManagerMock
				.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
				.ReturnsAsync((IdentityUser)null);

			// Act
			var result = await _userService.DeleteUserAsync("non-existing-id");

			// Assert
			Assert.False(result);
		}


		[Fact]
		public async Task AssignRoleAsync_UserExists_AssignsRole()
		{
			// Arrange
			var userId = "user-id";
			var user = new IdentityUser { Id = userId };

			_userManagerMock
				.Setup(um => um.FindByIdAsync(userId))
				.ReturnsAsync(user);

			_userManagerMock
				.Setup(um => um.GetRolesAsync(user))
				.ReturnsAsync(new List<string> { "OldRole" });

			_userManagerMock
				.Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
				.ReturnsAsync(IdentityResult.Success);

			_userManagerMock
				.Setup(um => um.AddToRoleAsync(user, "NewRole"))
				.ReturnsAsync(IdentityResult.Success);

			// Act
			var result = await _userService.AssignRoleAsync(userId, "NewRole");

			// Assert
			Assert.True(result);
		}
		[Fact]
		public async Task AssignRoleAsync_UserDoesNotExist_ReturnsFalse()
		{
			// Arrange
			_userManagerMock
				.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
				.ReturnsAsync((IdentityUser)null);

			// Act
			var result = await _userService.AssignRoleAsync("non-existing-id", "Role");

			// Assert
			Assert.False(result);
		}
	}
}
