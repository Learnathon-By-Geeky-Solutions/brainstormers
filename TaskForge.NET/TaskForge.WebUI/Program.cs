using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskForge.Application.Helpers.DependencyResolvers;
using TaskForge.Application.Helpers.TaskSorters;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Services;
using TaskForge.Infrastructure;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories;
using TaskForge.Infrastructure.Repositories.Common;
using TaskForge.Infrastructure.Services;

namespace TaskForge.WebUI
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog to log to a file
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()  // Keep default console logging
                .WriteTo.File("logs/taskforge-log.txt", rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)  // Only log warnings and above to file
                .CreateLogger();

            builder.Host.UseSerilog();  // Replace default logging with Serilog

            // Get the database connection string from configuration
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Configure the database context and use SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));  // UseSqlServer extension

            // Configure email sender service (can be replaced with a real email sender)
            builder.Services.AddTransient<IEmailSender, MockEmailSender>(); // Or use SendGridEmailSender

            // Add Identity services for custom IdentityUser and IdentityRole
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
                options.SignIn.RequireConfirmedAccount = false)  // Email confirmation option (optional)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Cookie configuration (important for setting the authentication cookie)
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "TaskForge_Auth";  // Cookie name (make sure it’s unique)
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensures secure cookies over HTTPS
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true; // Extend session on activity
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Auto logout after inactivity
            });

            // Configure IdentityServer for authentication and authorization
            builder.Services.AddIdentityServer()
                .AddAspNetIdentity<IdentityUser>()
                .AddInMemoryClients(IdentityServerConfig.GetClients())
                .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
                .AddDeveloperSigningCredential();

            // Register the IdentitySeeder service
            builder.Services.AddTransient<IdentitySeeder>();

            // Add services for controllers and Razor Pages
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();


            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


            // Register Generic Repository
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IUserContextService, UserContextService>();


            builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
            builder.Services.AddScoped<ITaskRepository, TaskRepository>();
            builder.Services.AddScoped<IProjectInvitationRepository, ProjectInvitationRepository>();
            builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
            builder.Services.AddScoped<ITaskAttachmentRepository, TaskAttachmentRepository>();
            builder.Services.AddScoped<ITaskAssignmentRepository, TaskAssignmentRepository>();
            builder.Services.AddScoped<ITaskDependencyRepository, TaskDependencyRepository>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<IProjectMemberService, ProjectMemberService>();
            builder.Services.AddScoped<IProjectInvitationService, ProjectInvitationService>();
            builder.Services.AddScoped<IFileService, FileService>();


            builder.Services.AddScoped<IDependentTaskStrategy, RecursiveDependentTaskStrategy>();
            builder.Services.AddScoped<ITaskSorter, TopologicalTaskSorter>();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts(); // Enforce HTTPS security
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Use IdentityServer, Authentication, and Authorization
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();  // Add Razor Pages for Authentication UI

            // Configure default routing for MVC controllers
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Seed default roles and super admin user
            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
                await seeder.SeedRolesAndSuperUser();
            }

            await app.RunAsync();
        }
    }
}