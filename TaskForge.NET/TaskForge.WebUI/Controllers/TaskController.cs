using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.WebUI.Models;

namespace TaskForge.WebUI.Controllers
{
    [Authorize(Roles = "Admin, User, Operator")]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly UserManager<IdentityUser> _userManager;

        public TaskController(ITaskService taskService, UserManager<IdentityUser> userManager)
        {
            _taskService = taskService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] TaskItemCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

	    var taskDto = new TaskDto
            {
                ProjectId = model.ProjectId,
                Title = model.Title,
                Description = model.Description,
                StartDate = model.StartDate,
                DueDate = model.DueDate,
                Status = model.Status,
                Priority = model.Priority,
				Attachments = model.Attachments
			};
            await _taskService.CreateTaskAsync(taskDto);

            return Ok(new { success = true });
        }



		[HttpGet("Tasks/GetTaskById/{id}")]
		public async Task<IActionResult> GetTaskDetailsById(int id)
		{
			var task = await _taskService.GetTaskByIdAsync(id);
			if (task == null) return NotFound();

			return Ok(new
			{
				task.Id,
				task.Title,
				task.Description,
				StartDate = task.StartDate?.ToString("g"),
				DueDate = task.DueDate?.ToString("g"),
				Status = task.Status.ToString(),
				Priority = task.Priority.ToString(),
				AssignedUsers = task.AssignedUsers.Select(a => new { a.UserProfile.FullName }),
				Attachments = task.Attachments.Select(a => new { a.FileName, a.FilePath })
			});
		}

	}
}

