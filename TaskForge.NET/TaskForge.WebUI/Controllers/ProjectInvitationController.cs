using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Enums;
using TaskForge.WebUI.Models;

namespace TaskForge.WebUI.Controllers
{
    [Authorize(Roles = "Admin, User, Operator")]
    public class ProjectInvitationController : Controller
    {
        private readonly IProjectInvitationService _invitationService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserProfileService _userProfileService;

        public ProjectInvitationController(
            IProjectInvitationService invitationService,
            IProjectMemberService projectMemberService,
            IUserProfileService userProfileService,
            UserManager<IdentityUser> userManager)
        {
            _invitationService = invitationService;
            _projectMemberService = projectMemberService;
            _userManager = userManager;
            _userProfileService = userProfileService;
        }


        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");

            var user = await _userManager.GetUserAsync(User);
            string userId = user.Id;

            var userProfileId = await _userProfileService.GetByUserIdAsync(userId);
            if (userProfileId == null)
            {
                TempData["ErrorMessage"] = "User profile not found.";
                return RedirectToAction("Index", "Home");
            }

            var paginatedInvitations = await _invitationService.GetInvitationsForUserAsync(userProfileId, pageIndex, pageSize);
            if (paginatedInvitations.Items.Count == 0)
            {
                ViewData["NoInvitationsMessage"] = "You have no pending invitations.";
            }
            var projectInvitationViewModel = new ProjectInvitationListViewModel
            {
                Invitations = paginatedInvitations.Items.Select(invitation => new ProjectInvitationViewModel
                {
                    Id = invitation.Id,
                    ProjectTitle = invitation.Project.Title,
                    Status = invitation.Status.ToString(),
                    Role = invitation.AssignedRole.ToString(),
                    InvitationSentDate = invitation.InvitationSentDate.ToUniversalTime(),
                    AcceptedDate = invitation.AcceptedDate?.ToUniversalTime(),
                    DeclinedDate = invitation.DeclinedDate?.ToUniversalTime(),
                }).ToList(),
                PageIndex = paginatedInvitations.PageIndex,
                PageSize = paginatedInvitations.PageSize,
                TotalItems = paginatedInvitations.TotalCount,
                TotalPages = paginatedInvitations.TotalPages
            };

            return View(projectInvitationViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Create(InviteViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            // Restrict project access to assigned users only
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, viewModel.ProjectId);
            if (member == null || member.Role != ProjectRole.Admin)
            {
                return Forbid();
            }

            // Send Invitation
            var result = await _invitationService.AddAsync(viewModel.ProjectId, viewModel.InvitedUserEmail, viewModel.AssignedRole);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = "Failed to send invitation: " + result.Message;
                return RedirectToAction("ManageMembers", "Project", new { Id = viewModel.ProjectId });
            }

            // Redirect to Manage Members Page with success message
            TempData["SuccessMessage"] = "Invitation sent to " + viewModel.InvitedUserEmail + " successfully.";
            return RedirectToAction("ManageMembers", "Project", new { Id = viewModel.ProjectId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InvitationApprovalViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["ErrorMessage"] = string.Join(" | ", errorMessages);

                return RedirectToAction("Index");
            }

            // Fetch the invitation
            var invitation = await _invitationService.GetByIdAsync(viewModel.Id);
            if (invitation == null)
            {
                return NotFound("Invitation not found.");
            }

            // Prevent invalid status changes
            if (invitation.Status != InvitationStatus.Pending)
            {
                return BadRequest($"Cannot update an invitation that is already {invitation.Status.ToString().ToLower()}.");
            }


            await _invitationService.UpdateInvitationStatusAsync(viewModel.Id, viewModel.Status);
            return RedirectToAction("Index");
        }
    }
}