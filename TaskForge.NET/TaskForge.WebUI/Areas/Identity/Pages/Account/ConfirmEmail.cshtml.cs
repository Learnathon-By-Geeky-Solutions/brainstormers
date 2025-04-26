// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TaskForge.Application.Interfaces.Services;

namespace TaskForge.WebUI.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserProfileService _userProfileService;
        public ConfirmEmailModel(UserManager<IdentityUser> userManager, IUserProfileService userProfileService)
        {
            _userManager = userManager;
            _userProfileService = userProfileService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
			if (result.Succeeded && user.Email != null)
			{
				// Check if user profile already exists
				var existingProfileId = await _userProfileService.GetByUserIdAsync(userId);
				if (existingProfileId == null)
				{
					await _userProfileService.CreateUserProfileAsync(userId, user.Email.Split('@')[0]);
				}
			}
			StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return Page();
        }
    }
}
