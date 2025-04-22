using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Enums;
using TaskForge.Web.Models;
using TaskForge.WebUI.Models;

namespace TaskForge.WebUI.Controllers
{
    [Authorize(Roles = "Admin, User, Operator")]
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly ITaskService _taskService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly IProjectInvitationService _invitationService;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectController(
            IProjectMemberService projectMemberService,
            IProjectService projectService,
            ITaskService taskService,
            IProjectInvitationService invitationService,
            UserManager<IdentityUser> userManager)
        {
            _projectMemberService = projectMemberService;
            _projectService = projectService;
            _taskService = taskService;
            _invitationService = invitationService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ProjectFilterDto filter, int pageIndex = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            filter.UserId = user.Id;
            var projectFilteredList = await _projectService.GetFilteredProjectsAsync(filter, pageIndex, pageSize);

            var projectList = new ProjectListViewModel
            {
                Filter = filter,
                FilteredProjectList = projectFilteredList.Items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItems = projectFilteredList.TotalCount,
                TotalPages = projectFilteredList.TotalPages
            };

            return View(projectList);
        }


        // GET: Project/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateProjectViewModel
            {
                Title = "",
                Status = 0,
                StatusOptions = await _projectService.GetProjectStatusOptions(),
                StartDate = DateTime.Now,
                EndDate = null,
            };

            return View(viewModel);
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectViewModel createProjectViewModel)
        {
            if (!ModelState.IsValid)
            {
                createProjectViewModel.StatusOptions = await _projectService.GetProjectStatusOptions();
                return View(createProjectViewModel);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var createNewProject = new CreateProjectDto
            {
                Title = createProjectViewModel.Title,
                Description = createProjectViewModel.Description,
                Status = createProjectViewModel.Status,
                CreatedBy = user.Id,
                StartDate = createProjectViewModel.StartDate,
                EndDate = createProjectViewModel.EndDate,
            };

            await _projectService.CreateProjectAsync(createNewProject);
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            if (!ModelState.IsValid) return View();

            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();

            var projectUpdate = new ProjectUpdateViewModel
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                StartDate = project.StartDate,
                Status = project.Status,
                EndDateInput = project.EndDate,
            };

            return PartialView("_EditProjectForm", projectUpdate);
        }

        // POST: Projects/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProjectUpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }

                return PartialView("_EditProjectForm", viewModel); // Return form with errors
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var existingProject = await _projectService.GetProjectByIdAsync(viewModel.Id);
            if (existingProject == null)
            {
                return NotFound();
            }

            // Update properties
            existingProject.Title = viewModel.Title;
            existingProject.Description = viewModel.Description;
            existingProject.StartDate = viewModel.StartDate;
            existingProject.SetEndDate(viewModel.EndDateInput);
            existingProject.Status = viewModel.Status;
            existingProject.UpdatedBy = user.Id;
            existingProject.UpdatedDate = DateTime.UtcNow;

            await _projectService.UpdateProjectAsync(existingProject);

            return RedirectToAction("Dashboard", "Project", new { id = existingProject.Id });
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!ModelState.IsValid) return View();

            // Restrict project access to assigned users only
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, id);
            if (member == null)
            {
                return Forbid(); // User is not an Admin, access denied
            }

            var project = await _projectService.GetProjectByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            // return project.tasks = the tasks of this project

            project.TaskItems = (await _taskService.GetTaskListAsync(id)).ToList();

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project
            };

            return View(viewModel);
        }

        // GET: Project/Dashboard/5
        public async Task<IActionResult> Dashboard(int id)
        {
            if (!ModelState.IsValid) return View();

            // Restrict project access to assigned users only
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, id);
            if (member == null)
            {
                return Forbid();
            }

            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var taskList = (await _taskService.GetTaskListAsync(project.Id)).ToList();

            var sortedTodoTasks = await _taskService.GetSortedTasksAsync(TaskWorkflowStatus.ToDo, id) ?? [];
            var sortedInProgressTasks = await _taskService.GetSortedTasksAsync(TaskWorkflowStatus.InProgress, id) ?? [];
            var sortedCompletedTasks = await _taskService.GetSortedTasksAsync(TaskWorkflowStatus.Done, id) ?? [];
            var sortedBlockedTasks = await _taskService.GetSortedTasksAsync(TaskWorkflowStatus.Blocked, id) ?? [];



            var model = new ProjectDashboardViewModel
            {
                ProjectId = project.Id,
                ProjectTitle = project.Title,
                ProjectDescription = project.Description ?? "",
                ProjectStatus = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                UserRoleInThisProject = member.Role,
                TotalTasks = taskList.Count,
                PendingTasks = taskList.Count(t => t.Status == TaskWorkflowStatus.ToDo),
                CompletedTasks = taskList.Count(t => t.Status == TaskWorkflowStatus.Done),
                Members = (await _projectMemberService.GetProjectMembersAsync(project.Id)).Select(m => new ProjectMemberViewModel
                {
                    Id = m.Id,
                    Name = m.Name ?? "",
                    Email = m.Email ?? "",
                    Role = m.Role
                }).ToList(),
                Invitations = (await _invitationService.GetInvitationListAsync(project.Id, 1, 10)).Items.Select(m => new InviteViewModel
                {
                    Id = m.Id,
                    ProjectId = m.ProjectId,
                    InvitedUserEmail = m.InvitedUserProfile?.User?.UserName ?? "No User",
                    Status = m.Status,
                    InvitationSentDate = m.InvitationSentDate,
                    AssignedRole = m.AssignedRole
                }).ToList(),
                TaskItems = taskList.Select(t => new TaskItemViewModel
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    StartDate = t.StartDate,
                    DueDate = t.DueDate,
                    AssignedUsers = t.AssignedUsers.Select(a => new TaskAssignmentViewModel
                    {
                        UserProfileId = a.UserProfileId,
                        FullName = a.UserProfile.FullName ?? "",
                        UserName = a.UserProfile.User.UserName ?? "",
                        Email = a.UserProfile.User.Email ?? "",
                        AvatarUrl = a.UserProfile.AvatarUrl
                    }).ToList()
                }).ToList(),
                SortedTodoTasks = sortedTodoTasks,
                SortedInProgressTasks = sortedInProgressTasks,
                SortedCompletedTasks = sortedCompletedTasks,
                SortedBlockedTasks = sortedBlockedTasks,
                UpdateViewModel = new ProjectUpdateViewModel
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description,
                    StartDate = project.StartDate,
                    Status = project.Status,
                    EndDateInput = project.EndDate
                }
            };

            return View(model);

        }

        // GET: Project/ManageMembers
        [HttpGet]
        public async Task<IActionResult> ManageMembers(int Id, int pageIndex = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid) return View();

            // Restrict project access to assigned users only
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, Id);
            if (member == null || member.Role != ProjectRole.Admin)
            {
                return Forbid(); // User is not an Admin, access denied
            }

            var project = await _projectService.GetProjectByIdAsync(Id);
            if (project == null) return NotFound();

            var projectMembers = await _projectMemberService.GetProjectMembersAsync(Id);
            var projectInvitations = await _invitationService.GetInvitationListAsync(Id, pageIndex, pageSize);

            var manageMembersViewModel = new ManageMembersViewModel
            {
                ProjectId = Id,
                ProjectTitle = project.Title,
                ProjectDescription = string.IsNullOrEmpty(project.Description) ? "No Description" : project.Description,
                ProjectMembers = projectMembers.Select(m => new ProjectMemberViewModel
                {
                    Id = m.Id,
                    Name = m.Name ?? "Unknown",
                    Email = m.Email ?? "N/A",
                    Role = m.Role
                }).ToList(),
                ProjectInvitations = new PaginatedList<InviteViewModel>(
                     projectInvitations.Items.Select(pi => new InviteViewModel
                     {
                         Id = pi.Id,
                         ProjectId = pi.ProjectId,
                         InvitedUserEmail = pi.InvitedUserProfile?.User?.Email ?? "N/A",
                         Status = pi.Status,
                         InvitationSentDate = pi.InvitationSentDate,
                         AssignedRole = pi.AssignedRole
                     }).ToList(),
                     projectInvitations.TotalCount,
                     pageIndex,
                     pageSize
                 )
            };

            return View(manageMembersViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int id)
        {
            if (!ModelState.IsValid) return View();

            // Fetch the member
            var member = await _projectMemberService.GetByIdAsync(id);
            if (member == null)
            {
                return NotFound("Member not found.");
            }

            // Restrict project access to assigned users only
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var userMember = await _projectMemberService.GetUserProjectRoleAsync(user.Id, member.ProjectId);
            if (userMember == null || userMember.Role != ProjectRole.Admin)
            {
                return Forbid();
            }

            if (userMember.Id == member.Id)
            {
                return BadRequest($"Invalid request.");
            }

            // Remove the member from the project
            await _projectMemberService.RemoveAsync(id);

            // Redirect back to the Project Management Members page
            return RedirectToAction("ManageMembers", "Project", new { Id = member.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelInvitation(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Fetch the invitation
            var invitation = await _invitationService.GetByIdAsync(id);
            if (invitation == null)
            {
                return NotFound("Invitation not found.");
            }

            // Restrict project access to assigned users only
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var userMember = await _projectMemberService.GetUserProjectRoleAsync(user.Id, invitation.ProjectId);
            if (userMember == null || userMember.Role != ProjectRole.Admin)
            {
                return Forbid();
            }

            // Prevent cancellation if the status is not "Pending"
            if (invitation.Status != InvitationStatus.Pending)
            {
                return BadRequest($"Cannot cancel an invitation that is already {invitation.Status.ToString().ToLower()}.");
            }

            invitation.Status = InvitationStatus.Canceled;

            // Update the invitation status in the database
            await _invitationService.UpdateInvitationStatusAsync(id, invitation.Status);

            // Redirect back to the Project Management Members page
            return RedirectToAction("ManageMembers", "Project", new { Id = invitation.ProjectId });
        }

    }
}