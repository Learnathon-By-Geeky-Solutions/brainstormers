using System.ComponentModel.DataAnnotations;

namespace TaskForge.WebUI.Models;

public class UserProfileViewModel
{
    public string FullName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Location { get; set; }

    public string? JobTitle { get; set; }

    public string? Company { get; set; }

    public string? ProfessionalSummary { get; set; }

    public string? LinkedInProfile { get; set; }

    public string? WebsiteUrl { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

}


public class UserProfileEditViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(150)]
	public string FullName { get; set; } = string.Empty;

	public IFormFile? AvatarImage { get; set; }
	public string? AvatarUrl { get; set; }

	[MaxLength(20)]
	public string? PhoneNumber { get; set; }

	[MaxLength(250)]
	public string? Location { get; set; }

	[MaxLength(150)]
	public string? JobTitle { get; set; }

	[MaxLength(200)]
	public string? Company { get; set; }

	[MaxLength(2000)]
	public string? ProfessionalSummary { get; set; }

	[MaxLength(500)]
	[RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", ErrorMessage = "Please enter a valid URL")]
	public string? LinkedInProfile { get; set; }

	[MaxLength(500)]
	[RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", ErrorMessage = "Please enter a valid URL")]
	public string? WebsiteUrl { get; set; }
}



