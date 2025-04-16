using TaskForge.Application.Common.Model;

namespace TaskForge.WebUI.Models
{
    public class ProjectInvitationViewModel
    {
        public int Id { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime InvitationSentDate { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public DateTime? DeclinedDate { get; set; }
    }
    public class ProjectInvitationListViewModel : PaginationViewModel
    {
        public List<ProjectInvitationViewModel> Invitations { get; set; } = new();
    }
}
