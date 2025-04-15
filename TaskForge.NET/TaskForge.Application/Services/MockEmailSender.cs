using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
