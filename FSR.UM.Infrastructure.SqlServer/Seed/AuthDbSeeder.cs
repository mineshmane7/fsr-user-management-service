using FSR.UM.Core.Models;
using FSR.UM.Infrastructure.SqlServer.Db.AuthDb;

namespace FSR.UM.Infrastructure.SqlServer.Seed
{
    public static class AuthDbSeeder
    {
        public static void Seed(AuthDbContext context)
        {
            // Seed Permissions first
            SeedPermissions(context);
            
            // Seed Roles
            SeedRoles(context);
            
            // Seed RegisteredPingUsers
            SeedRegisteredPingUsers(context);
            
            // Seed Admin User
            SeedAdminUser(context);
        }

        private static void SeedPermissions(AuthDbContext context)
        {
            if (context.Permissions.Any())
                return;

            var permissions = new List<Permission>
            {
                new Permission { Name = "Create", Description = "Create new records", IsActive = true },
                new Permission { Name = "View", Description = "View records", IsActive = true },
                new Permission { Name = "Edit", Description = "Edit existing records", IsActive = true },
                new Permission { Name = "Archive", Description = "Archive records", IsActive = true },
                new Permission { Name = "Delete", Description = "Delete records permanently", IsActive = true },
                new Permission { Name = "BulkEdit", Description = "Edit multiple records at once", IsActive = true },
                new Permission { Name = "BulkExport", Description = "Export multiple records", IsActive = true },
                new Permission { Name = "BulkImport", Description = "Import multiple records", IsActive = true }
            };

            context.Permissions.AddRange(permissions);
            context.SaveChanges();
        }

        private static void SeedRoles(AuthDbContext context)
        {
            if (context.Roles.Any())
                return;

            var roles = new List<Role>
            {
                new Role { Name = "Admin", Description = "Full system access", IsActive = true },
                new Role { Name = "Manager", Description = "Management level access", IsActive = true },
                new Role { Name = "User", Description = "Basic user access", IsActive = true }
            };

            context.Roles.AddRange(roles);
            context.SaveChanges();

            // Assign all permissions to Admin role
            var adminRole = context.Roles.First(r => r.Name == "Admin");
            var allPermissions = context.Permissions.ToList();

            foreach (var permission in allPermissions)
            {
                context.RolePermissionAssignments.Add(new RolePermissionAssignment
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }

            // Assign View and Edit permissions to Manager role
            var managerRole = context.Roles.First(r => r.Name == "Manager");
            var managerPermissions = context.Permissions
                .Where(p => p.Name == "View" || p.Name == "Edit" || p.Name == "Create")
                .ToList();

            foreach (var permission in managerPermissions)
            {
                context.RolePermissionAssignments.Add(new RolePermissionAssignment
                {
                    RoleId = managerRole.Id,
                    PermissionId = permission.Id
                });
            }

            // Assign View permission to User role
            var userRole = context.Roles.First(r => r.Name == "User");
            var viewPermission = context.Permissions.First(p => p.Name == "View");

            context.RolePermissionAssignments.Add(new RolePermissionAssignment
            {
                RoleId = userRole.Id,
                PermissionId = viewPermission.Id
            });

            context.SaveChanges();
        }

        private static void SeedRegisteredPingUsers(AuthDbContext context)
        {
            if (context.RegisteredPingUsers.Any())
                return;

            var registeredPingUsers = new List<RegisteredPingUser>
            {
                new RegisteredPingUser 
                { 
                    Email = "admin@fsr.com", 
                    FirstName = "System", 
                    LastName = "Administrator",
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = true,
                    Notes = "System administrator account"
                },
                new RegisteredPingUser 
                { 
                    Email = "john.doe@fsr.com", 
                    FirstName = "John", 
                    LastName = "Doe",
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = true,
                    Notes = "Test manager user"
                },
                new RegisteredPingUser 
                { 
                    Email = "jane.smith@fsr.com", 
                    FirstName = "Jane", 
                    LastName = "Smith",
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = true,
                    Notes = "Test regular user"
                },
                new RegisteredPingUser 
                { 
                    Email = "mike.wilson@fsr.com", 
                    FirstName = "Mike", 
                    LastName = "Wilson",
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = true,
                    Notes = "Test user account"
                },
                new RegisteredPingUser 
                { 
                    Email = "sarah.johnson@fsr.com", 
                    FirstName = "Sarah", 
                    LastName = "Johnson",
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = true,
                    Notes = "Test user account"
                }
            };

            context.RegisteredPingUsers.AddRange(registeredPingUsers);
            context.SaveChanges();
        }

        private static void SeedAdminUser(AuthDbContext context)
        {
            if (context.Users.Any())
                return;

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@fsr.com",
                UserName = "admin",
                FirstName = "System",
                LastName = "Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                PhoneNumber = "1234567890",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            context.SaveChanges();

            // Assign Admin role to admin user
            var adminRole = context.Roles.First(r => r.Name == "Admin");
            context.UserRoleAssignments.Add(new UserRoleAssignment
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedDate = DateTime.UtcNow
            });

            context.SaveChanges();
        }
    }
}
