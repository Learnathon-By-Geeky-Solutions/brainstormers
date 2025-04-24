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
        [Fact]
        public void Validate_WhenEndDateIsNull_ShouldNotReturnError()
        {
            // Arrange
            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Status = ProjectStatus.Completed,
                StartDate = DateTime.Now,
                EndDate = null // EndDate.HasValue == false
            };

            // Act
            var results = model.Validate(new ValidationContext(model));

            // Assert
            Assert.Empty(results); // No validation error
        }

        [Fact]
        public void Validate_WhenEndDateGreaterThanStartDate_ShouldNotReturnError()
        {
            // Arrange
            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Status = ProjectStatus.NotStarted,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1) // EndDate > StartDate
            };

            // Act
            var results = model.Validate(new ValidationContext(model));

            // Assert
            Assert.Empty(results); // No validation error
        }

        [Fact]
        public void Validate_WhenEndDateEqualsStartDate_ShouldReturnError()
        {
            // Arrange
            var startDate = DateTime.Now;
            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Status = ProjectStatus.OnHold,
                StartDate = startDate,
                EndDate = startDate // EndDate == StartDate
            };

            // Act
            var results = new List<ValidationResult>(model.Validate(new ValidationContext(model)));

            // Assert
            Assert.Single(results); // 1 error
            Assert.Equal("EndDate must be greater than StartDate", results[0].ErrorMessage);
            Assert.Equal(nameof(CreateProjectViewModel.EndDate), results[0].MemberNames.Single());
        }
        [Fact]
        public void Validate_WhenEndDateIsNull_ShouldNotYield()
        {
            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Status = ProjectStatus.OnHold,
                StartDate = DateTime.Now,
                EndDate = null // Explicitly test null case
            };
            var results = model.Validate(new ValidationContext(model));
            Assert.Empty(results); // Ensure no yield
        }
        [Fact]
        public void Validate_WhenEndDateGreaterThanStartDate_ShouldNotYield()
        {
            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Status = ProjectStatus.OnHold,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1) // Explicitly test > StartDate
            };
            var results = model.Validate(new ValidationContext(model));
            Assert.Empty(results); // Ensure no yield
        }
        [Fact]
        public void Validate_WhenEndDateLessThanStartDate_ShouldReturnError()
        {
            // Arrange
            var startDate = DateTime.Now;
            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Status = ProjectStatus.Cancelled,
                StartDate = startDate,
                EndDate = startDate.AddDays(-1) // EndDate < StartDate
            };

            // Act
            var results = new List<ValidationResult>(model.Validate(new ValidationContext(model)));

            // Assert
            Assert.Single(results); // 1 error
            Assert.Equal("EndDate must be greater than StartDate", results[0].ErrorMessage);
            Assert.Equal(nameof(CreateProjectViewModel.EndDate), results[0].MemberNames.Single());
        }
    }
}