using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using TaskForge.Application;
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

namespace TaskForge.WebUI;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog to log to a file
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/taskforge-log.txt", rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Warning)
            .CreateLogger();

        builder.Host.UseSerilog(); // Replace default logging with Serilog

        // Load environment variables from .env file if in development mode
        if (builder.Environment.IsDevelopment())
        {
            var envVars = Env.Load();

            // Inject into configuration manually
            foreach (var kvp in envVars)
            {
                Console.WriteLine($"Loaded environment variable: {kvp.Key} = {kvp.Value}");
                builder.Configuration[kvp.Key] = kvp.Value;
            }
        }

        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();


        // Get the database connection string from configuration
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not set.");
        }

        // Configure the database context and use SQL Server
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();

        if (emailSettings == null ||
            string.IsNullOrWhiteSpace(emailSettings.Host) ||
            string.IsNullOrWhiteSpace(emailSettings.Port.ToString()) ||
            string.IsNullOrWhiteSpace(emailSettings.Username) ||
            string.IsNullOrWhiteSpace(emailSettings.Password))
        {
            throw new InvalidOperationException("Email settings are not configured properly.");
        }

        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

        // Configure email sender service (can be replaced with a real email sender)
        builder.Services.AddTransient<IEmailSender, RealEmailSender>();


        // Add Identity services for custom IdentityUser and IdentityRole
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
                options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 1;
			options.SignIn.RequireConfirmedEmail = true;
		});

        // Cookie configuration (important for setting the authentication cookie)
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "TaskForge_Auth"; // Cookie name (make sure it’s unique)
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
            .AddDeveloperSigningCredential(persistKey: false);

        // Register the IdentitySeeder service
        builder.Services.AddTransient<IdentitySeeder>();

        // Add services for controllers and Razor Pages
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        // Register Generic Repository
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IUserContextService, UserContextService>();


        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<ITaskRepository, TaskRepository>();
        builder.Services.AddScoped<IProjectInvitationRepository, ProjectInvitationRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        builder.Services.AddScoped<ITaskAttachmentRepository, TaskAttachmentRepository>();
        builder.Services.AddScoped<ITaskAssignmentRepository, TaskAssignmentRepository>();
        builder.Services.AddScoped<ITaskDependencyRepository, TaskDependencyRepository>();
        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<ITaskSorter, TopologicalTaskSorter>();
        builder.Services.AddScoped<IDependentTaskStrategy, RecursiveDependentTaskStrategy>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IUserProfileService, UserProfileService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IProjectMemberService, ProjectMemberService>();
        builder.Services.AddScoped<IProjectInvitationService, ProjectInvitationService>();
        builder.Services.AddHttpContextAccessor();


        builder.Services.AddScoped<TaskServiceDependencies>(provider => new TaskServiceDependencies
        {
            UnitOfWork = provider.GetRequiredService<IUnitOfWork>(),
            TaskRepository = provider.GetRequiredService<ITaskRepository>(),
            ProjectMemberRepository = provider.GetRequiredService<IProjectMemberRepository>(),
            UserProfileRepository = provider.GetRequiredService<IUserProfileRepository>(),
            TaskAssignmentRepository = provider.GetRequiredService<ITaskAssignmentRepository>(),
            TaskAttachmentRepository = provider.GetRequiredService<ITaskAttachmentRepository>(),
            FileService = provider.GetRequiredService<IFileService>(),
            TaskSorter = provider.GetRequiredService<ITaskSorter>(),
            DependentTaskStrategy = provider.GetRequiredService<IDependentTaskStrategy>(),
            Logger = provider.GetRequiredService<ILogger<TaskService>>()
        });
        builder.Services.AddScoped<ITaskService, TaskService>();
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

        app.MapRazorPages(); // Add Razor Pages for Authentication UI

        // Configure default routing for MVC controllers
        app.MapControllerRoute(
            "default",
            "{controller=Home}/{action=Index}/{id?}");

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                // Apply migrations to the database to ensure tables are created
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();

                var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
                await seeder.SeedRolesAndSuperUser();
            }
            catch (Exception ex)
            {
                // Log any errors that occur during migration or seeding
                var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
                logger.LogError(ex, "An error occurred during migration or seeding");
                throw;
            }
        }

        await app.RunAsync();
    }
}