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

        private string ValidateAndGetAbsolutePath(string relativePath)
        {
	        if (string.IsNullOrWhiteSpace(relativePath))
		        throw new ArgumentException("Path cannot be null or empty.", nameof(relativePath));

	        // Normalize path for cross-platform comparison
	        var normalizedRoot = Path.GetFullPath(_environment.WebRootPath).Replace('\\', '/');
	        var normalizedInput = relativePath.Replace('\\', '/');

	        // Check for absolute paths (both Windows and Unix style)
	        if (Path.IsPathRooted(normalizedInput))
	        {
		        // Get full path and normalize for comparison
		        var inputFullPath = Path.GetFullPath(Path.Combine(normalizedRoot, normalizedInput))
			        .Replace('\\', '/');

		        if (!inputFullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
			        throw new ArgumentException("Absolute paths are not allowed.", nameof(relativePath));
	        }

	        // Prevent directory traversal
	        var combinedPath = Path.Combine(normalizedRoot, normalizedInput);
	        var fullPath = Path.GetFullPath(combinedPath).Replace('\\', '/');

	        if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
		        throw new ArgumentException("Path traversal detected.", nameof(relativePath));

	        return fullPath;
        }

        public Task DeleteFileAsync(string relativePath)
        {
	        var fullPath = ValidateAndGetAbsolutePath(relativePath);

	        try
	        {
		        if (File.Exists(fullPath))
		        {
			        File.Delete(fullPath);
		        }
		        return Task.CompletedTask;
	        }
	        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
	        {
		        throw new InvalidOperationException("Failed to delete the file.", ex);
	        }
        }

	}

}