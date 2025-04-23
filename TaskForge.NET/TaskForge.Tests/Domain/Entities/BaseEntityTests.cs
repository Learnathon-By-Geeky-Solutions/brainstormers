using Moq;
using TaskForge.Domain.Entities.Common;
using Xunit;

namespace TaskForge.Tests.Domain.Entities
{
    public class BaseEntityTests
    {
        [Fact]
        public void Constructor_ShouldInitializeDefaults()
        {
            var entity = new Mock<BaseEntity>().Object;

            Assert.Equal(string.Empty, entity.CreatedBy);
            Assert.False(entity.IsDeleted);
            Assert.True(entity.CreatedDate <= DateTime.UtcNow);
            Assert.True(entity.CreatedDate > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            var entity = new Mock<BaseEntity>().Object;

            entity.Id = 42;
            entity.CreatedBy = "TestUser";
            entity.UpdatedBy = "Updater";
            var testDate = new DateTime(2023, 4, 15, 10, 30, 0, DateTimeKind.Utc);
            entity.UpdatedDate = testDate;

            Assert.Equal(42, entity.Id);
            Assert.Equal("TestUser", entity.CreatedBy);
            Assert.Equal("Updater", entity.UpdatedBy);
            Assert.Equal(testDate, entity.UpdatedDate);
        }
    }
}
