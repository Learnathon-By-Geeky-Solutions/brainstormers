using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using TaskForge.Application.Interfaces.Services;

namespace TaskForge.Infrastructure.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetCurrentUserIdAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
                return string.Empty;

            var currentUser = await _userManager.GetUserAsync(user);
            return currentUser?.Id ?? string.Empty;
        }
    }
}
