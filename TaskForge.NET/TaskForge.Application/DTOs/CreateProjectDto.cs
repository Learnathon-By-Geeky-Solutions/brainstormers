using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.DTOs
{
    public class CreateProjectDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required ProjectStatus Status { get; set; }
        public required string CreatedBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
