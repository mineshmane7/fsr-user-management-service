using FSR.UM.Infrastructure.SqlServer.Db.AuthDb;
using FSR.UM.Infrastructure.SqlServer.Db.PropertyDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FSR.UM.Infrastructure.SqlServer.Migrations
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=CyanAuth;Integrated Security=True;TrustServerCertificate=True;",
                b => b.MigrationsAssembly("FSR.UM.Infrastructure.SqlServer.Migrations"));
            
            return new AuthDbContext(optionsBuilder.Options);
        }
    }

    public class PropertyDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PropertyDbContext>
    {
        public PropertyDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PropertyDbContext>();
            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=CyanPropertyManagement;Integrated Security=True;TrustServerCertificate=True;",
                b => b.MigrationsAssembly("FSR.UM.Infrastructure.SqlServer.Migrations"));
            
            return new PropertyDbContext(optionsBuilder.Options);
        }
    }
}
