using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;

namespace TaskForge.Web.Models
{
    public class ProjectListViewModel
    {
        public ProjectFilterDto? Filter { get; set; }
        public IEnumerable<ProjectWithRoleDto>? ProjectWithRoleDto { get; set; }
    }
}
