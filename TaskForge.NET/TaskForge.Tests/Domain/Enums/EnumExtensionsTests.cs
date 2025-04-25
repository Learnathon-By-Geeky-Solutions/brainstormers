using System.ComponentModel.DataAnnotations;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Enums
{
    public enum Sample
    {
        [Display(Name = "Test Display")]
        WithDisplay,
        WithoutDisplay
    }

    public class EnumExtensionsTests
    {
        [Fact]
        public void GetDisplayName_ShouldReturnDisplayName_IfPresent()
        {
            var result = Sample.WithDisplay.GetDisplayName();

            Assert.Equal("Test Display", result);
        }

        [Fact]
        public void GetDisplayName_ShouldReturnEnumName_IfNoDisplay()
        {
            var result = Sample.WithoutDisplay.GetDisplayName();

            Assert.Equal("WithoutDisplay", result);
        }
    }
}
