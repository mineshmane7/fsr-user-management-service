# FSR.UM.Infrastructure.SqlServer.Migrations

This project contains Entity Framework Core migrations for the FSR User Management Service. The solution uses two separate database contexts for different concerns.

## ?? Overview

### Database Contexts

1. **AuthDbContext** - Handles authentication and user management
   - Users
   - Roles
   - Permissions
   - OrgTiers

2. **PropertyDbContext** - Handles property and unit management
   - Properties
   - Units

## ?? Auto-Migration

The application is configured to automatically apply pending migrations on startup. This is handled in `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var propertyDb = scope.ServiceProvider.GetRequiredService<PropertyDbContext>();
    propertyDb.Database.Migrate();
    PropertyDbSeeder.Seed(propertyDb);

    var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    authDb.Database.Migrate();
    AuthDbSeeder.Seed(authDb);
}
```

### What Auto-Migration Does:
- ? Automatically creates databases if they don't exist
- ? Applies all pending migrations on application startup
- ? Seeds initial data (admin user, default property)
- ? Ensures database schema is always up-to-date

### Benefits:
- No manual migration steps required for developers
- Consistent database state across environments
- Simplified deployment process

## ??? Manual Migration Commands

### Prerequisites
Install EF Core CLI tools globally:
```bash
dotnet tool install --global dotnet-ef
```

Or update if already installed:
```bash
dotnet tool update --global dotnet-ef
```

### Important: Command Location

**All migration commands must be run from the `FSR.UM.Infrastructure.SqlServer.Migrations` directory.**

```bash
# Navigate to migrations directory first
cd FSR.UM.Infrastructure.SqlServer.Migrations
```

### Creating New Migrations

#### For AuthDb Context:
```bash
dotnet ef migrations add MigrationName --context AuthDbContext --output-dir Migrations/AuthDb
```

**Example:**
```bash
dotnet ef migrations add AddUserProfileFields --context AuthDbContext --output-dir Migrations/AuthDb
```

#### For PropertyDb Context:
```bash
dotnet ef migrations add MigrationName --context PropertyDbContext --output-dir Migrations/PropertyDb
```

**Example:**
```bash
dotnet ef migrations add AddPropertyAddress --context PropertyDbContext --output-dir Migrations/PropertyDb
```

### Listing Migrations

#### List AuthDb Migrations:
```bash
dotnet ef migrations list --context AuthDbContext
```

#### List PropertyDb Migrations:
```bash
dotnet ef migrations list --context PropertyDbContext
```

### Removing Last Migration

#### Remove from AuthDb:
```bash
dotnet ef migrations remove --context AuthDbContext
```

#### Remove from PropertyDb:
```bash
dotnet ef migrations remove --context PropertyDbContext
```

?? **Warning:** Only remove migrations that haven't been applied to production databases!

### Applying Migrations Manually (Optional)

**Note:** Migrations are automatically applied when you run the application. These commands are only needed if you want to manually update the database without running the app.

#### Update AuthDb:
```bash
dotnet ef database update --context AuthDbContext
```

#### Update PropertyDb:
```bash
dotnet ef database update --context PropertyDbContext
```

### Rollback to Specific Migration

#### Rollback AuthDb:
```bash
dotnet ef database update MigrationName --context AuthDbContext
```

#### Rollback PropertyDb:
```bash
dotnet ef database update MigrationName --context PropertyDbContext
```

### Complete Workflow Example

```bash
# 1. Navigate to migrations directory
cd FSR.UM.Infrastructure.SqlServer.Migrations

# 2. Create a new migration for AuthDb
dotnet ef migrations add AddUserPhoneNumber --context AuthDbContext --output-dir Migrations/AuthDb

# 3. Review the generated migration file

# 4. Run the application (migrations will be applied automatically)
cd ../FSR.UM.Api
dotnet run
```

## ?? Project Structure

```
FSR.UM.Infrastructure.SqlServer.Migrations/
??? Migrations/
?   ??? AuthDb/
?   ?   ??? 20251218033803_InitialCreate.cs
?   ?   ??? 20251218033803_InitialCreate.Designer.cs
?   ?   ??? AuthDbContextModelSnapshot.cs
?   ??? PropertyDb/
?       ??? 20251218033819_InitialCreate.cs
?       ??? 20251218033819_InitialCreate.Designer.cs
?       ??? PropertyDbContextModelSnapshot.cs
??? DesignTimeDbContextFactory.cs          # Enables migrations without startup project
??? FSR.UM.Infrastructure.SqlServer.Migrations.csproj
??? README.md
```

## ?? Quick Start for New Developers

1. **Clone the repository**
   ```bash
   git clone https://github.com/mineshmane7/fsr-user-management-service
   cd FSR.UserManagement
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application** (Auto-migration will handle database setup)
   ```bash
   cd FSR.UM.Api
   dotnet run
   ```

4. **Verify databases were created**
   - Check SQL Server for `CyanAuth` and `CyanPropertyManagement` databases
   - Verify seed data is present

## ?? Troubleshooting

### Migration Build Errors

If you get build errors when creating migrations:
```bash
# Clean and rebuild
cd FSR.UM.Infrastructure.SqlServer.Migrations
dotnet clean
dotnet build
```

### Connection String Issues

The `DesignTimeDbContextFactory` uses hardcoded connection strings for design-time operations:
- **AuthDb:** `Server=localhost;Database=CyanAuth;Integrated Security=True;TrustServerCertificate=True;`
- **PropertyDb:** `Server=localhost;Database=CyanPropertyManagement;Integrated Security=True;TrustServerCertificate=True;`

If your SQL Server instance is different, update the connection strings in `DesignTimeDbContextFactory.cs`.

Runtime connection strings are configured in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "CyanAuth": "Server=(localdb)\\mssqllocaldb;Database=CyanAuth;Trusted_Connection=True;MultipleActiveResultSets=true",
    "CyanPropertyManagement": "Server=(localdb)\\mssqllocaldb;Database=CyanPropertyManagement;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### EF Core Tools Not Found

Install or update EF Core tools:
```bash
dotnet tool install --global dotnet-ef
# or
dotnet tool update --global dotnet-ef
```

Verify installation:
```bash
dotnet ef --version
```

### "Build failed" Error

If you get a build error with the solution:
- Navigate to the Migrations project directory: `cd FSR.UM.Infrastructure.SqlServer.Migrations`
- Run commands from there instead of the solution root
- This avoids potential build issues with other projects in the solution

### Auto-Migration Not Working

Check `Program.cs` for the migration code block:
- Ensure `Database.Migrate()` is called for both contexts
- Verify seeders are executed after migration
- Check application startup logs for errors

## ?? Best Practices

1. **Always run commands from the Migrations project directory** - This ensures proper context resolution

2. **Create migrations for both contexts separately** - They have different schemas and purposes

3. **Use descriptive migration names** - e.g., `AddUserPhoneNumber` instead of `Update1`

4. **Review generated migrations** - Always check the generated code before applying

5. **Test migrations locally first** - Don't apply untested migrations to production

6. **Keep migrations small and focused** - One logical change per migration

7. **Don't modify applied migrations** - Create a new migration to fix issues

8. **Backup databases before major migrations** - Especially in production

9. **Commit migration files to source control** - They are part of your application code

## ?? Key Architecture Decisions

### Why No API Project Reference?

The Migrations project intentionally **does not** reference the API project to:
- ? Keep dependencies clean and unidirectional
- ? Avoid circular dependencies
- ? Enable migrations to run independently of the API
- ? Simplify CI/CD pipeline for database updates
- ? Prevent build issues from other projects affecting migrations

### Design-Time Context Factory

The `DesignTimeDbContextFactory` enables EF Core tools to create DbContext instances at design-time without:
- Running the full application
- Needing configuration from `appsettings.json`
- Depending on the API project's startup logic
- Requiring a startup project reference

This approach is cleaner and follows EF Core best practices for migration management.

## ?? Related Documentation

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [EF Core Migrations Overview](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Design-Time DbContext Creation](https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation)
- [Main Project README](../README.md)

## ?? Support

For issues or questions:
- Check the troubleshooting section above
- Review EF Core documentation
- Contact the development team

---

**Last Updated:** December 2024  
**EF Core Version:** 8.0.11  
**.NET Version:** 8.0
