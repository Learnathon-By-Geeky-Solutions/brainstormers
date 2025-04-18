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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("../evil.txt")]
        [InlineData("/absolute/path/to/file.txt")]
        public async Task DeleteFileAsync_ThrowsArgumentException_OnInvalidPaths(string badPath)
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteFileAsync(badPath));
            Assert.Equal("relativePath", ex.ParamName);
        }

        [Fact]
        public async Task DeleteFileAsync_ThrowsInvalidOperationException_OnIOException()
        {
            // Arrange
            var fileName = "locked.txt";
            var fullPath = Path.Combine(_rootPath, fileName);
            await File.WriteAllTextAsync(fullPath, "some content");

            using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteFileAsync(fileName));

                // Assert
                Assert.Contains("Could not delete file", ex.Message);
                Assert.IsType<IOException>(ex.InnerException);
            }
        }

        [Fact]
        public async Task DeleteFileAsync_ThrowsInvalidOperationException_OnUnauthorizedAccessException()
        {
            // Arrange
            var fileName = "unauthorized.txt";
            var fullPath = Path.Combine(_rootPath, fileName);
            await File.WriteAllTextAsync(fullPath, "content");

            // Temporarily make the file read-only to simulate unauthorized access
            File.SetAttributes(fullPath, FileAttributes.ReadOnly);

            try
            {
                // Act
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteFileAsync(fileName));

                // Assert
                Assert.Contains("Access denied", ex.Message);
                Assert.IsType<UnauthorizedAccessException>(ex.InnerException);
            }
            finally
            {
                File.SetAttributes(fullPath, FileAttributes.Normal);
            }
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

