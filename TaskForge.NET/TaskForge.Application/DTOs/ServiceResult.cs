namespace TaskForge.Application.DTOs
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ServiceResult SuccessResult(string message) => new ServiceResult { Success = true, Message = message };
        public static ServiceResult FailureResult(string message) => new ServiceResult { Success = false, Message = message };
    }

}
