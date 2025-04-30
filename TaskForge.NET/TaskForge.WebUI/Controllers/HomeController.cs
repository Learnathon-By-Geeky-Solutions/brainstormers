using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskForge.Application.Interfaces.Services;
using TaskForge.WebUI.Models;

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

        [AllowAnonymous]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 10)
        {
            if (User?.Identity?.IsAuthenticated == false)
            {
                return View("Welcome");
            }

            if (!ModelState.IsValid) return BadRequest();

            var user = await _userManager.GetUserAsync(User!);
            if (user == null) return RedirectToPage("Identity/Account/Login");


            var userId = user.Id;

            var userProfileId = await _userProfileService.GetUserProfileIdByUserIdAsync(userId);
            if (userProfileId == null) return BadRequest();

            var totalProjects = await _projectMemberService.GetUserProjectCountAsync(userProfileId);
            var userTaskList = await _taskService.GetUserTaskAsync(userProfileId, pageIndex, pageSize);

            var taskList = new HomeViewModel
            {
                TotalProjects = totalProjects,
                TotalTasks = userTaskList.TotalCount,
                CompletedTasks = userTaskList.Items.Count(task => task.Status == Domain.Enums.TaskWorkflowStatus.Done),

                UserTasks = userTaskList.Items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItems = userTaskList.TotalCount,
                TotalPages = userTaskList.TotalPages
            };

            return View("Index", taskList);
        }
    }
}