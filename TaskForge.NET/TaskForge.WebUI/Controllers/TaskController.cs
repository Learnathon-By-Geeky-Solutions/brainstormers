using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.WebUI.Models;

namespace TaskForge.WebUI.Controllers
{
	[Authorize(Roles = "Admin, User, Operator")]
	public class TaskController : Controller
	{
		private readonly ITaskService _taskService;
        private readonly IProjectMemberService _projectMemberService;

		public TaskController(ITaskService taskService, IProjectMemberService projectMemberService)
		{
			_taskService = taskService;
            _projectMemberService = projectMemberService;
		}


		[HttpPost]
		public async Task<IActionResult> Create([FromForm] TaskItemCreateViewModel model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return Json(new { success = false, message = "Invalid data." });
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

				return Json(new { success = true, message = "Task created successfully." });
			}
			catch (Exception ex)
			{
				// Log exception if needed
				return Json(new { success = false, message = ex.Message });
			}
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
				Attachments = task.Attachments.Select(a => new { a.FileName, a.StoredFileName, a.FilePath })
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
                    downloadUrl = Url.Content($"~/uploads/tasks/{a.StoredFileName}")
        }),
				assignedUserIds = task.AssignedUsers.Select(u => u.UserProfileId),
				allUsers = allUsers.Select(u => new { id = u.UserProfileId, name = u.Name })
			});
		}


        [HttpPost]
		public async Task<IActionResult> Update(TaskUpdateDto dto)
		{
			try
			{
				if (!ModelState.IsValid)
					return Json(new { success = false, message = "Invalid data." });

				await _taskService.UpdateTaskAsync(dto);

				return Json(new { success = true, message = "Task updated successfully." });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _taskService.RemoveTaskAsync(id);
				return Json(new { success = true, message = "Task deleted successfully." });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}


		[HttpDelete]
		public async Task<IActionResult> DeleteAttachment(int id)
		{
			try
			{
				await _taskService.DeleteAttachmentAsync(id);
				return Json(new { success = true, message = "TaskAttachment deleted successfully." });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}


	}
}

