using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Enums
{
    public class InvitationStatusTests
    {
        [Theory]
        [InlineData(InvitationStatus.Pending, "Pending")]
        [InlineData(InvitationStatus.Accepted, "Accepted")]
        [InlineData(InvitationStatus.Declined, "Declined")]
        [InlineData(InvitationStatus.Canceled, "Canceled")]
        public void InvitationStatus_DisplayName_ShouldMatch(InvitationStatus status, string expectedName)
        {
            var name = status.GetDisplayName();
            Assert.Equal(expectedName, name);
        }
    }
}