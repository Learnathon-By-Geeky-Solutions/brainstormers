using Microsoft.AspNetCore.Mvc;
using TaskForge.Domain.Entities;
using TaskForge.WebUI.Models;
using System;
using System.Linq;
using TaskForge.Domain.Enums;
using TaskForge.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace TaskForge.WebUI.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<IdentityUser> _userManager;
        public TaskController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
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

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                var taskItem = new TaskItem
                {
                    Title = model.Title,
                    Description = model.Description,
                    ProjectId = model.ProjectId,
                    Status = TaskWorkflowStatus.ToDo,
                    Priority = TaskPriority.Medium,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = user.Id
                };
                taskItem.SetDueDate(model.DueDate);

                _context.TaskItems.Add(taskItem);
                _context.SaveChangesAsync();

                //// If DueDate is null, send a fallback value
                //var taskDueDate = taskItem.DueDate?.ToString("yyyy-MM-dd") ?? "No Due Date";

            }

            return RedirectToAction("Details", "Project", new { Id = model.ProjectId });
        }


    }
}

