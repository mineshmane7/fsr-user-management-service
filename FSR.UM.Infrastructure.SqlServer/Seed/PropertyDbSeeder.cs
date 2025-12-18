using FSR.UM.Core.Models;
using FSR.UM.Infrastructure.SqlServer.Db;
using FSR.UM.Infrastructure.SqlServer.Db.PropertyDb;

namespace FSR.UM.Infrastructure.SqlServer.Seed
{
    public static class PropertyDbSeeder
    {
        public static void Seed(PropertyDbContext context)
        {
            if (!context.Properties.Any())
            {
                context.Properties.AddRange(
                    new Property
                    {
                        Name = "Default Property"
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
