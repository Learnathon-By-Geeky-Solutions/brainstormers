using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskForge.Application.Interfaces.Services;
using TaskForge.WebUI.Models;

namespace TaskForge.WebUI.Controllers;

[Authorize]
public class UserProfileController(IUserProfileService userProfileService, UserManager<IdentityUser> userManager) : Controller
{
    private readonly IUserProfileService _userProfileService = userProfileService;
    private readonly UserManager<IdentityUser> _userManager = userManager;

    public async Task<IActionResult> Setup()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }
        var profile = await _userProfileService.GetByUserIdAsync(userId);

        if (profile == null)
        {
            return Unauthorized();
        }

        var model = new UserProfileEditViewModel
        {
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            AvatarUrl = profile.AvatarUrl,
            Location = profile.Location,
            JobTitle = profile.JobTitle,
            Company = profile.Company,
            ProfessionalSummary = profile.ProfessionalSummary,
            LinkedInProfile = profile.LinkedInProfile,
            WebsiteUrl = profile.WebsiteUrl
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Setup(UserProfileEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var profile = await _userProfileService.GetByUserIdAsync(userId);
            if (profile == null)
                return NotFound();

            // Update properties
            profile.FullName = model.FullName;
            profile.PhoneNumber = model.PhoneNumber;
            profile.Location = model.Location;
            profile.JobTitle = model.JobTitle;
            profile.Company = model.Company;
            profile.ProfessionalSummary = model.ProfessionalSummary;
            profile.LinkedInProfile = model.LinkedInProfile;
            profile.WebsiteUrl = model.WebsiteUrl;

            if (model.AvatarImage?.Length > 0)
            {
                if (model.AvatarImage.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("AvatarImage", "Maximum file size is 10 MB.");
                    return View(model);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(model.AvatarImage.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("AvatarImage", "Only .jpg, .jpeg, and .png files are allowed.");
                    return View(model);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarImage.CopyToAsync(stream);
                }

                profile.AvatarUrl = "/uploads/avatars/" + fileName;
            }

            await _userProfileService.UpdateAsync(profile);

            return RedirectToAction("Setup", "UserProfile");
        }
        catch (Exception)
        {
            return new JsonResult("An error occurred while updating the profile. Please try again.");
        }
    }

}
