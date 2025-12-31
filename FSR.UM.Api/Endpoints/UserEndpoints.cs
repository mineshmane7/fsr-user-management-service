using CsvHelper;
using FSR.UM.Core.Authorization;
using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Interfaces.Auth;
using FSR.UM.Core.Models;
using FSR.UM.Core.Models.Auth;
using FSR.UM.Core.Models.DTOs;
using FSR.UM.Infrastructure.Parsing;
using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;
using System.Globalization;

namespace Dummy.Iam.Api.Endpoints;

public static class ApiEndpoints
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        // ==================== Authentication ====================
        var authGroup = app.MapGroup("/api/auth")
            .WithTags("🔐 Authentication");

        authGroup.MapPost("/login", async (
            LoginRequest request,
            IAuthService authService) =>
        {
            try
            {
                var response = await authService.LoginAsync(request);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Return 401 with specific error message
                return Results.Json(
                    new {
                        error = "Authentication Failed",
                        message = ex.Message,
                        timestamp = DateTime.UtcNow
                    },
                    statusCode: 401
                );
            }
            catch (Exception ex)
            {
                // Log unexpected errors and return generic message
                Console.WriteLine($"Login error: {ex.Message}");
                return Results.Json(
                    new {
                        error = "Login Error",
                        message = "An unexpected error occurred during login. Please try again later.",
                        timestamp = DateTime.UtcNow
                    },
                    statusCode: 500
                );
            }
        })
        .WithName("Login")
        .WithSummary("User Login")
        .WithDescription(@"Authenticate and get JWT token with roles and permissions.

**⚠️ Ping Identity Requirement:**
- Only users with emails registered in Ping Identity can login
- Email must exist in the RegisteredPingUsers table
- If email is not registered with Ping, login will be denied

**Error Scenarios:**
- **Email not in Ping**: 'Access denied. Your email is not registered with Ping Identity...'
- **Ping registration inactive**: 'Your Ping Identity registration has been deactivated...'
- **User account not found**: 'No account found with this email...'
- **User account inactive**: 'Your user account has been deactivated...'
- **Invalid password**: 'Invalid password. Please check your credentials...'

**Test Account:**
- Email: admin@fsr.com
- Password: Admin@123

**Response includes:**
- JWT access token
- User information
- Assigned roles and permissions")
        .AllowAnonymous();

        // ==================== User Management (Admin Only) ====================
        var adminGroup = app.MapGroup("/api/admin")
            .WithTags("👤 User Management (Admin)")
            .RequireAuthorization("AdminOnly");

        adminGroup.MapPost("/users", [HasPermission("Create")] async (
            CreateUserRequest request,
            IUserManagementService userService) =>
        {
            try
            {
                var user = await userService.CreateUserWithRoleAsync(request);
                return Results.Created($"/api/admin/users/{user.Id}", new
                {
                    message = "User created successfully",
                    user = user
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("CreateUser")
        .WithSummary("Create New User")
        .WithDescription(@"Create a new user with a role. Available roles: Admin, Manager, User

**⚠️ Important: Ping Registration Required**
- Only emails registered in Ping Identity can be used to create users
- The email must exist in the RegisteredPingUsers table
- If the email is not registered with Ping, user creation will fail

**Role Permissions:**
- Admin: All permissions (Create, View, Edit, Delete, Archive, BulkEdit, BulkExport, BulkImport)
- Manager: Create, View, Edit
- User: View only

**Test Registered Ping Emails:**
- john.doe@fsr.com
- jane.smith@fsr.com
- mike.wilson@fsr.com
- sarah.johnson@fsr.com");

                adminGroup.MapPost("/users/bulk-upload",
            [HasPermission("BulkImport")] async (
                IFormFile file,
                IUserManagementService userService) =>
            {
                if (file == null || file.Length == 0)
                    return Results.BadRequest("CSV file is required");

              //  var users = CsvParser.ParseCsv(file.OpenReadStream());
                var users = UserCsvParser.Parse(file.OpenReadStream());


                var result = await userService.BulkCreateUsersAsync(users);

                return Results.Ok(new
                {
                    message = "Bulk upload completed",
                    result.TotalRecords,
                    result.CreatedCount,
                    result.FailedCount,
                    result.Failures
                });
            }).DisableAntiforgery();

        //public static List<CreateUserRequest> ParseCsv(Stream stream)
        //{
        //    using var reader = new StreamReader(stream);
        //    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        //    return csv.GetRecords<CreateUserRequest>().ToList();
        //}
        //whre should i add this parsing code?

        // ==================== Property Management (RBAC) ====================
        var propertyGroup = app.MapGroup("/api/properties")
            .WithTags("🏢 Property Management");

        propertyGroup.MapGet("/", [HasPermission("View")] async (
            IPropertyRepository repo) =>
        {
            var properties = await repo.GetAllAsync();
            return Results.Ok(properties);
        })
        .WithName("GetAllProperties")
        .WithSummary("Get All Properties")
        .WithDescription("Requires 'View' permission. Returns list of all properties.")
        .RequireAuthorization();

        propertyGroup.MapGet("/{id:int}", [HasPermission("View")] async (
            int id,
            IPropertyRepository repo) =>
        {
            var property = await repo.GetByIdAsync(id);
            return property != null ? Results.Ok(property) : Results.NotFound(new { error = "Property not found" });
        })
        .WithName("GetPropertyById")
        .WithSummary("Get Property by ID")
        .WithDescription("Requires 'View' permission")
        .RequireAuthorization();

        propertyGroup.MapPost("/", [HasPermission("Create")] async (
            CreatePropertyRequest request,
            IPropertyRepository repo) =>
        {
            try
            {
                var property = new Property
                {
                    Name = request.Name,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    ZipCode = request.ZipCode
                };

                var created = await repo.AddAsync(property);
                return Results.Created($"/api/properties/{created.Id}", new
                {
                    message = "Property created successfully",
                    property = created
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateProperty")
        .WithSummary("Create New Property")
        .WithDescription("Requires 'Create' permission")
        .RequireAuthorization();

        propertyGroup.MapPut("/{id:int}", [HasPermission("Edit")] async (
            int id,
            UpdatePropertyRequest request,
            IPropertyRepository repo) =>
        {
            try
            {
                var existing = await repo.GetByIdAsync(id);
                if (existing == null)
                    return Results.NotFound(new { error = "Property not found" });

                existing.Name = request.Name;
                existing.Address = request.Address;
                existing.City = request.City;
                existing.State = request.State;
                existing.ZipCode = request.ZipCode;

                var updated = await repo.UpdateAsync(existing);
                return Results.Ok(new
                {
                    message = "Property updated successfully",
                    property = updated
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateProperty")
        .WithSummary("Update Property")
        .WithDescription("Requires 'Edit' permission")
        .RequireAuthorization();

        propertyGroup.MapDelete("/{id:int}", [HasPermission("Delete")] async (
            int id,
            IPropertyRepository repo) =>
        {
            try
            {
                var existing = await repo.GetByIdAsync(id);
                if (existing == null)
                    return Results.NotFound(new { error = "Property not found" });

                await repo.DeleteAsync(id);
                return Results.Ok(new { message = "Property deleted successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("DeleteProperty")
        .WithSummary("Delete Property")
        .WithDescription("Requires 'Delete' permission")
        .RequireAuthorization();
    }
}
