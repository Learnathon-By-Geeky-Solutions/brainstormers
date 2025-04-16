using System.ComponentModel.DataAnnotations;
using TaskForge.Application.Common.Model;
using TaskForge.Domain.Enums;

namespace TaskForge.WebUI.Models
{
    public class ManageMembersViewModel
    {
        public int ProjectId { get; set; }
        public required string ProjectTitle { get; set; }
        public string ProjectDescription { get; set; } = string.Empty;
        public List<ProjectMemberViewModel> ProjectMembers { get; set; } = new();
        public PaginatedList<InviteViewModel> ProjectInvitations { get; set; } = new PaginatedList<InviteViewModel>(new List<InviteViewModel>(), 0, 1, 10);

        [Required]
        [EmailAddress]
        public string InvitedUserEmail { get; set; } = string.Empty; // For invitation

        [Required]
        public ProjectRole AssignedRole { get; set; } // For invitation
    }


    public class InviteViewModel : PaginationViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        [Required]
        [EmailAddress]
        public required string InvitedUserEmail { get; set; }

        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        public DateTime InvitationSentDate { get; set; } = DateTime.UtcNow;

        [Required]
        public ProjectRole AssignedRole { get; set; }

    }

    public class ProjectMemberViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public ProjectRole Role { get; set; }
    }
}