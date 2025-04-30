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
	public string? LinkedInProfile { get; set; }

	[MaxLength(500)]
	public string? WebsiteUrl { get; set; }
}
