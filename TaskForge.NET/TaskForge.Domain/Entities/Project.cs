using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Domain.Entities.Common;
using TaskForge.Domain.Enums;

namespace TaskForge.Domain.Entities
{
    public class Project : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; private set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.NotStarted; // Enum for status

        public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();



        // Custom method to set EndDate safely
        public void SetEndDate(DateTime? endDate)
        {
            if (endDate.HasValue && endDate < StartDate)
            {
                throw new ArgumentException("EndDate cannot be earlier than StartDate.");
            }
            EndDate = endDate;
        }
    }
}
