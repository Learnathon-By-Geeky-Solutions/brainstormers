using Microsoft.AspNetCore.Hosting;
using Moq;
using TaskForge.Application.Services;
using Xunit;

namespace TaskForge.Tests.Services
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    public class FileServiceTests : IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly FileService _service;
        private readonly string _rootPath;

        public FileServiceTests()
        {
            _rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootPath);

            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.WebRootPath).Returns(_rootPath);

            _service = new FileService(_mockEnv.Object);
        }

        [Fact]
        public async Task DeleteFileAsync_ThrowsArgumentException_ForInvalidPath()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteFileAsync("../somefile.txt"));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteFileAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteFileAsync("C:/absolute/path.txt"));
        }

        [Fact]
        public async Task DeleteFileAsync_DoesNothing_WhenFileDoesNotExist()
        {
            // Arrange
            var relativePath = "nonexistentfile.txt";
            var fullPath = Path.Combine(_rootPath, relativePath);
            Assert.False(File.Exists(fullPath));

            // Act
            await _service.DeleteFileAsync(relativePath);

            // Assert - Should not throw
            Assert.False(File.Exists(fullPath));
        }

        [Fact]
        public async Task DeleteFileAsync_DeletesExistingFile()
        {
            // Arrange
            var fileName = "test.txt";
            var fullPath = Path.Combine(_rootPath, fileName);
            await File.WriteAllTextAsync(fullPath, "Hello World");
            Assert.True(File.Exists(fullPath));

            // Act
            await _service.DeleteFileAsync(fileName);

            // Assert
            Assert.False(File.Exists(fullPath));
        }

        [Fact]
        public async Task DeleteFileAsync_ThrowsInvalidOperation_ForIOException()
        {
            // Arrange
            var fileName = "locked.txt";
            var fullPath = Path.Combine(_rootPath, fileName);
            await File.WriteAllTextAsync(fullPath, "data");

            using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.None);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteFileAsync(fileName));

            // Assert
            Assert.Contains("Could not delete file", exception.Message);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_rootPath))
                    Directory.Delete(_rootPath, recursive: true);
            }
            catch
            {
                // Ignore cleanup failures
            }
        }
    }
}

