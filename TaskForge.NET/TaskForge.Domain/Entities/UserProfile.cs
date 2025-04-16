using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskForge.Domain.Entities.Common;

namespace TaskForge.Domain.Entities
{
    public class UserProfile : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; } // URL or file path to the user's avatar image

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(250)]
        public string? Location { get; set; }


        [MaxLength(150)]
        public string? JobTitle { get; set; }

        [MaxLength(200)]
        public string? Company { get; set; }

        [MaxLength(2000)]
        public string? ProfessionalSummary { get; set; } // A self-described text about skills & experience

        [MaxLength(500)]
        public string? LinkedInProfile { get; set; }

        [MaxLength(500)]
        public string? WebsiteUrl { get; set; } // Optional website or portfolio URL

        [Required]
        public string UserId { get; set; } = string.Empty; // Foreign Key to IdentityUser

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser User { get; set; } = null!;

        public virtual ICollection<ProjectMember> Projects { get; set; } = new List<ProjectMember>();
        public virtual ICollection<TaskAssignment> AssignedTasks { get; set; } = new List<TaskAssignment>();

    }
}
