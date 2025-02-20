using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Enums;
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

        // GET: Projects/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var projects = await _projectService.GetAllProjectsAsync(user.Id);
            return View(projects);
        }

        // GET: Projects/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateProjectViewModel
            {
                Title = "",
                Status = 0,
                StatusOptions = await _projectService.GetProjectStatusOptions()
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
                CreatedBy = user.Id
            };

            await _projectService.CreateProjectAsync(dto);
            return RedirectToAction("Index");
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int Id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var project = await _projectService.GetProjectByIdAsync(Id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }
    }
}
