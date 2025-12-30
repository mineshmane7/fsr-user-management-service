# RBAC (Role-Based Access Control) Implementation Guide

## ?? Overview

This document explains the complete RBAC implementation in the FSR User Management Service, including how to use permissions, roles, and JWT authentication.

## ?? Database Schema

### Tables

1. **Users** - User accounts
2. **Roles** - System roles (Admin, Manager, User)
3. **Permissions** - System permissions (Create, View, Edit, Delete, etc.)
4. **UserRoleAssignments** - Junction table linking Users to Roles
5. **RolePermissionAssignments** - Junction table linking Roles to Permissions

### Standard Permissions

The system includes 8 standard permissions:
- **Create** - Create new records
- **View** - View/read records
- **Edit** - Edit existing records
- **Archive** - Archive records
- **Delete** - Delete records permanently
- **BulkEdit** - Edit multiple records at once
- **BulkExport** - Export multiple records
- **BulkImport** - Import multiple records

### Default Roles

Three roles are seeded automatically:

| Role | Permissions | Description |
|------|-------------|-------------|
| **Admin** | All 8 permissions | Full system access |
| **Manager** | Create, View, Edit | Management level access |
| **User** | View only | Basic read-only access |

### Default Admin User

- **Email**: admin@fsr.com
- **Username**: admin
- **Password**: Admin@123
- **Role**: Admin
- **Permissions**: All

## ?? Authentication Flow

### 1. Login

**Endpoint**: `POST /api/auth/login`

**Request**:
```json
{
  "email": "admin@fsr.com",
  "password": "Admin@123"
}
```

**Response**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-12-18T12:00:00Z",
  "userId": "guid-here",
  "email": "admin@fsr.com",
  "userName": "admin",
  "firstName": "System",
  "lastName": "Administrator",
  "roles": ["Admin"],
  "permissions": ["Create", "View", "Edit", "Archive", "Delete", "BulkEdit", "BulkExport", "BulkImport"]
}
```

### 2. Using the JWT Token

Include the token in the Authorization header for all protected endpoints:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. JWT Token Contents

The JWT token includes the following claims:
- **NameIdentifier** - User ID
- **Email** - User email
- **Name** - Username
- **Role** - All user roles (can be multiple)
- **permission** - All user permissions (flattened from all roles)

## ??? Authorization Usage

### Method 1: Using `[HasPermission]` Attribute

```csharp
// Require specific permission
app.MapGet("/api/users", [HasPermission("View")] async (IUserRepository repo) =>
{
    return Results.Ok(await repo.GetAllAsync());
});

app.MapPost("/api/users", [HasPermission("Create")] async (User user, IUserRepository repo) =>
{
    return Results.Created($"/api/users/{user.Id}", await repo.AddAsync(user));
});

app.MapPut("/api/users/{id}", [HasPermission("Edit")] async (Guid id, User user, IUserRepository repo) =>
{
    user.Id = id;
    return Results.Ok(await repo.UpdateAsync(user));
});

app.MapDelete("/api/users/{id}", [HasPermission("Delete")] async (Guid id, IUserRepository repo) =>
{
    await repo.DeleteAsync(id);
    return Results.NoContent();
});
```

### Method 2: Using `[Authorize]` with Roles

```csharp
// Require specific role
app.MapGet("/api/admin", [Authorize(Roles = "Admin")] () =>
{
    return Results.Ok("Admin only endpoint");
});

// Require one of multiple roles
app.MapGet("/api/management", [Authorize(Roles = "Admin,Manager")] () =>
{
    return Results.Ok("Admin or Manager endpoint");
});
```

### Method 3: Using Policy-Based Authorization

```csharp
// In Program.cs - define policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
});

// In endpoints
app.MapGet("/api/reports", [Authorize(Policy = "ManagerOrAdmin")] () =>
{
    return Results.Ok("Manager or Admin can access");
});
```

### Method 4: Manual Authorization Check

```csharp
app.MapGet("/api/data", [Authorize] (ClaimsPrincipal user) =>
{
    // Check if user has specific permission
    var hasViewPermission = user.Claims
        .Any(c => c.Type == "permission" && c.Value == "View");
    
    if (!hasViewPermission)
        return Results.Forbid();
    
    // Check if user has specific role
    var isAdmin = user.IsInRole("Admin");
    
    return Results.Ok("Data");
});
```

## ?? API Endpoints

### Authentication Endpoints

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| POST | /api/auth/login | None | User login |

### User Management Endpoints

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| GET | /api/users | View | Get all users |
| GET | /api/users/{id} | View | Get user by ID with roles |
| GET | /api/users/me | Authenticated | Get current user |
| POST | /api/users | Create | Create new user |
| PUT | /api/users/{id} | Edit | Update user |
| DELETE | /api/users/{id} | Delete | Delete user |

### Property Management Endpoints

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| GET | /api/properties | View | Get all properties |
| POST | /api/properties | Create | Create new property |

## ?? Testing with Swagger

1. **Start the application**
   ```bash
   cd FSR.UM.Api
   dotnet run
   ```

2. **Navigate to Swagger UI**
   - Open browser: `https://localhost:<port>/`
   - Swagger UI will load automatically

3. **Login to get token**
   - Expand `POST /api/auth/login`
   - Click "Try it out"
   - Use credentials:
     ```json
     {
       "email": "admin@fsr.com",
       "password": "Admin@123"
     }
     ```
   - Execute and copy the `accessToken` from response

4. **Authorize in Swagger**
   - Click the "Authorize" button at the top
   - Enter: `Bearer <your-token-here>`
   - Click "Authorize" then "Close"

5. **Test protected endpoints**
   - All endpoints will now include the Authorization header
   - Try endpoints that require different permissions

## ?? Configuration

### appsettings.json

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm",
    "Issuer": "FSR.UserManagement.Api",
    "Audience": "FSR.UserManagement.Client",
    "ExpirationInMinutes": 60
  }
}
```

**Important**: Change the JWT Key in production to a strong, randomly generated secret.

## ?? Managing Users and Roles

### Assigning Roles to Users

```csharp
// In your service or repository
public async Task AssignRoleToUser(Guid userId, int roleId)
{
    var assignment = new UserRoleAssignment
    {
        UserId = userId,
        RoleId = roleId,
        AssignedDate = DateTime.UtcNow,
        AssignedBy = currentUserId // Optional
    };
    
    _context.UserRoleAssignments.Add(assignment);
    await _context.SaveChangesAsync();
}
```

### Creating Custom Roles

```csharp
// Create a new role
var customRole = new Role
{
    Name = "PropertyManager",
    Description = "Manages properties",
    IsActive = true
};

_context.Roles.Add(customRole);
await _context.SaveChangesAsync();

// Assign permissions to the role
var viewPermission = await _context.Permissions.FirstAsync(p => p.Name == "View");
var editPermission = await _context.Permissions.FirstAsync(p => p.Name == "Edit");

_context.RolePermissionAssignments.AddRange(new[]
{
    new RolePermissionAssignment { RoleId = customRole.Id, PermissionId = viewPermission.Id },
    new RolePermissionAssignment { RoleId = customRole.Id, PermissionId = editPermission.Id }
});

await _context.SaveChangesAsync();
```

## ?? Advanced Usage

### Custom Permission Requirements

```csharp
// Create custom requirement
public class MinimumRoleRequirement : IAuthorizationRequirement
{
    public string MinimumRole { get; }
    
    public MinimumRoleRequirement(string minimumRole)
    {
        MinimumRole = minimumRole;
    }
}

// Create handler
public class MinimumRoleHandler : AuthorizationHandler<MinimumRoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumRoleRequirement requirement)
    {
        var roleHierarchy = new Dictionary<string, int>
        {
            { "User", 1 },
            { "Manager", 2 },
            { "Admin", 3 }
        };
        
        var userRoles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);
        
        var userMaxLevel = userRoles
            .Max(r => roleHierarchy.GetValueOrDefault(r, 0));
        
        var requiredLevel = roleHierarchy.GetValueOrDefault(requirement.MinimumRole, 999);
        
        if (userMaxLevel >= requiredLevel)
            context.Succeed(requirement);
        
        return Task.CompletedTask;
    }
}

// Register in Program.cs
builder.Services.AddSingleton<IAuthorizationHandler, MinimumRoleHandler>();
```

## ?? Troubleshooting

### Token Not Working

1. Check if token is expired
2. Verify JWT configuration matches between generation and validation
3. Ensure token is sent with `Bearer` prefix
4. Check if user is active

### Permission Denied

1. Verify user has the required permission in the JWT token
2. Check role-permission assignments in database
3. Ensure `PermissionAuthorizationHandler` is registered
4. Verify `PermissionPolicyProvider` is registered

### Migration Issues

```bash
# If you need to recreate the database
cd FSR.UM.Infrastructure.SqlServer.Migrations
dotnet ef database drop --context AuthDbContext
dotnet ef database update --context AuthDbContext
```

## ?? Best Practices

1. **Always use HTTPS** in production
2. **Store JWT keys securely** (use Azure Key Vault, AWS Secrets Manager, etc.)
3. **Set appropriate token expiration** times
4. **Implement token refresh** mechanism for better UX
5. **Log authorization failures** for security auditing
6. **Use principle of least privilege** - give minimum required permissions
7. **Regularly review** user roles and permissions
8. **Implement password policies** for user accounts
9. **Add rate limiting** to prevent brute force attacks
10. **Consider implementing** password reset functionality

## ?? Related Documentation

- [Main Project README](../README.md)
- [Migration Guide](../FSR.UM.Infrastructure.SqlServer.Migrations/README.md)
- [ASP.NET Core Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/)
- [JWT Bearer Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)

---

**Last Updated:** December 2024  
**Version:** 1.0
