using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Entities;

namespace TaskForge.Web.Models
{
	public class ProjectListViewModel : PaginationViewModel
	{
        public ProjectFilterDto? Filter { get; set; }
		public IEnumerable<ProjectWithRoleDto>? FilteredProjectList { get; set; }
	}
}
