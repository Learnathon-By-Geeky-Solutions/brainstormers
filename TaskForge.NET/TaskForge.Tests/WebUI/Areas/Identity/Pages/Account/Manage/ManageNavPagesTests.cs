using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TaskForge.WebUI.Areas.Identity.Pages.Account.Manage;
using Xunit;

namespace TaskForge.Tests.WebUI.Areas.Identity.Pages.Account.Manage
{
    public class ManageNavPagesTests
    {
        private ViewContext GetViewContextWithActivePage(string? activePage = null, string? displayName = null)
        {
            var viewData = new ViewDataDictionary(
                new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary());

            if (activePage != null)
                viewData["ActivePage"] = activePage;

            return new ViewContext
            {
                ViewData = viewData,
                ActionDescriptor = new ActionDescriptor
                {
                    DisplayName = displayName ?? "Index.cshtml"
                },
                RouteData = new RouteData(),
            };
        }

        [Theory]
        [InlineData("Index", "active")]
        [InlineData("index", "active")]
        [InlineData("Email", null)]
        public void IndexNavClass_ShouldReturnExpectedValue(string activePage, string? expected)
        {
            // Arrange
            var context = GetViewContextWithActivePage(activePage);
            // Act
            var result = ManageNavPages.IndexNavClass(context);
            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Email", "Email", "active")]
        [InlineData("email", "Email", "active")]
        [InlineData("WrongPage", "Email", null)]
        public void PageNavClass_ShouldMatchExpectedBehavior(string activePage, string pageToCheck, string? expected)
        {
            // Arrange
            var context = GetViewContextWithActivePage(activePage);
            // Act
            var result = ManageNavPages.PageNavClass(context, pageToCheck);
            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void PageNavClass_ShouldFallbackToDisplayName_WhenActivePageMissing()
        {
            // Arrange
            var context = GetViewContextWithActivePage(null, "DownloadPersonalData.cshtml");
            // Act
            var result = ManageNavPages.PageNavClass(context, "DownloadPersonalData");
            // Assert
            Assert.Equal("active", result);
        }

        [Theory]
        [InlineData("Email", nameof(ManageNavPages.EmailNavClass))]
        [InlineData("ChangePassword", nameof(ManageNavPages.ChangePasswordNavClass))]
        [InlineData("DownloadPersonalData", nameof(ManageNavPages.DownloadPersonalDataNavClass))]
        [InlineData("DeletePersonalData", nameof(ManageNavPages.DeletePersonalDataNavClass))]
        [InlineData("ExternalLogins", nameof(ManageNavPages.ExternalLoginsNavClass))]
        [InlineData("PersonalData", nameof(ManageNavPages.PersonalDataNavClass))]
        [InlineData("TwoFactorAuthentication", nameof(ManageNavPages.TwoFactorAuthenticationNavClass))]
        public void NavClassHelpers_ShouldReturnActive_WhenActivePageMatches(string activePage, string methodName)
        {
            // Arrange
            var context = GetViewContextWithActivePage(activePage);
            // Act
            string? result = methodName switch
            {
                nameof(ManageNavPages.EmailNavClass) => ManageNavPages.EmailNavClass(context),
                nameof(ManageNavPages.ChangePasswordNavClass) => ManageNavPages.ChangePasswordNavClass(context),
                nameof(ManageNavPages.DownloadPersonalDataNavClass) => ManageNavPages.DownloadPersonalDataNavClass(context),
                nameof(ManageNavPages.DeletePersonalDataNavClass) => ManageNavPages.DeletePersonalDataNavClass(context),
                nameof(ManageNavPages.ExternalLoginsNavClass) => ManageNavPages.ExternalLoginsNavClass(context),
                nameof(ManageNavPages.PersonalDataNavClass) => ManageNavPages.PersonalDataNavClass(context),
                nameof(ManageNavPages.TwoFactorAuthenticationNavClass) => ManageNavPages.TwoFactorAuthenticationNavClass(context),
                _ => null
            };
            // Assert
            Assert.Equal("active", result);
        }

        [Fact]
        public void StaticProperties_ShouldReturnExpectedStrings()
        {
            // Assert
            Assert.Equal("Index", ManageNavPages.Index);
            Assert.Equal("Email", ManageNavPages.Email);
            Assert.Equal("ChangePassword", ManageNavPages.ChangePassword);
            Assert.Equal("DownloadPersonalData", ManageNavPages.DownloadPersonalData);
            Assert.Equal("DeletePersonalData", ManageNavPages.DeletePersonalData);
            Assert.Equal("ExternalLogins", ManageNavPages.ExternalLogins);
            Assert.Equal("PersonalData", ManageNavPages.PersonalData);
            Assert.Equal("TwoFactorAuthentication", ManageNavPages.TwoFactorAuthentication);
        }
    }
}
