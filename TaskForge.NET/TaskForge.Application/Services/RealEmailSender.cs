using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace TaskForge.Application.Services
{
	public class RealEmailSender : IEmailSender
	{
		private readonly EmailSettings _settings;

		public RealEmailSender(IOptions<EmailSettings> settings)
		{
			_settings = settings.Value;
		}

		public async Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			using var client = new SmtpClient(_settings.Host)
			{
				Port = _settings.Port,
				Credentials = new NetworkCredential(_settings.Username, _settings.Password),
				EnableSsl = true,
			};

			using var mail = new MailMessage(_settings.Username, email, subject, htmlMessage)
			{
				IsBodyHtml = true
			};

			await client.SendMailAsync(mail);
		}
	}
}