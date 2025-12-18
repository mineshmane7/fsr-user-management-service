using FSR.UM.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FSR.UM.Infrastructure.SqlServer.Db.PropertyDb
{
    public class PropertyDbContext : DbContext
    {
        public PropertyDbContext(DbContextOptions<PropertyDbContext> options)
            : base(options) { }

        public DbSet<Property> Properties { get; set; }

        public DbSet<Unit> Units => Set<Unit>();
    }
}
