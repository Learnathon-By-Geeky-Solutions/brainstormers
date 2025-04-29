using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Enums;
using TaskForge.WebUI.Models;

namespace TaskForge.WebUI.Controllers
{
    [Authorize(Roles = "Admin, User, Operator")]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly UserManager<IdentityUser> _userManager;

        public TaskController(ITaskService taskService, 
            IProjectMemberService projectMemberService, 
            UserManager<IdentityUser> userManager)
        {
            _taskService = taskService;
            _projectMemberService = projectMemberService;
            _userManager = userManager;
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

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, model.ProjectId);
                if (member == null || member.Role == ProjectRole.Viewer) return Json(new { success = false, message = "You do not have permission to create tasks in this project." });

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
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetTask(int id)
        {
            if (!ModelState.IsValid) return Json(new { success = false, message = "Invalid Data" });

            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            var allUsers = await _projectMemberService.GetProjectMembersAsync(task.ProjectId);

            var dependentTaskIds = await _taskService.GetDependentTaskIdsAsync(id, task.Status);

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
                allUsers = allUsers.Select(u => new { id = u.UserProfileId, name = u.Name }),
                dependsOnTaskIds = task.Dependencies.Select(u => u.DependsOnTaskId),
                dependentTaskIds
            });
        }


        [HttpPut]
        public async Task<IActionResult> Update(TaskUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid data." });

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var task = await _taskService.GetTaskByIdAsync(dto.Id);
                if (task == null) return Json(new { success = false, message = "Task not found." });

                var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, task.ProjectId);
                if (member == null || member.Role == ProjectRole.Viewer) return Json(new { success = false, message = "You do not have permission to update tasks in this project." });


                await _taskService.UpdateTaskAsync(dto);

                return Json(new { success = true, message = "Task updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var task = await _taskService.GetTaskByIdAsync(id);
                if (task == null) return Json(new { success = false, message = "Task not found." });

                var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, task.ProjectId);
                if (member == null || member.Role == ProjectRole.Viewer) return Json(new { success = false, message = "You do not have permission to delete tasks in this project." });


                await _taskService.RemoveTaskAsync(id);
                return Json(new { success = true, message = "Task deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            if (!ModelState.IsValid) return Json(new { success = false, message = "Invalid Data" });

            try
            {
                var user = await _userManager.GetUserAsync(User);

                await _taskService.DeleteAttachmentAsync(id, user!.Id);
                return Json(new { success = true, message = "TaskAttachment deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}

