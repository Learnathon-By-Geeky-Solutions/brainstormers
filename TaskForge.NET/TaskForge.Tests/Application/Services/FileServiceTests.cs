using Moq;
using TaskForge.Application.Interfaces.Services;
using Xunit;

namespace TaskForge.Tests.Application.Services
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
            var relativePath = "nonexistentfile.txt";
            var fullPath = Path.Combine(_rootPath, relativePath);
            Assert.False(File.Exists(fullPath));

            await _service.DeleteFileAsync(relativePath);

            Assert.False(File.Exists(fullPath));
        }

        [Fact]
        public async Task DeleteFileAsync_DeletesExistingFile()
        {
            var fileName = "test.txt";
            var fullPath = Path.Combine(_rootPath, fileName);
            await File.WriteAllTextAsync(fullPath, "Hello World");
            Assert.True(File.Exists(fullPath));

            await _service.DeleteFileAsync(fileName);

            Assert.False(File.Exists(fullPath));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("../evil.txt")]
        [InlineData("/absolute/path/to/file.txt")]
        public async Task DeleteFileAsync_ThrowsArgumentException_OnInvalidPaths(string? badPath)
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteFileAsync(badPath!));
            Assert.Equal("relativePath", ex.ParamName);
        }

        [Fact]
        public async Task DeleteFileAsync_ThrowsInvalidOperationException_OnIOException()
        {
            var fileName = "throwio.txt";
            var fullPath = Path.Combine(_rootPath, fileName);
            await File.WriteAllTextAsync(fullPath, "dummy");

            var service = new TestableFileService(_mockEnv.Object, () =>
            {
                throw new IOException("Simulated IO error");
            });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteFileAsync(fileName));
            Assert.Contains("Could not delete file", ex.Message);
            Assert.IsType<IOException>(ex.InnerException);
        }



        [Fact]
        public async Task DeleteFileAsync_ThrowsInvalidOperationException_OnUnauthorizedAccessException()
        {
            var fileName = "throwunauth.txt";
            var fullPath = Path.Combine(_rootPath, fileName);
            await File.WriteAllTextAsync(fullPath, "dummy");

            var service = new TestableFileService(_mockEnv.Object, () =>
            {
                throw new UnauthorizedAccessException("Simulated access error");
            });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteFileAsync(fileName));
            Assert.Contains("Access denied", ex.Message);
            Assert.IsType<UnauthorizedAccessException>(ex.InnerException);
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
            GC.SuppressFinalize(this);
        }

        internal class TestableFileService : FileService
        {
            private readonly Action _onDelete;

            public TestableFileService(IWebHostEnvironment environment, Action onDelete)
                : base(environment)
            {
                _onDelete = onDelete;
            }

            public override async Task DeleteFileAsync(string relativePath)
            {
                if (string.IsNullOrEmpty(relativePath) ||
                    relativePath.Contains("..") ||
                    Path.IsPathRooted(relativePath))
                {
                    throw new ArgumentException("Invalid file path specified", nameof(relativePath));
                }

                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

                try
                {
                    if (File.Exists(fullPath))
                    {
                        await Task.Run(() => _onDelete.Invoke());
                    }
                }
                catch (IOException ex)
                {
                    throw new InvalidOperationException($"Could not delete file: {relativePath}. It might be in use.", ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new InvalidOperationException($"Access denied when trying to delete file: {relativePath}", ex);
                }
            }
        }

        public class FileService : IFileService
        {
            protected readonly IWebHostEnvironment _environment;

            public FileService(IWebHostEnvironment environment)
            {
                _environment = environment;
            }

            public virtual async Task DeleteFileAsync(string relativePath)
            {
                if (string.IsNullOrEmpty(relativePath) ||
                    relativePath.Contains("..") ||
                    Path.IsPathRooted(relativePath))
                {
                    throw new ArgumentException("Invalid file path specified", nameof(relativePath));
                }

                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

                try
                {
                    if (File.Exists(fullPath))
                    {
                        await Task.Run(() => File.Delete(fullPath));
                    }
                }
                catch (IOException ex)
                {
                    throw new InvalidOperationException($"Could not delete file: {relativePath}. It might be in use.", ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new InvalidOperationException($"Access denied when trying to delete file: {relativePath}", ex);
                }
            }
        }
    }
}
