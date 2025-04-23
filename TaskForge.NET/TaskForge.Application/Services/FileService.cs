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

			// Check for any absolute path (both Windows and Unix style)
			if (Path.IsPathRooted(relativePath) && !relativePath.StartsWith(_environment.WebRootPath, StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException("Absolute paths or path traversal is not allowed.", nameof(relativePath));

			var combinedPath = Path.Combine(_environment.WebRootPath, relativePath);
			var fullPath = Path.GetFullPath(combinedPath);

			if (!fullPath.StartsWith(_environment.WebRootPath, StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException("Invalid path traversal detected.", nameof(relativePath));

			return fullPath;
		}


		public async Task DeleteFileAsync(string relativePath)
		{
			var fullPath = ValidateAndGetAbsolutePath(relativePath);

			try
			{
				if (File.Exists(fullPath))
				{
					File.Delete(fullPath);
				}
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				throw new InvalidOperationException("Failed to delete the file.", ex);
			}

			await Task.CompletedTask;
		}


	}

}