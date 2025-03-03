using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Services;
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

        public ProjectController(IProjectMemberService projectMemberService, IProjectService projectService, ITaskService taskService, IUserProfileService userProfileService, IProjectInvitationService invitationService, UserManager<IdentityUser> userManager)
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
        public async Task<IActionResult> Invite(int projectId)
        {
            // Retrieve the project (Optional)
            var project = await _projectService.GetProjectByIdAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            // Return the view with projectId
            return View(new InviteViewModel { ProjectId = projectId, Project = project, InvitedUserEmail = ""});
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(int Id)
        {

            // Restrict project access to assigned users only
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var userProfileId = await _userProfileService.GetByUserIdAsync(user.Id);
            bool isMember = await _projectMemberService.IsUserAssignedToProjectAsync(user.Id, Id);
            if (!isMember)
            {
                return Forbid(); // User is not allowed to access this project
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
