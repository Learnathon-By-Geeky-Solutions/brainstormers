﻿namespace TaskForge.Application.DTOs;

public class UserFilterDto
{
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
}

public class UserListItemDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? Role { get; set; }
}

public class UserCreateDto
{
    public required string Email { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public string? FullName { get; set; } = string.Empty;
    public string? Role { get; set; } = string.Empty;
}