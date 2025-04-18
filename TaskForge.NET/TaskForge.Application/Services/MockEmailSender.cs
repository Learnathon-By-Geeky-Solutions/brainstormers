using Microsoft.AspNetCore.Identity.UI.Services;

namespace TaskForge.Application.Services
{
    public class MockEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Log or print the email details (or simulate sending an email)
            Console.WriteLine($"Email sent to {email} with subject: {subject}");
            return Task.CompletedTask;
        }
    }
}
