using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using Xunit;
namespace TaskForge.Tests.Infrastructure.Data
{
    public class ApplicationDbContextTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public ApplicationDbContextTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void Can_Create_Database_Context()
        {
            using var context = new ApplicationDbContext(_dbContextOptions);
            Assert.NotNull(context);
        }

        [Fact]
        public void Can_Add_And_Retrieve_Entities()
        {
            using var context = new ApplicationDbContext(_dbContextOptions);

            var user = new IdentityUser { UserName = "testuser", Email = "test@example.com" };
            context.Users.Add(user);

            context.SaveChanges();

            var profile = new UserProfile { FullName = "John Doe", UserId = user.Id, User = user };
            context.UserProfiles.Add(profile);

            var project = new Project { Title = "Test Project" };
            context.Projects.Add(project);

            var taskItem = new TaskItem { Title = "Test Task", Project = project };
            context.TaskItems.Add(taskItem);

            context.SaveChanges();

            Assert.Single(context.Users);
            Assert.Single(context.UserProfiles);
            Assert.Single(context.Projects);
            Assert.Single(context.TaskItems);
        }

        [Fact]
        public void OnModelCreating_ConfiguresIdentityTables()
        {
            using var context = new ApplicationDbContext(_dbContextOptions);
            var model = context.Model;

            Assert.Contains(model.GetEntityTypes(), t => t.GetTableName() == "Users");
            Assert.Contains(model.GetEntityTypes(), t => t.GetTableName() == "Roles");
            Assert.Contains(model.GetEntityTypes(), t => t.GetTableName() == "UserRoles");
            Assert.Contains(model.GetEntityTypes(), t => t.GetTableName() == "UserClaims");
            Assert.Contains(model.GetEntityTypes(), t => t.GetTableName() == "UserLogins");
            Assert.Contains(model.GetEntityTypes(), t => t.GetTableName() == "UserTokens");
            Assert.Contains(model.GetEntityTypes(), t => t.GetTableName() == "RoleClaims");
        }

        [Fact]
        public void OnModelCreating_ConfiguresTaskDependencyKeysAndRelationships()
        {
            using var context = new ApplicationDbContext(_dbContextOptions);
            var entity = context.Model.FindEntityType(typeof(TaskDependency));
            Assert.NotNull(entity);
            Assert.Equal(2, entity.FindPrimaryKey()!.Properties.Count);
        }
    }
}
