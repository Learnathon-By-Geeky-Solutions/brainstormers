using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using TaskForge.WebUI.Models; // Your ViewModel for displaying invitations

namespace TaskForge.WebUI.Controllers
{
    [Authorize(Roles = "Admin, User, Operator")]
    public class ProjectInvitationController : Controller
    {
        private readonly IProjectInvitationService _invitationService;
        private readonly ApplicationDbContext _context;

        public ProjectInvitationController(ApplicationDbContext context, 
            IProjectInvitationService invitationService)
        {
            _invitationService = invitationService;
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
    }
}