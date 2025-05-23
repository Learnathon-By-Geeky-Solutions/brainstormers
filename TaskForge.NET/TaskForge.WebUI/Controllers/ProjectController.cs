﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Enums;
using TaskForge.WebUI.Models;

namespace TaskForge.WebUI.Controllers;

[Authorize(Roles = "Admin, User, Operator")]
public class ProjectController : Controller
{
    private readonly IProjectService _projectService;
    private readonly ITaskService _taskService;
    private readonly IProjectMemberService _projectMemberService;
    private readonly IProjectInvitationService _invitationService;
    private readonly UserManager<IdentityUser> _userManager;

    public ProjectController(
        IProjectMemberService projectMemberService,
        IProjectService projectService,
        ITaskService taskService,
        IProjectInvitationService invitationService,
        UserManager<IdentityUser> userManager)
    {
        _projectMemberService = projectMemberService;
        _projectService = projectService;
        _taskService = taskService;
        _invitationService = invitationService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ProjectFilterDto filter, int pageIndex = 1, int pageSize = 10)
    {
        if (!ModelState.IsValid) return RedirectToAction("Index");

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        filter.UserId = user.Id;
        var projectFilteredList = await _projectService.GetFilteredProjectsAsync(filter, pageIndex, pageSize);

        var projectList = new ProjectListViewModel
        {
            Filter = filter,
            FilteredProjectList = projectFilteredList.Items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalItems = projectFilteredList.TotalCount,
            TotalPages = projectFilteredList.TotalPages
        };

        return View(projectList);
    }

    // GET: Project/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateProjectViewModel
        {
            Title = "",
            Status = 0,
            StatusOptions = await _projectService.GetProjectStatusOptions(),
            StartDate = DateTime.UtcNow,
            EndDate = null
        };

        return View(viewModel);
    }

    // POST: Projects/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProjectViewModel createProjectViewModel)
    {
        if (!ModelState.IsValid)
        {
            createProjectViewModel.StatusOptions = await _projectService.GetProjectStatusOptions();
            return View(createProjectViewModel);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var createNewProject = new CreateProjectDto
        {
            Title = createProjectViewModel.Title,
            Description = createProjectViewModel.Description,
            Status = createProjectViewModel.Status,
            CreatedBy = user.Id,
            StartDate = createProjectViewModel.StartDate.ToUniversalTime(),
            EndDate = createProjectViewModel.EndDate?.ToUniversalTime()
        };

        await _projectService.CreateProjectAsync(createNewProject);

        return RedirectToAction("Index");
    }

    // POST: Projects/Update
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProjectUpdateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            var errorMessages = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            TempData["ValidationErrors"] = string.Join(" | ", errorMessages);

            return RedirectToAction("Dashboard");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var existingProject = await _projectService.GetProjectByIdAsync(viewModel.Id);
        if (existingProject == null) return NotFound();

        var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, existingProject.Id);
        if (member == null || member.Role != ProjectRole.Admin) return Forbid();

        // Update properties
        existingProject.Title = viewModel.Title;
        existingProject.Description = viewModel.Description;
        existingProject.StartDate = viewModel.StartDate.ToUniversalTime();
        existingProject.SetEndDate(viewModel.EndDateInput);
        existingProject.Status = viewModel.Status;
        existingProject.UpdatedBy = user.Id;
        existingProject.UpdatedDate = DateTime.UtcNow;

        await _projectService.UpdateProjectAsync(existingProject);

        return RedirectToAction("Dashboard", "Project", new { id = existingProject.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        if(!ModelState.IsValid) return BadRequest();

        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();

        var model = new ProjectUpdateViewModel
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            Status = project.Status,
            StartDate = project.StartDate.ToUniversalTime(),
            EndDateInput = project.EndDate?.ToUniversalTime()
        };

        return PartialView("_EditProjectForm", model);
    }


    // GET: Project/Dashboard/5
    public async Task<IActionResult> Dashboard(int id)
    {
        if (!ModelState.IsValid) return View();

        // Restrict project access to assigned users only
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, id);
        if (member == null) return Forbid();

        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();

        var taskList = (await _taskService.GetTaskListAsync(project.Id)).ToList();

        var sortedTodoTasks = await _taskService.GetSortedTasksAsync(TaskWorkflowStatus.ToDo, id) ?? [];
        var sortedInProgressTasks = await _taskService.GetSortedTasksAsync(TaskWorkflowStatus.InProgress, id) ?? [];
        var sortedCompletedTasks = await _taskService.GetSortedTasksAsync(TaskWorkflowStatus.Done, id) ?? [];
        var sortedBlockedTasks = await _taskService.GetSortedTasksAsync(TaskWorkflowStatus.Blocked, id) ?? [];


        var model = new ProjectDashboardViewModel
        {
            ProjectId = project.Id,
            ProjectTitle = project.Title,
            ProjectDescription = project.Description ?? "",
            ProjectStatus = project.Status,
            StartDate = project.StartDate.ToUniversalTime(),
            EndDate = project.EndDate?.ToUniversalTime(),
            UserRoleInThisProject = member.Role,
            TotalTasks = taskList.Count,
            PendingTasks = taskList.Count(t => t.Status == TaskWorkflowStatus.ToDo),
            CompletedTasks = taskList.Count(t => t.Status == TaskWorkflowStatus.Done),
            Members = (await _projectMemberService.GetProjectMembersAsync(project.Id)).Select(m =>
                new ProjectMemberViewModel
                {
                    Id = m.Id,
                    Name = m.Name ?? "",
                    Email = m.Email ?? "",
                    UserId = m.UserId,
                    Role = m.Role
                }).ToList(),
            Invitations = (await _invitationService.GetInvitationListAsync(project.Id, 1, 10)).Items.Select(m =>
                new InviteViewModel
                {
                    Id = m.Id,
                    ProjectId = m.ProjectId,
                    InvitedUserEmail = m.InvitedUserProfile?.User?.UserName ?? "No User",
                    Status = m.Status,
                    InvitationSentDate = m.InvitationSentDate.ToUniversalTime(),
                    AssignedRole = m.AssignedRole
                }).ToList(),
            TaskItems = taskList.Select(t => new TaskItemViewModel
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                StartDate = t.StartDate?.ToUniversalTime(),
                DueDate = t.DueDate?.ToUniversalTime(),
                AssignedUsers = t.AssignedUsers.Select(a => new TaskAssignmentViewModel
                {
                    UserId = a.UserProfile.UserId ?? "",
                    UserProfileId = a.UserProfileId,
                    FullName = a.UserProfile.FullName ?? "",
                    UserName = a.UserProfile.User.UserName ?? "",
                    Email = a.UserProfile.User.Email ?? "",
                    AvatarUrl = a.UserProfile.AvatarUrl
                }).ToList()
            }).ToList(),
            SortedTodoTasks = sortedTodoTasks,
            SortedInProgressTasks = sortedInProgressTasks,
            SortedCompletedTasks = sortedCompletedTasks,
            SortedBlockedTasks = sortedBlockedTasks,
            UpdateViewModel = new ProjectUpdateViewModel
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                StartDate = project.StartDate.ToUniversalTime(),
                Status = project.Status,
                EndDateInput = project.EndDate?.ToUniversalTime()
            }
        };

        return View(model);
    }

    // GET: Project/ManageMembers
    [HttpGet]
    public async Task<IActionResult> ManageMembers(int Id, int pageIndex = 1, int pageSize = 10)
    {
        if (!ModelState.IsValid) return View();

        // Restrict project access to assigned users only
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, Id);
        if (member == null || member.Role != ProjectRole.Admin) return Forbid();

        var project = await _projectService.GetProjectByIdAsync(Id);
        if (project == null) return NotFound();

        var projectMembers = await _projectMemberService.GetProjectMembersAsync(Id);
        var projectInvitations = await _invitationService.GetInvitationListAsync(Id, pageIndex, pageSize);

        var manageMembersViewModel = new ManageMembersViewModel
        {
            ProjectId = Id,
            ProjectTitle = project.Title,
            ProjectDescription = string.IsNullOrEmpty(project.Description) ? "No Description" : project.Description,
            ProjectMembers = projectMembers.Select(m => new ProjectMemberViewModel
            {
                Id = m.Id,
                UserId = m.UserId!,
                Name = m.Name ?? "Unknown",
                Email = m.Email ?? "N/A",
                Role = m.Role
            }).ToList(),
            ProjectInvitations = new PaginatedList<InviteViewModel>(
                projectInvitations.Items.Select(pi => new InviteViewModel
                {
                    Id = pi.Id,
                    ProjectId = pi.ProjectId,
                    InvitedUserEmail = pi.InvitedUserProfile?.User?.Email ?? "N/A",
                    Status = pi.Status,
                    InvitationSentDate = pi.InvitationSentDate.ToUniversalTime(),
                    AssignedRole = pi.AssignedRole
                }).ToList(),
                projectInvitations.TotalCount,
                pageIndex,
                pageSize
            )
        };

        return View(manageMembersViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int id)
    {
        if (!ModelState.IsValid)
        {
            var errorMessages = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            TempData["ValidationErrors"] = string.Join(" | ", errorMessages);

            return RedirectToAction("ManageMembers");
        }

        // Fetch the member
        var member = await _projectMemberService.GetByIdAsync(id);
        if (member == null) return NotFound("Member not found.");

        // Restrict project access to assigned users only
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var userMember = await _projectMemberService.GetUserProjectRoleAsync(user.Id, member.ProjectId);
        if (userMember == null || userMember.Role != ProjectRole.Admin) return Forbid();

        if (userMember.Id == member.Id) return BadRequest("Invalid request.");

        // Remove the member from the project
        await _projectMemberService.RemoveAsync(id);

        // Redirect back to the Project Management Members page
        return RedirectToAction("ManageMembers", "Project", new { Id = member.ProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelInvitation(int id)
    {
        if (!ModelState.IsValid)
        {
            var errorMessages = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            TempData["ValidationErrors"] = string.Join(" | ", errorMessages);

            return RedirectToAction("ManageMembers");
        }

        // Fetch the invitation
        var invitation = await _invitationService.GetByIdAsync(id);
        if (invitation == null) return NotFound("Invitation not found.");

        // Restrict project access to assigned users only
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var userMember = await _projectMemberService.GetUserProjectRoleAsync(user.Id, invitation.ProjectId);
        if (userMember == null || userMember.Role != ProjectRole.Admin) return Forbid();

        // Prevent cancellation if the status is not "Pending"
        if (invitation.Status != InvitationStatus.Pending)
            return BadRequest($"Cannot cancel an invitation that is already {invitation.Status.ToString().ToLower()}.");

        invitation.Status = InvitationStatus.Canceled;

        // Update the invitation status in the database
        await _invitationService.UpdateInvitationStatusAsync(id, invitation.Status);

        // Redirect back to the Project Management Members page
        return RedirectToAction("ManageMembers", "Project", new { Id = invitation.ProjectId });
    }
}