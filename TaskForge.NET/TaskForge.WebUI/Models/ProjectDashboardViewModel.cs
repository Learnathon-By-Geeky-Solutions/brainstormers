using TaskForge.Domain.Enums;

namespace TaskForge.WebUI.Models
{
    public class ProjectDashboardViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public ProjectStatus ProjectStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectRole? UserRoleInThisProject { get; set; }


        public ProjectUpdateViewModel UpdateViewModel { get; set; } = new ProjectUpdateViewModel();


        // Task Summary
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int CompletedTasks { get; set; }
        public List<TaskItemViewModel> TaskItems { get; set; } = new();

        // Team Members
        public List<ProjectMemberViewModel> Members { get; set; } = new();
        public List<InviteViewModel> Invitations { get; set; } = new();

    }

}
