using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using TaskForge.Domain.Entities;

namespace TaskForge.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for each entity
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
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                foreach (var property in entry.OriginalValues.Properties)
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        var currentValue = entry.Property(property.Name).CurrentValue;

                        if (currentValue is DateTime dateTimeValue)
                        {
                            if (dateTimeValue.Kind == DateTimeKind.Unspecified)
                                entry.Property(property.Name).CurrentValue = DateTime.SpecifyKind(dateTimeValue, DateTimeKind.Utc);
                            else if (dateTimeValue.Kind == DateTimeKind.Local)
                                entry.Property(property.Name).CurrentValue = dateTimeValue.ToUniversalTime();
                        }
                        else if (currentValue is DateTime?)
                        {
                            var nullableDateTimeValue = (DateTime?)currentValue;
                            var dateTime = nullableDateTimeValue.Value;
                            if (dateTime.Kind == DateTimeKind.Unspecified)
                                entry.Property(property.Name).CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                            else if (dateTime.Kind == DateTimeKind.Local)
                                entry.Property(property.Name).CurrentValue = dateTime.ToUniversalTime();
                        }
                    }
                }
            }
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Define relationships and configurations here
            builder.Entity<IdentityUser>(entity =>
            {
                entity.ToTable("Users");
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("Roles");
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });


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
