using Microsoft.AspNetCore.Authorization;
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

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
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
        public async Task<IActionResult> GetTask(int id)
        {
            var taskDetailsDTO = await _taskService.GetTaskDetailsAsync(id);
            if (taskDetailsDTO == null) return NotFound();

            // fix attachment URLs (because Url.Content is only available in controller)
            taskDetailsDTO.Attachments.ForEach(a => a.DownloadUrl = Url.Content($"~{a.DownloadUrl}"));

            return Json(taskDetailsDTO);
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


        [HttpDelete]
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
        [ValidateAntiForgeryToken]
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
