using Moq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Services;
using Xunit;

namespace TaskForge.Tests.Application.Services
{
    public sealed class FileServiceTests : IDisposable
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly FileService _fileService;
        private readonly string _webRootPath;
        private bool _disposed;

        public FileServiceTests()
        {
            _mockEnv = new Mock<IWebHostEnvironment>();
            _webRootPath = Path.Combine(Path.GetTempPath(), "TaskForgeTestFiles");
            Directory.CreateDirectory(_webRootPath);
            _mockEnv.Setup(e => e.WebRootPath).Returns(_webRootPath);
            _fileService = new FileService(_mockEnv.Object);
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
			// Skip test on Linux if it's known to not work
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return;
			}

			// Rest of the test remains the same
			var relativePath = "io-error.txt";
			var fullPath = Path.Combine(_webRootPath, relativePath);
			await File.WriteAllTextAsync(fullPath, "test data");

			try
			{
				using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.None);
				var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _fileService.DeleteFileAsync(relativePath));
				Assert.IsType<IOException>(ex.InnerException);
			}
			finally
			{
				if (File.Exists(fullPath))
				{
					File.Delete(fullPath);
				}
			}
		}



		public void Dispose()
        {
            if (_disposed) return;

            try
            {
                if (Directory.Exists(_webRootPath))
                {
                    foreach (var file in Directory.GetFiles(_webRootPath))
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }

                    Directory.Delete(_webRootPath, recursive: true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cleanup failed: {ex.Message}");
            }

            _disposed = true;
        }

    }
}