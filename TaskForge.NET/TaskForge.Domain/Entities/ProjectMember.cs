using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Entities.Common;
using TaskForge.Domain.Enums;

namespace TaskForge.Domain.Entities
{
    public class ProjectMember : BaseEntity
    {
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public int UserProfileId { get; set; } // FK to UserProfile
        public virtual UserProfile UserProfile { get; set; } = null!; // Navigational property to UserProfile

        public ProjectRole Role { get; set; } = ProjectRole.Admin; // Default role is "Read" (Who created the project, will be Admin by default)
    }

}
