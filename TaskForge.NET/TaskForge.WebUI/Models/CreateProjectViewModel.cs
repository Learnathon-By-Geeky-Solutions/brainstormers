using Microsoft.AspNetCore.Mvc.Rendering;
using TaskForge.Application.DTOs;
using TaskForge.Domain.Enums;

namespace TaskForge.WebUI.Models
{
    public class CreateProjectViewModel
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required ProjectStatus Status { get; set; }

        public IEnumerable<SelectListItem>? StatusOptions { get; set; }
    }
}
