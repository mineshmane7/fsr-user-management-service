using Dummy.Iam.Api.Endpoints;
using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Interfaces.Auth;
using FSR.UM.Infrastructure.Authorization;
using FSR.UM.Infrastructure.Services;
using FSR.UM.Infrastructure.SqlServer;
using FSR.UM.Infrastructure.SqlServer.Db.AuthDb;
using FSR.UM.Infrastructure.SqlServer.Db.PropertyDb;
using FSR.UM.Infrastructure.SqlServer.Seed;
using FSR.UM.Infrastructure.SqlServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Configure JSON serialization to handle reference loops
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "FSR Property Management API - RBAC Demo", 
        Version = "v1",
        Description = @"
## 🎯 API Overview
Simple Property Management API demonstrating Role-Based Access Control (RBAC) with Ping Identity integration.

## 🔐 Available Roles & Permissions
- **Admin**: All permissions (Create, View, Edit, Delete, Archive, BulkEdit, BulkExport, BulkImport)
- **Manager**: Create, View, Edit
- **User**: View only

## ⚠️ Ping Identity Registration Requirement
**User Creation Security:**
- Only users with emails registered in Ping Identity can be created
- Email must exist in the `RegisteredPingUsers` database table
- This check happens automatically when creating users

**Test Ping Registered Emails (Pre-seeded):**
- john.doe@fsr.com
- jane.smith@fsr.com
- mike.wilson@fsr.com
- sarah.johnson@fsr.com

**Note:** To add more emails, update the `AuthDbSeeder.cs` file in the codebase.

## 🚀 Quick Start
1. Login with: `admin@fsr.com` / `Admin@123`
2. Copy the access token
3. Click 'Authorize' and paste: `Bearer <token>`
4. Test creating users - use only Ping-registered emails!
5. Test the Property APIs with different roles!
        "
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Enable XML comments for better Swagger docs
    c.EnableAnnotations();
});

// Register infrastructure services
builder.Services.AddSqlServerInfrastructure(builder.Configuration);

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
        };
    });

// Configure Authorization with Permission-based policies
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    // Add role-based policies
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
});

var app = builder.Build();

// Auto-apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    try
    {
        Console.WriteLine("🔄 Starting database migration and seeding...");
        
        // Migrate and seed PropertyDb
        var propertyDb = scope.ServiceProvider.GetRequiredService<PropertyDbContext>();
        propertyDb.Database.Migrate();
        PropertyDbSeeder.Seed(propertyDb);

        // Migrate and seed AuthDb
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        authDb.Database.Migrate();
        AuthDbSeeder.Seed(authDb);
        
        Console.WriteLine("✅ Database migrations and seeding completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error during migration or seeding: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        throw;
    }
}

// Configure middleware pipeline
app.UseCors("AllowAll");

// Enable Swagger in all environments (for development/testing)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FSR Property Management API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "FSR Property Management API - RBAC Demo";
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map all API endpoints
app.MapApiEndpoints();

// Add a redirect from root to swagger for convenience
app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("🚀 FSR Property Management API Started Successfully!");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("📖 Swagger UI: /swagger");
Console.WriteLine();
Console.WriteLine("🔐 Test Accounts:");
Console.WriteLine("   Admin   → admin@fsr.com / Admin@123 (All permissions)");
Console.WriteLine();
Console.WriteLine("⚠️  Ping Registration Requirement:");
Console.WriteLine("   Only emails registered with Ping can create users!");
Console.WriteLine("   Test Ping Emails: john.doe@fsr.com, jane.smith@fsr.com");
Console.WriteLine();
Console.WriteLine("📝 Available Endpoints:");
Console.WriteLine("   🔐 POST /api/auth/login - User login");
Console.WriteLine("   👤 POST /api/admin/users - Create user (Admin only, Ping required)");
Console.WriteLine("   🏢 GET    /api/properties - View properties (View permission)");
Console.WriteLine("   🏢 GET    /api/properties/{id} - View property (View permission)");
Console.WriteLine("   🏢 POST   /api/properties - Create property (Create permission)");
Console.WriteLine("   🏢 PUT    /api/properties/{id} - Update property (Edit permission)");
Console.WriteLine("   🏢 DELETE /api/properties/{id} - Delete property (Delete permission)");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

app.Run();
