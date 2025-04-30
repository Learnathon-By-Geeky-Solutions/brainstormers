using System.ComponentModel.DataAnnotations;

namespace TaskForge.WebUI.Models;
public class UserProfileEditViewModel
{
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
