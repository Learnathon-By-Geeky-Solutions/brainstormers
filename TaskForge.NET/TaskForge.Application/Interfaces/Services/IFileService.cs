namespace TaskForge.Application.Interfaces.Services
{
    public interface IFileService
    {
        Task DeleteFileAsync(string relativePath);
    }

}