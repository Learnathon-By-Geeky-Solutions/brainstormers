using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Services;
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
        private readonly IUserProfileService _userProfileService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly IProjectInvitationService _invitationService;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectController(
            IProjectMemberService projectMemberService,
            IProjectService projectService,
            ITaskService taskService,
            IUserProfileService userProfileService,
            IProjectInvitationService invitationService,
            UserManager<IdentityUser> userManager)
        {
            _projectMemberService = projectMemberService;
            _projectService = projectService;
            _taskService = taskService;
            _userProfileService = userProfileService;
            _invitationService = invitationService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ProjectFilterDto filter)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            filter.UserId = user.Id;
            var projects = await _projectService.GetFilteredProjectsAsync(filter);

            var viewModel = new ProjectListViewModel
            {
                Filter = filter,
                Projects = projects
            };

            return View(viewModel);
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
        public async Task<IActionResult> Create(CreateProjectViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.StatusOptions = await _projectService.GetProjectStatusOptions();
                return View(viewModel);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var dto = new CreateProjectDto
            {
                Title = viewModel.Title,
                Description = viewModel.Description,
                Status = viewModel.Status,
                CreatedBy = user.Id,
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
            };

            await _projectService.CreateProjectAsync(dto);
            return RedirectToAction("Index");
        }


        // GET: Project/Invite
        [HttpGet]
        public async Task<IActionResult> ManageMembers(int Id)
        {
            // Restrict project access to assigned users only
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, Id);
            if (member == null || member.Role != ProjectRole.Admin)
            {
                return Forbid(); // User is not an Admin, access denied
            }

            var project = await _projectService.GetProjectByIdAsync(Id); // Retrieve project info
            var projectMembers = await _projectMemberService.GetProjectMembersAsync(Id); // Get project members

            var model = new ManageMembersViewModel
            {
                ProjectId = Id,
                ProjectTitle = project.Title,
                ProjectDescription = project.Description,
                ProjectMembers = projectMembers.Select(m => new ProjectMemberViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Email = m.Email,
                    Role = m.Role
                }).ToList()
            };

            var projectInvitations = await _invitationService.GetInvitationListAsync(Id); // Get project members

            model.ProjectInvitations = projectInvitations.Select(m => new InviteViewModel
            {
                Id = m.Id,
                ProjectId = m.ProjectId,
                InvitedUserEmail = m.InvitedUserProfile.User.UserName,
                Status = m.Status,
                InvitationSentDate = m.InvitationSentDate,
                AssignedRole = m.AssignedRole
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Invite(InviteViewModel viewModel)
        {
            // Restrict project access to assigned users only
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, viewModel.ProjectId);

            if (member == null || member.Role != ProjectRole.Admin)
            {
                return Forbid(); // User is not an Admin, access denied
            }

            // Send Invitation
            var success = await _invitationService.AddAsync(viewModel.ProjectId, viewModel.InvitedUserEmail, viewModel.AssignedRole);
            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to send invitation.";
                return RedirectToAction("ManageMembers", new { Id = viewModel.ProjectId });
            }

            // Redirect to Manage Members Page with success message
            TempData["SuccessMessage"] = "Invitation sent to " + viewModel.InvitedUserEmail + " successfully.";
            return RedirectToAction("ManageMembers", new { Id = viewModel.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF Protection
        public async Task<IActionResult> UpdateInvitationStatus(int id, InvitationStatus status)
        {
            // Fetch the invitation
            var invitation = await _invitationService.GetByIdAsync(id);
            if (invitation == null)
            {
                return NotFound("Invitation not found.");
            }

            // Prevent invalid status changes
            if (invitation.Status != InvitationStatus.Pending)
            {
                return BadRequest($"Cannot update an invitation that is already {invitation.Status.ToString().ToLower()}.");
            }


            await _invitationService.UpdateInvitationStatusAsync(id, status);
            return RedirectToAction("ManageMembers", new { Id = invitation.ProjectId });
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(int Id)
        {
            // Restrict project access to assigned users only
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, Id);
            if (member == null)
            {
                return Forbid(); // User is not an Admin, access denied
            }

            var project = await _projectService.GetProjectByIdAsync(Id);

            if (project == null)
            {
                return NotFound();
            }

            // return project.tasks = the tasks of this project

            project.Tasks = (await _taskService.Get(Id)).ToList();

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project
            };

            return View(viewModel);
        }

    }
}
