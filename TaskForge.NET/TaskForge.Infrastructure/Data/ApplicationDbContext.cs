using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TaskForge.Domain.Entities;

namespace TaskForge.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSets
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<ProjectInvitation> ProjectInvitations { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }

        public override int SaveChanges()
        {
            ConvertDateTimeToUtc();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDateTimeToUtc();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ConvertDateTimeToUtc()
        {
            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                ProcessDateTimeProperties(entry);
            }
        }

        private static void ProcessDateTimeProperties(EntityEntry entry)
        {
            foreach (var property in entry.OriginalValues.Properties)
            {
                if (property.ClrType != typeof(DateTime) && property.ClrType != typeof(DateTime?))
                    continue;

                var currentValue = entry.Property(property.Name).CurrentValue;
                entry.Property(property.Name).CurrentValue = ConvertToUtc(currentValue);
            }
        }

        private static object? ConvertToUtc(object? dateTimeValue)
        {
            if (dateTimeValue is DateTime dt)
            {
                return ConvertDateTime(dt);
            }
            else if (dateTimeValue is DateTime nullableDt)
            {
                return ConvertDateTime(nullableDt);
            }
            return dateTimeValue;
        }

        private static DateTime ConvertDateTime(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                _ => dateTime
            };
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            ConfigureIdentityTables(builder);
            ConfigureTaskDependencies(builder);
        }

        private static void ConfigureIdentityTables(ModelBuilder builder)
        {
            builder.Entity<IdentityUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        }

        private static void ConfigureTaskDependencies(ModelBuilder builder)
        {
            builder.Entity<TaskDependency>()
                .HasKey(td => new { td.TaskId, td.DependsOnTaskId });

            builder.Entity<TaskDependency>()
                .HasOne(td => td.Task)
                .WithMany(t => t.Dependencies)
                .HasForeignKey(td => td.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskDependency>()
                .HasOne(td => td.DependsOnTask)
                .WithMany(t => t.DependentOnThis)
                .HasForeignKey(td => td.DependsOnTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
