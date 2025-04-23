using TaskForge.WebUI.Models;
using Xunit;

namespace TaskForge.Tests.WebUI.Models
{
    public class ErrorViewModelTests
    {
        [Fact]
        public void ShowRequestId_ShouldReturnTrue_WhenRequestIdIsNotNullOrEmpty()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "12345" };

            // Act & Assert
            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_ShouldReturnFalse_WhenRequestIdIsNull()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = null };

            // Act & Assert
            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_ShouldReturnFalse_WhenRequestIdIsEmpty()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "" };

            // Act & Assert
            Assert.False(model.ShowRequestId);
        }
    }
}
