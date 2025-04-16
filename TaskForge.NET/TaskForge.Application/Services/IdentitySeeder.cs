using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskForge.Application.Interfaces.Services;

namespace TaskForge.Application.Services
{
    public class IdentitySeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserProfileService _userProfileService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IdentitySeeder> _logger;

        public IdentitySeeder(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            IUserProfileService userProfileService,
            IConfiguration configuration,
            ILogger<IdentitySeeder> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userProfileService = userProfileService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedRolesAndSuperUser()
        {
            string[] roles = { "Admin", "User", "Operator" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = _configuration["SuperAdmin:Email"];
            var adminPassword = _configuration["SuperAdmin:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                _logger.LogWarning("Super admin email or password is not set in configuration.");
                return;
            }

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Super admin user created successfully.");
                }
            }
            else
            {
                _logger.LogInformation("Super admin user already exists.");
            }

            var adminUserProfileId = await _userProfileService.GetByUserIdAsync(adminUser.Id);
            if (adminUserProfileId == 0)
            {
                // Create UserProfile for Super Admin
                await _userProfileService.CreateUserProfileAsync(adminUser.Id, "Super Admin");

                _logger.LogInformation("Super admin UserProfile created successfully.");

            }
            else
            {
                _logger.LogInformation("Super admin UserProfile already exists.");
            }

        }
    }

}
