using FSR.UM.Core.Models;
using FSR.UM.Infrastructure.SqlServer.Db;
using FSR.UM.Infrastructure.SqlServer.Db.AuthDb;

namespace FSR.UM.Infrastructure.SqlServer.Seed
{
    public static class AuthDbSeeder
    {
        public static void Seed(AuthDbContext context)
        {
            // ✅ Auth DB check
            if (!context.Users.Any())
            {
                context.Users.Add(new User
                {
                    Email = "admin@fsr.com"
                });

                context.SaveChanges();
            }
        }
    }
}
