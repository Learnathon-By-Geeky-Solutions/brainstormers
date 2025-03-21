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

        [HttpPost]
        public async Task<IActionResult> Create(TaskItemCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Dashboard", "Project", new { Id = model.ProjectId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var taskDto = new TaskDto
            {
                ProjectId = model.ProjectId,
                Title = model.Title,
                Description = model.Description,
                StartDate = model.StartDate,
                DueDate = model.DueDate,
                Status = model.Status,
                Priority = model.Priority
            };
            await _taskService.CreateTaskAsync(taskDto);

            return RedirectToAction("Dashboard", "Project", new { Id = model.ProjectId });
        }

    }
}

