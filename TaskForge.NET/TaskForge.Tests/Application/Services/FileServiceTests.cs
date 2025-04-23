using Moq;
using TaskForge.Application.Services;
using Xunit;

namespace TaskForge.Tests.Application.Services
{
    public class FileServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly FileService _fileService;
        private readonly string _webRootPath;

        public FileServiceTests()
        {
            _mockEnv = new Mock<IWebHostEnvironment>();
            _webRootPath = Path.Combine(Path.GetTempPath(), "TaskForgeTestFiles");
            Directory.CreateDirectory(_webRootPath);

            _mockEnv.Setup(e => e.WebRootPath).Returns(_webRootPath);

            _fileService = new FileService(_mockEnv.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("../secret.txt")]
        [InlineData("..\\secret.txt")]
        [InlineData("C:\\absolute\\path.txt")]
        [InlineData("/etc/passwd")]
        public async Task DeleteFileAsync_InvalidPaths_ThrowsArgumentException(string? invalidPath)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _fileService.DeleteFileAsync(invalidPath!));
        }

        [Fact]
        public async Task DeleteFileAsync_FileExists_DeletesSuccessfully()
        {
            // Arrange
            var relativePath = "valid-file.txt";
            var fullPath = Path.Combine(_webRootPath, relativePath);
            await File.WriteAllTextAsync(fullPath, "test data");

            // Act
            await _fileService.DeleteFileAsync(relativePath);

            // Assert
            Assert.False(File.Exists(fullPath));
        }

        [Fact]
        public async Task DeleteFileAsync_FileDoesNotExist_DoesNotThrow()
        {
            // Act + Assert
            var ex = await Record.ExceptionAsync(() =>
                _fileService.DeleteFileAsync("nonexistent-file.txt"));
            Assert.Null(ex);
        }

        [Fact]
        public async Task DeleteFileAsync_ThrowsIOException_WrappedInInvalidOperation()
        {
            // Arrange
            var relativePath = "io-error.txt";
            var fullPath = Path.Combine(_webRootPath, relativePath);
            await File.WriteAllTextAsync(fullPath, "test data");

            // Lock the file to simulate IOException
            using var stream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.None);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _fileService.DeleteFileAsync(relativePath));

            Assert.IsType<IOException>(ex.InnerException);
        }

        [Fact]
        public async Task DeleteFileAsync_ThrowsUnauthorizedAccess_WrappedInInvalidOperation()
        {
            // Arrange
            var relativePath = "unauthorized.txt";
            var fullPath = Path.Combine(_webRootPath, relativePath);
            await File.WriteAllTextAsync(fullPath, "test data");

            // Make file read-only to simulate UnauthorizedAccessException
            File.SetAttributes(fullPath, FileAttributes.ReadOnly);

            try
            {
                // Act & Assert
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _fileService.DeleteFileAsync(relativePath));

                Assert.IsType<UnauthorizedAccessException>(ex.InnerException);
            }
            finally
            {
                File.SetAttributes(fullPath, FileAttributes.Normal);
                File.Delete(fullPath);
            }
        }
    }
}
