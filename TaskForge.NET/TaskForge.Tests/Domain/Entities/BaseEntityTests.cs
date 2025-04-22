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
            Assert.True((DateTime.UtcNow - entity.CreatedDate).TotalSeconds < 5);
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            var entity = new Mock<BaseEntity>().Object;

            entity.Id = 42;
            entity.CreatedBy = "TestUser";
            entity.UpdatedBy = "Updater";
            entity.UpdatedDate = DateTime.UtcNow;

            Assert.Equal(42, entity.Id);
            Assert.Equal("TestUser", entity.CreatedBy);
            Assert.Equal("Updater", entity.UpdatedBy);
            Assert.NotNull(entity.UpdatedDate);
        }
    }
}
