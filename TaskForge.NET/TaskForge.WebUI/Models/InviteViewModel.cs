using System.ComponentModel.DataAnnotations;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.WebUI.Models
{
    public class InviteViewModel
    {
        public int ProjectId { get; set; }

        [Required]
        [EmailAddress]
        public required string InvitedUserEmail { get; set; }

        [Required]
        public ProjectRole AssignedRole { get; set; }

        public Project Project { get; set; } 
    }
}