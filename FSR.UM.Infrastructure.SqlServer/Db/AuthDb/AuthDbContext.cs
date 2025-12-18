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
    }
}
