using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Web.Models;
using TaskForge.WebUI.Models;

namespace TaskForge.WebUI.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectController(IProjectService projectService, UserManager<IdentityUser> userManager)
        {
            _projectService = projectService;
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
    }
}
