using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.WebUI.Models;

namespace TaskForge.WebUI.Controllers;

[Authorize(Roles = "Admin,Operator")]
public class UserManagementController : Controller
{
    private readonly IUserService _userService;

    public UserManagementController(IUserService userService)
    {
        _userService = userService;
    }


    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, string? roleFilter, int pageIndex = 1, int pageSize = 10)
    {
        var userFilterDto = new UserFilterDto
        {
            SearchTerm = searchTerm,
            RoleFilter = roleFilter
        };

        var result = await _userService.GetFilteredUsersAsync(userFilterDto, pageIndex, pageSize);

        var viewModel = new FilterUserListViewModel
        {
            SearchTerm = searchTerm,
            RoleFilter = roleFilter,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalPages = result.TotalPages,
            Users = result.Items.Select(u => new UserListItemViewModel
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.FullName ?? "",
                AvatarUrl = u.AvatarUrl,
                PhoneNumber = u.PhoneNumber,
                JobTitle = u.JobTitle,
                Company = u.Company,
                Role = u.Role
            }).ToList()
        };

        ViewBag.Roles = (await _userService.GetAllRolesAsync()).Select(r => r.Name);

        return View(viewModel);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var roles = await _userService.GetAllRolesAsync();

        var model = new UserCreateViewModel
        {
            AvailableRoles = roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            })
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = (await _userService.GetAllRolesAsync())
                .Select(r => new SelectListItem { Text = r.Name, Value = r.Name });
            return View(model);
        }

        var dto = new UserCreateDto
        {
            Email = model.Email,
            Password = model.Password,
            PhoneNumber = model.PhoneNumber,
            FullName = model.FullName,
            Role = model.Role
        };

        var result = await _userService.CreateUserAsync(dto);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            model.AvailableRoles = (await _userService.GetAllRolesAsync())
                .Select(r => new SelectListItem { Text = r.Name, Value = r.Name });

            TempData["SuccessMessage"] = null;
            TempData["ErrorMessage"] = "Create: There was an issue creating the user. Please try again.";
            return View(model);
        }

        TempData["SuccessMessage"] = "Create: User successfully created.";
        TempData["ErrorMessage"] = null;

        return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        if (user.Email == User.Identity?.Name || (User.IsInRole("Operator") && user.Role != "User")) return Forbid();

        var userViewModel = new UserListItemViewModel
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName ?? "",
            AvatarUrl = user.AvatarUrl,
            PhoneNumber = user.PhoneNumber,
            JobTitle = user.JobTitle,
            Company = user.Company,
            Role = user.Role
        };

        return View(userViewModel);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        if (user.Email == User.Identity?.Name || (User.IsInRole("Operator") && user.Role != "User")) return Forbid();

        var result = await _userService.DeleteUserAsync(id);

        if (result)
        {
            TempData["SuccessMessage"] = "Delete: User successfully deleted.";
            TempData["ErrorMessage"] = null;
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = null;
        TempData["ErrorMessage"] = "Delete: There was an issue deleting the user. Please try again.";
        return RedirectToAction(nameof(Delete), new { id });
    }
}