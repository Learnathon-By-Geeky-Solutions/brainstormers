using TaskForge.Application.Services;
using Xunit;

namespace TaskForge.Tests.Services
{
    public class MockEmailSenderTests
    {
        private readonly MockEmailSender _emailSender;

        public MockEmailSenderTests()
        {
            _emailSender = new MockEmailSender();
        }

        [Fact]
        public async Task SendEmailAsync_ShouldCompleteWithoutException()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject";
            var message = "<p>This is a test message.</p>";

            // Act
            var exception = await Record.ExceptionAsync(() =>
                _emailSender.SendEmailAsync(email, subject, message));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldWriteExpectedOutputToConsole()
        {
            // Arrange
            var email = "recipient@test.com";
            var subject = "Welcome!";
            var htmlMessage = "<h1>Hello!</h1>";

            using var sw = new StringWriter();
            Console.SetOut(sw);

            // Act
            await _emailSender.SendEmailAsync(email, subject, htmlMessage);

            // Assert
            var output = sw.ToString().Trim();
            Assert.Contains($"Email sent to {email} with subject: {subject}", output);
        }

        [Theory]
        [InlineData("", "No Subject", "<p>Empty Email</p>")]
        [InlineData("user@example.com", "", "<p>Empty Subject</p>")]
        [InlineData("user@example.com", "Subject", "")]
        public async Task SendEmailAsync_ShouldHandleEmptyParameters(string email, string subject, string htmlMessage)
        {
            // Act & Assert
            var exception = await Record.ExceptionAsync(() =>
                _emailSender.SendEmailAsync(email, subject, htmlMessage));
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldAllowNullHtmlMessage()
        {
            // Arrange
            var email = "nullhtml@example.com";
            var subject = "Null HTML";

            using var sw = new StringWriter();
            Console.SetOut(sw);

            // Act
            var exception = await Record.ExceptionAsync(() =>
                _emailSender.SendEmailAsync(email, subject, null!)); // null-forgiving

            // Assert
            Assert.Null(exception);
            var output = sw.ToString();
            Assert.Contains("Email sent to", output);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldBeCallableMultipleTimes()
        {
            // Arrange
            var email = "repeat@example.com";
            var subject = "Repeated Subject";
            var message = "Repeated Message";

            using var sw = new StringWriter();
            Console.SetOut(sw);

            // Act
            for (int i = 0; i < 5; i++)
            {
                await _emailSender.SendEmailAsync(email, subject, message);
            }

            // Assert
            var output = sw.ToString();
            var occurrences = output.Split("Email sent to").Length - 1;
            Assert.Equal(5, occurrences);
        }
    }
}
