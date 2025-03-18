using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TaskForge.Application.Interfaces;
using System.Threading.Tasks;
using TaskForge.WebUI.Models;
using TaskForge.Application.Interfaces.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TaskForge.WebUI.Controllers
{
    [Authorize] // Ensure only authenticated users can access
    public class HomeController : Controller
    {
        private readonly IProjectMemberService _projectMemberService;
        private readonly IUserProfileService _userProfileService;
        private readonly ITaskService _taskService;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            IProjectMemberService projectMemberService,
            IUserProfileService userProfileService,
            ITaskService taskService,
            UserManager<IdentityUser> userManager)
        {
            _projectMemberService = projectMemberService;
            _userProfileService = userProfileService;
            _taskService = taskService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            string userId = user.Id;

            var userProfileId = await _userProfileService.GetByUserIdAsync(userId);
            if(userProfileId == null) return NotFound();

            var totalProjects = await _projectMemberService.GetUserProjectCountAsync(userProfileId);
            var userTasks = await _taskService.GetUserTaskAsync(userProfileId);

            var completedTasks = userTasks.Count(task => task.Status == Domain.Enums.TaskWorkflowStatus.Done);

            var model = new HomeViewModel
            {
                TotalProjects = totalProjects,
                TotalTasks = userTasks.Count,
                CompletedTasks = completedTasks,
                UserTasks = userTasks
            };

            return View(model);
        }
    }
}
