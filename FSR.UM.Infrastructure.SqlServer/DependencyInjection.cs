using FSR.UM.Core.Interfaces;
using FSR.UM.Infrastructure.SqlServer.Db.AuthDb;
using FSR.UM.Infrastructure.SqlServer.Db.PropertyDb;
using FSR.UM.Infrastructure.SqlServer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FSR.UM.Infrastructure.SqlServer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSqlServerInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CyanAuth"),
                    sql => sql.MigrationsAssembly(
                        "FSR.UM.Infrastructure.SqlServer.Migrations"
                    )
                )
            );

            services.AddDbContext<PropertyDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CyanPropertyManagement"),
                    sql => sql.MigrationsAssembly(
                        "FSR.UM.Infrastructure.SqlServer.Migrations"
                    )
                )
            );

            // Register repositories
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
