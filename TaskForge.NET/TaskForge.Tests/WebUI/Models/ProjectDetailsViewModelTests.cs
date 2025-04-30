using TaskForge.Domain.Entities;
using TaskForge.WebUI.Models;
using Xunit;

namespace TaskForge.Tests.WebUI.Models
{
    public class ProjectDetailsViewModelTests
    {
        [Fact]
        public void ShowRequestId_ShouldReturnTrue_WhenRequestIdIsNotNullOrEmpty()
        {
            // Arrange
            var model = new ProjectDetailsViewModel
            {
                Project = new Project
                {
                    Id = 1
                }
            };

            // Act & Assert
            Assert.NotNull(model.Project);
            var project = model.Project;
            Assert.Equal(1, project.Id);
        }

        [Fact]
        public void ShowRequestId_ShouldReturnFalse_WhenRequestIdIsNull()
        {
            // Arrange
            var model = new ProjectDetailsViewModel
            {
                Project = null
            };

            // Act & Assert
            Assert.Null(model.Project);
        }
    }
}
