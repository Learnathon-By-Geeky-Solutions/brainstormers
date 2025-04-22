using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Domain.Entities
{
    public class ProjectInvitationTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaults()
        {
            // Act
            var invitation = new ProjectInvitation();

            // Assert
            Assert.Equal(InvitationStatus.Pending, invitation.Status);
            Assert.True((DateTime.UtcNow - invitation.InvitationSentDate).TotalSeconds < 5);
            Assert.Null(invitation.AcceptedDate);
            Assert.Null(invitation.DeclinedDate);
            Assert.Equal(ProjectRole.Viewer, invitation.AssignedRole);

            // These are null by default and should be explicitly set
            Assert.Null(invitation.Project);
            Assert.Null(invitation.InvitedUserProfile);
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var acceptedDate = now.AddHours(1);
            var declinedDate = now.AddHours(2);

            var invitation = new ProjectInvitation
            {
                ProjectId = 1,
                Project = new Project { Id = 1, Title = "Test Project" },
                InvitedUserProfileId = 2,
                InvitedUserProfile = new UserProfile { Id = 2 },
                Status = InvitationStatus.Accepted,
                InvitationSentDate = now,
                AcceptedDate = acceptedDate,
                DeclinedDate = declinedDate,
                AssignedRole = ProjectRole.Admin
            };

            // Assert
            Assert.Equal(1, invitation.ProjectId);
            Assert.Equal("Test Project", invitation.Project.Title);
            Assert.Equal(2, invitation.InvitedUserProfileId);
            Assert.Equal(2, invitation.InvitedUserProfile.Id);
            Assert.Equal(InvitationStatus.Accepted, invitation.Status);
            Assert.Equal(now, invitation.InvitationSentDate);
            Assert.Equal(acceptedDate, invitation.AcceptedDate);
            Assert.Equal(declinedDate, invitation.DeclinedDate);
            Assert.Equal(ProjectRole.Admin, invitation.AssignedRole);
        }
    }
}
