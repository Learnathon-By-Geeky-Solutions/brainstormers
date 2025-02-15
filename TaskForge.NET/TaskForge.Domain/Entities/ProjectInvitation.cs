using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Domain.Entities.Common;
using TaskForge.Domain.Enums;

namespace TaskForge.Domain.Entities
{
    public class ProjectInvitation : BaseEntity
    {
        // Foreign Key to Project
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public int InvitedUserProfileId { get; set; } // FK to UserProfile
        public virtual UserProfile InvitedUserProfile { get; set; } = null!; // Navigational property to UserProfile of invited user

        // The status of the invitation
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        // Date when the invitation was sent
        public DateTime InvitationSentDate { get; set; } = DateTime.UtcNow;

        // Optional: Date when the invitation was accepted
        public DateTime? AcceptedDate { get; set; }

        // Optional: Date when the invitation was declined
        public DateTime? DeclinedDate { get; set; }

        // The role assigned to the invited user (if they accept the invitation)
        public ProjectRole AssignedRole { get; set; } = ProjectRole.Read; // Default role can be "Read"
    }
}
