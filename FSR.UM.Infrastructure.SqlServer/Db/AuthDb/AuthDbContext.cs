using FSR.UM.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FSR.UM.Infrastructure.SqlServer.Db.AuthDb
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options) { }

        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<OrgTier> OrgTiers => Set<OrgTier>();
        public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();
        public DbSet<RolePermissionAssignment> RolePermissionAssignments => Set<RolePermissionAssignment>();
        public DbSet<RegisteredPingUser> RegisteredPingUsers => Set<RegisteredPingUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure UserRoleAssignment (many-to-many: User <-> Role)
            modelBuilder.Entity<UserRoleAssignment>()
                .HasKey(ura => new { ura.UserId, ura.RoleId });

            modelBuilder.Entity<UserRoleAssignment>()
                .HasOne(ura => ura.User)
                .WithMany(u => u.UserRoleAssignments)
                .HasForeignKey(ura => ura.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRoleAssignment>()
                .HasOne(ura => ura.Role)
                .WithMany(r => r.UserRoleAssignments)
                .HasForeignKey(ura => ura.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure RolePermissionAssignment (many-to-many: Role <-> Permission)
            modelBuilder.Entity<RolePermissionAssignment>()
                .HasKey(rpa => new { rpa.RoleId, rpa.PermissionId });

            modelBuilder.Entity<RolePermissionAssignment>()
                .HasOne(rpa => rpa.Role)
                .WithMany(r => r.RolePermissionAssignments)
                .HasForeignKey(rpa => rpa.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermissionAssignment>()
                .HasOne(rpa => rpa.Permission)
                .WithMany(p => p.RolePermissionAssignments)
                .HasForeignKey(rpa => rpa.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Ignore(u => u.Roles); // This is a computed property, not stored in DB

            // Configure Role
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .Ignore(r => r.Permissions); // This is a computed property, not stored in DB

            // Configure Permission
            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.Name)
                .IsUnique();

            // Configure RegisteredPingUser
            modelBuilder.Entity<RegisteredPingUser>()
                .HasIndex(rpu => rpu.Email)
                .IsUnique();
        }
    }
}
