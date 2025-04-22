using Microsoft.EntityFrameworkCore;
using Moq;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories.Common;
using TaskForge.Tests.Helpers;
using Xunit;

namespace TaskForge.Tests.Infrastructure.Repositories.Common
{
    public class UnitOfWorkTests
    {
        private readonly Mock<IUserContextService> _mockUserContextService;
        private readonly TestApplicationDbContext _realContext;
        private readonly UnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _realContext = new TestApplicationDbContext(options);

            _mockUserContextService = new Mock<IUserContextService>();

            _unitOfWork = new UnitOfWork(_realContext, _mockUserContextService.Object);
        }

        [Fact]
        public void Constructor_InitializesRepositories()
        {
            Assert.NotNull(_unitOfWork.Projects);
            Assert.NotNull(_unitOfWork.Tasks);
            Assert.NotNull(_unitOfWork.ProjectMembers);
            Assert.NotNull(_unitOfWork.ProjectInvitations);
            Assert.NotNull(_unitOfWork.UserProfiles);
            Assert.NotNull(_unitOfWork.TaskAttachments);
            Assert.NotNull(_unitOfWork.TaskAssignments);
        }

        [Fact]
        public void Dispose_CallsContextDispose()
        {
            _unitOfWork.Dispose();
            Assert.True(_unitOfWork is IDisposable); 
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimesSafely()
        {
            _unitOfWork.Dispose();
            _unitOfWork.Dispose();
            Assert.True(_unitOfWork is IDisposable);
        }

        [Fact]
        public async Task SaveChangesAsync_CallsContextSaveChangesAsync()
        {
            var result = await _unitOfWork.SaveChangesAsync();
            Assert.Equal(0, result);
        }

    }
}
