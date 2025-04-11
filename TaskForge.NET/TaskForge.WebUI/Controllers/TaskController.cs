using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Services;
using TaskForge.WebUI.Models;

namespace TaskForge.WebUI.Controllers
{
    [Authorize(Roles = "Admin, User, Operator")]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IProjectMemberService _projectMemberService;
		private readonly UserManager<IdentityUser> _userManager;

        public TaskController(ITaskService taskService, IProjectMemberService projectMemberService, UserManager<IdentityUser> userManager)
        {
            _taskService = taskService;
			_projectMemberService = projectMemberService;
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





		[HttpGet]
		public async Task<IActionResult> GetTask(int id)
		{
			var task = await _taskService.GetTaskByIdAsync(id);
			if (task == null) return NotFound();

			var allUsers = await _projectMemberService.GetProjectMembersAsync(task.ProjectId);
			return Json(new
			{
				id = task.Id,
				title = task.Title,
				description = task.Description,
				startDate = task.StartDate?.ToString("yyyy-MM-ddTHH:mm"),
				dueDate = task.DueDate?.ToString("yyyy-MM-ddTHH:mm"),
				status = (int)task.Status,
				priority = (int)task.Priority,
				attachments = task.Attachments.Select(a => new
				{
					id = a.Id,
					fileName = a.FileName,
					downloadUrl = Url.Action("Download", "Attachment", new { id = a.Id })
				}),
				assignedUserIds = task.AssignedUsers.Select(u => u.UserProfileId),
				allUsers = allUsers.Select(u => new { id = u.Id, name = u.Name })
			});
		}


		[HttpPost]
		public async Task<IActionResult> UpdateTask(TaskUpdateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			await _taskService.UpdateTaskAsync(dto);

			return Ok();
		}

	}
}

