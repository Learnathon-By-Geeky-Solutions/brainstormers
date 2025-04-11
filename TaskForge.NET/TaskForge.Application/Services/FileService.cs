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

        public Task DeleteFileAsync(string relativePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }
    }

}
