using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Services;
using TaskForge.WebUI.Models;

namespace TaskForge.WebUI.Controllers
{
    [Authorize(Roles = "Admin,Operator")]
    public class UserManagementController : Controller
    {
        private readonly IUserService _userService;

        public UserManagementController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, string? roleFilter, int pageIndex = 1,
            int pageSize = 10)
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


        public async Task<IActionResult> Delete(string id)
        {
            await _userService.DeleteUserAsync(id);
            return RedirectToAction("Index");
        }
    }
}