using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;

namespace TaskForge.WebUI.Models;

public class ProjectListViewModel : PaginationViewModel
{
    public ProjectFilterDto? Filter { get; set; }
    public IEnumerable<ProjectWithRoleDto>? FilteredProjectList { get; set; }
}