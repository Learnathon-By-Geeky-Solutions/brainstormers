using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Configuration;
using TaskForge.Application.Interfaces.Services;

namespace TaskForge.Application.Services
{
	public class LinkGeneratorService : ILinkGeneratorService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly string _defaultBaseUrl;

		public LinkGeneratorService(IHttpContextAccessor httpContextAccessor, IOptions<AppSettings> appSettings)
		{
			_httpContextAccessor = httpContextAccessor;
			_defaultBaseUrl = appSettings.Value.BaseUrl;
		}

		public string GenerateInvitationLink(string returnUrl)
		{
			var request = _httpContextAccessor.HttpContext?.Request;

			string baseUrl = request != null
				? $"{request.Scheme}://{request.Host.Value}"
				: _defaultBaseUrl;

			return $"{baseUrl}/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}";
		}
	}

}
