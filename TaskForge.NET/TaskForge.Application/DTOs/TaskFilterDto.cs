using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.DTOs
{
    public class TaskFilterDto
    {
        public int? UserProfileId { get; set; }
        public int? ProjectId {  get; set; }
        public string? Title { get; set; }
        public TaskWorkflowStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }

        public string? SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";
    }
}
