using Microsoft.AspNetCore.Hosting;
using TaskForge.Application.Interfaces.Services;

namespace TaskForge.Application.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task DeleteFileAsync(string relativePath)
        {
            // Prevent path traversal attacks
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
                    await Task.Run(() => File.Delete(fullPath)); // Run on thread pool to avoid blocking
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