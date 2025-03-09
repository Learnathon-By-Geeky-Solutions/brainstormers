using Microsoft.AspNetCore.Mvc;
using TaskForge.Domain.Entities;
using TaskForge.WebUI.Models;
using System;
using System.Linq;
using TaskForge.Domain.Enums;
using TaskForge.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;

namespace TaskForge.WebUI.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly UserManager<IdentityUser> _userManager;

        public TaskController(ITaskService taskService, UserManager<IdentityUser> userManager)
        {
            _taskService = taskService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create(int projectId)
        {
            var model = new TaskItemViewModel { ProjectId = projectId };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Create(TaskItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Return the form with validation errors
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var taskDto = new TaskDto
            {
                Title = model.Title,
                Description = model.Description,
                ProjectId = model.ProjectId,
                CreatedBy = user.Id,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                StartDate = DateTime.UtcNow,
                DueDate = model.DueDate
            };
            await _taskService.CreateTaskAsync(taskDto);

            return RedirectToAction("Details", "Project", new { Id = model.ProjectId });
        }

    }
}

