using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TaskForge.Domain.Enums;

namespace TaskForge.WebUI.Models
{
    public class CreateProjectViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Title can't be empty")]
        public required string Title { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public required ProjectStatus Status { get; set; }

        [Required(ErrorMessage = "StartDate can't be empty")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }

        public IEnumerable<SelectListItem>? StatusOptions { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate.HasValue && EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "EndDate must be greater than StartDate",
                    new[] { nameof(EndDate) });
            }
        }
    }
}
