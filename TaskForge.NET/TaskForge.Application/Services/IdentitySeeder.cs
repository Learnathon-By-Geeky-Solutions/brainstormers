using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskForge.Application.Interfaces.Services;

namespace TaskForge.Application.Services
{
    public class IdentitySeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        IUserProfileService userProfileService,
        IConfiguration configuration,
        ILogger<IdentitySeeder> logger)
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly IUserProfileService _userProfileService = userProfileService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<IdentitySeeder> _logger = logger;

        public async Task SeedRolesAndSuperUser()
        {
            await SeedRoles();
            await SeedSuperAdmin();
        }

        private async Task SeedRoles()
        {
            string[] roles = { "Admin", "User", "Operator" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task SeedSuperAdmin()
        {
            var (adminEmail, adminPassword) = GetSuperAdminCredentials();
            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                _logger.LogWarning("Super admin email or password is not set in configuration.");
                return;
            }

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = await CreateAdminUser(adminEmail, adminPassword);
                if (adminUser == null) return;
            }
            else
            {
                _logger.LogInformation("Super admin user already exists.");
            }

            await EnsureAdminProfileExists(adminUser);
        }

        private (string? Email, string? Password) GetSuperAdminCredentials()
        {
            return (
                _configuration["SuperAdmin:Email"],
                _configuration["SuperAdmin:Password"]
            );
        }

        private async Task<IdentityUser?> CreateAdminUser(string email, string password)
        {
            try
            {
                var adminUser = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, password);
                if (!result.Succeeded)
                {
                    _logger.LogError("Error creating super admin user: {Errors}", string.Join(", ", result.Errors));
                    return null;
                }

                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Super admin user created successfully.");
                return adminUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating super admin user.");
                return null;
            }
        }

        private async Task EnsureAdminProfileExists(IdentityUser adminUser)
        {
            if (string.IsNullOrEmpty(adminUser?.Id))
            {
                _logger.LogError("Super admin user is null. Cannot create UserProfile.");
                return;
            }

            var adminUserProfileId = await _userProfileService.GetUserProfileIdByUserIdAsync(adminUser.Id);
            if (adminUserProfileId == 0)
            {
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
