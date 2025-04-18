using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;

namespace TaskForge.WebUI.Models
{
    public class HomeViewModel : PaginationViewModel
    {
        public int TotalProjects { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public List<TaskDto>? UserTasks { get; set; } = new();
    }
}
