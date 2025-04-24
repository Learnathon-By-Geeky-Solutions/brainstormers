using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TaskForge.Domain.Enums;
using TaskForge.WebUI.Models;
using Xunit;

namespace TaskForge.Tests.WebUI.Models
{
    public class CreateProjectViewModelTests
    {
        [Theory]
        [InlineData(0, true)]  // EndDate equals StartDate -> Error
        [InlineData(-1, true)] // EndDate less than StartDate -> Error
        [InlineData(1, false)] // EndDate greater than StartDate -> No Error
        [InlineData(null, false)] // EndDate is null -> No Error
        public void Validate_EndDateValidation_ShouldBehaveAsExpected(int? daysToAdd, bool expectError)
        {
            // Arrange
            var startDate = DateTime.Now;
            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Status = ProjectStatus.OnHold,
                StartDate = startDate,
                EndDate = daysToAdd.HasValue ? startDate.AddDays(daysToAdd.Value) : null
            };

            // Act
            var results = new List<ValidationResult>(model.Validate(new ValidationContext(model)));

            // Assert
            if (expectError)
            {
                Assert.Single(results);
                Assert.Equal("EndDate must be greater than StartDate", results[0].ErrorMessage);
                Assert.Equal(nameof(CreateProjectViewModel.EndDate), results[0].MemberNames.Single());
            }
            else
            {
                Assert.Empty(results);
            }
        }
    }
}