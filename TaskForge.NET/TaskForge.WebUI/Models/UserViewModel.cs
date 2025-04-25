using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskForge.WebUI.Models;

public class FilterUserListViewModel
{
    public List<UserListItemViewModel> Users { get; set; } = [];

    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
}

public class UserListItemViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? Role { get; set; }
}

public class UserCreateViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public IEnumerable<SelectListItem>? AvailableRoles { get; set; }
}