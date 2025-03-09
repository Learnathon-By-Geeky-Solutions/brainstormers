using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Infrastructure.Data;
using TaskForge.WebUI.Models; // Your ViewModel for displaying invitations

namespace TaskForge.WebUI.Controllers
{
    [Authorize(Roles = "Admin, User, Operator")]
    public class ProjectInvitationController : Controller
    {
        private readonly IProjectInvitationService _invitationService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProjectInvitationController(ApplicationDbContext context, 
            IProjectInvitationService invitationService,
            IProjectMemberService projectMemberService,
            UserManager<IdentityUser> userManager)
        {
            _invitationService = invitationService;
            _projectMemberService = projectMemberService;
            _userManager = userManager;
            _context = context;
        }

        // Action method to display the current user's invitations
        public async Task<IActionResult> Index()
        {
            // Get the invitations for the logged-in user
            var invitations = _context.ProjectInvitations.Include(i => i.Project)
                                    .Where(i => i.InvitedUserProfile.User.UserName == User.Identity.Name).ToList();

            // Map the invitations to a ViewModel for display
            var invitationViewModels = invitations.Select(invitation => new ProjectInvitationViewModel
            {
                Id = invitation.Id,
                ProjectTitle = invitation.Project.Title,
                Status = invitation.Status.ToString(),
                Role = invitation.AssignedRole.ToString(),
                InvitationSentDate = invitation.InvitationSentDate,
                AcceptedDate = invitation.AcceptedDate,
                DeclinedDate = invitation.DeclinedDate
            }).ToList();

            // Return the view with the list of invitations
            return View(invitationViewModels);
        }


        [HttpPost]
        public async Task<IActionResult> Create(InviteViewModel viewModel)
        {
            // Restrict project access to assigned users only
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var member = await _projectMemberService.GetUserProjectRoleAsync(user.Id, viewModel.ProjectId);
            if (member == null || member.Role != ProjectRole.Admin)
            {
                return Forbid(); // User is not an Admin, access denied
            }

            // Send Invitation
            var success = await _invitationService.AddAsync(viewModel.ProjectId, viewModel.InvitedUserEmail, viewModel.AssignedRole);
            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to send invitation.";
                return RedirectToAction("ManageMembers", "Project", new { Id = viewModel.ProjectId });
            }

            // Redirect to Manage Members Page with success message
            TempData["SuccessMessage"] = "Invitation sent to " + viewModel.InvitedUserEmail + " successfully.";
            return RedirectToAction("ManageMembers", "Project", new { Id = viewModel.ProjectId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InviteViewModel viewModel)
        {
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