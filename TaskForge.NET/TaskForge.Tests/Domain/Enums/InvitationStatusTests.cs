using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Enums
{
    public class InvitationStatusTests
    {
        [Theory]
        [InlineData(InvitationStatus.Pending)]
        [InlineData(InvitationStatus.Accepted)]
        [InlineData(InvitationStatus.Declined)]
        [InlineData(InvitationStatus.Canceled)]
        public void InvitationStatus_ShouldHaveValidValues(InvitationStatus status)
        {
            Assert.True((int)status >= 0 && (int)status <= 3);
        }
    }
}