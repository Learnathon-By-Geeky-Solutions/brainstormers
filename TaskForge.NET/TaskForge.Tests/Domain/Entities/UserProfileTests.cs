using TaskForge.Domain.Entities;
using Xunit;

namespace TaskForge.Tests.Domain.Entities
{
    public class UserProfileTests
    {
        [Fact]
        public void UserProfile_Should_Set_And_Get_Properties()
        {
            var userProfile = new UserProfile
            {
                Id = 1,
                FullName = "Jane Doe",
                AvatarUrl = "https://example.com/avatar.jpg",
                PhoneNumber = "+1234567890",
                Location = "New York",
                JobTitle = "Software Engineer",
                Company = "TechCorp",
                ProfessionalSummary = "Experienced in .NET and React",
                LinkedInProfile = "https://linkedin.com/in/janedoe",
                WebsiteUrl = "https://janedoe.dev",
                UserId = "abc123",
                CreatedBy = "admin"
            };

            Assert.Equal(1, userProfile.Id);
            Assert.Equal("Jane Doe", userProfile.FullName);
            Assert.Equal("https://example.com/avatar.jpg", userProfile.AvatarUrl);
            Assert.Equal("+1234567890", userProfile.PhoneNumber);
            Assert.Equal("New York", userProfile.Location);
            Assert.Equal("Software Engineer", userProfile.JobTitle);
            Assert.Equal("TechCorp", userProfile.Company);
            Assert.Equal("Experienced in .NET and React", userProfile.ProfessionalSummary);
            Assert.Equal("https://linkedin.com/in/janedoe", userProfile.LinkedInProfile);
            Assert.Equal("https://janedoe.dev", userProfile.WebsiteUrl);
            Assert.Equal("abc123", userProfile.UserId);
            Assert.Equal("admin", userProfile.CreatedBy);
        }
    }
}
