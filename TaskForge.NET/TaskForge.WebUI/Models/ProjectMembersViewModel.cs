using System.ComponentModel.DataAnnotations;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using System.Collections.Generic;

namespace TaskForge.WebUI.Models
{
    public class ProjectMembersViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string ProjectDescription { get; set; }
        public List<ProjectMemberViewModel> ProjectMembers { get; set; } = new();

        [Required]
        [EmailAddress]
        public string InvitedUserEmail { get; set; } // For invitation

        [Required]
        public ProjectRole AssignedRole { get; set; } // For invitation
    }


    public class InviteViewModel
    {
        public int ProjectId { get; set; }

        [Required]
        [EmailAddress]
        public required string InvitedUserEmail { get; set; }

        [Required]
        public ProjectRole AssignedRole { get; set; }

    }

    public class ProjectMemberViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public ProjectRole Role { get; set; }
    }
}