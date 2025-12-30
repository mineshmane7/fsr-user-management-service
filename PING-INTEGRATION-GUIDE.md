# Ping Identity Integration - User Registration Authorization

## ?? Overview

This document describes the Ping Identity integration feature that adds an additional layer of security to **both login and user creation** in the FSR User Management API.

## ?? Feature Description

**Requirement:** 
1. Only users with emails registered in Ping Identity can **login** to the system
2. Only Admin users can create new users, and only if the user's email is pre-registered with Ping Identity

### Security Flow

#### Login Flow
1. ? **Ping Registration Check** - The email must exist in the `RegisteredPingUsers` table with `IsActive = true`
2. ? **User Authentication** - Verify user exists and password is correct
3. ? **Token Generation** - Generate JWT token with roles and permissions

#### User Creation Flow
1. ? **Admin Authentication** - Only users with Admin role can access the user creation endpoint
2. ? **Ping Registration Check** - The email must exist in the `RegisteredPingUsers` table with `IsActive = true`
3. ? **User Creation** - If both checks pass, the user is created with the specified role

## ??? Database Schema

### RegisteredPingUsers Table

```sql
CREATE TABLE RegisteredPingUsers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(450) NOT NULL UNIQUE,
    FirstName NVARCHAR(MAX) NOT NULL,
    LastName NVARCHAR(MAX) NOT NULL,
    RegisteredDate DATETIME2 NOT NULL,
    IsActive BIT NOT NULL,
    Notes NVARCHAR(MAX) NULL
)

-- Unique index on Email
CREATE UNIQUE INDEX IX_RegisteredPingUsers_Email ON RegisteredPingUsers (Email)
```

**Note:** This table is managed internally and seeded with test data. There are no API endpoints to manage this table - it serves only as an authorization whitelist for user creation.

## ?? Test Data

The following emails are seeded in the database for testing:

| Email | First Name | Last Name | Status | Notes |
|-------|-----------|----------|--------|-------|
| admin@fsr.com | System | Administrator | Active | System administrator account |
| john.doe@fsr.com | John | Doe | Active | Test manager user |
| jane.smith@fsr.com | Jane | Smith | Active | Test regular user |
| mike.wilson@fsr.com | Mike | Wilson | Active | Test user account |
| sarah.johnson@fsr.com | Sarah | Johnson | Active | Test user account |

**To add more emails:** Update the `AuthDbSeeder.cs` file and add entries to the `SeedRegisteredPingUsers()` method.

## ?? API Endpoints

### User Login (With Ping Check)

**Endpoint:** `POST /api/auth/login`

**Authorization:** Anonymous (but requires Ping registration)

**Request Body:**
```json
{
  "email": "admin@fsr.com",
  "password": "Admin@123"
}
```

**Success Response (200):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-12-22T11:30:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "admin@fsr.com",
  "userName": "admin",
  "firstName": "System",
  "lastName": "Administrator",
  "roles": ["Admin"],
  "permissions": ["Create", "View", "Edit", "Delete", "Archive", "BulkEdit", "BulkExport", "BulkImport"]
}
```

**Error Responses:**

**401 - Email Not Registered with Ping:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```
Console shows: "Access denied. Your email is not registered with Ping Identity. Please contact your administrator."

**401 - Invalid Credentials:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

**401 - User Account Inactive:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

---

### User Creation (With Ping Check)

**Endpoint:** `POST /api/admin/users`

**Authorization:** Admin role required + Create permission

**Request Body:**
```json
{
  "email": "john.doe@fsr.com",
  "userName": "johndoe",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "1234567890",
  "password": "SecurePass@123",
  "roleName": "Manager"
}
```

**Success Response (201):**
```json
{
  "message": "User created successfully",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "john.doe@fsr.com",
    "userName": "johndoe",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "1234567890",
    "isActive": true,
    "createdDate": "2024-12-22T10:30:00Z",
    "roles": [...],
    "permissions": [...]
  }
}
```

**Error Responses:**

**400 - Email Not Registered with Ping:**
```json
{
  "error": "Email 'unknown@email.com' is not registered with Ping Identity. Only users registered with Ping can be created in the system."
}
```

**400 - User Already Exists:**
```json
{
  "error": "User with email 'john.doe@fsr.com' already exists"
}
```

**401 - Unauthorized (Not logged in)**

**403 - Forbidden (Not Admin role)**

## ?? Testing Scenarios

### Scenario 1: Create User with Registered Email (Success)
```bash
# 1. Login as Admin
POST /api/auth/login
{
  "email": "admin@fsr.com",
  "password": "Admin@123"
}

# 2. Create user with registered Ping email
POST /api/admin/users
Authorization: Bearer {token}
{
  "email": "john.doe@fsr.com",
  "userName": "johndoe",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "1234567890",
  "password": "SecurePass@123",
  "roleName": "Manager"
}

# Expected: 201 Created - User created successfully
```

### Scenario 2: Create User with Unregistered Email (Failure)
```bash
# 1. Login as Admin
POST /api/auth/login
{
  "email": "admin@fsr.com",
  "password": "Admin@123"
}

# 2. Try to create user with unregistered email
POST /api/admin/users
Authorization: Bearer {token}
{
  "email": "notregistered@example.com",
  "userName": "newuser",
  "firstName": "New",
  "lastName": "User",
  "phoneNumber": "1234567890",
  "password": "SecurePass@123",
  "roleName": "User"
}

# Expected: 400 Bad Request
# Error: "Email 'notregistered@example.com' is not registered with Ping Identity..."
```

### Scenario 3: Non-Admin Tries to Create User (Failure)
```bash
# 1. Login as non-admin user (assuming one exists)
POST /api/auth/login
{
  "email": "regularuser@fsr.com",
  "password": "UserPass@123"
}

# 2. Try to create user
POST /api/admin/users
Authorization: Bearer {token}
{
  "email": "john.doe@fsr.com",
  "userName": "test",
  ...
}

# Expected: 403 Forbidden - Only Admin role can create users
```

## ?? Testing Scenarios

### Scenario 1: Login with Registered Email (Success)
```bash
# Login with Ping-registered email
POST /api/auth/login
{
  "email": "admin@fsr.com",
  "password": "Admin@123"
}

# Expected: 200 OK with JWT token
```

### Scenario 2: Login with Unregistered Email (Failure)
```bash
# Try to login with email NOT in RegisteredPingUsers table
POST /api/auth/login
{
  "email": "notregistered@example.com",
  "password": "SomePassword123"
}

# Expected: 401 Unauthorized
# Error message: "Access denied. Your email is not registered with Ping Identity..."
```

### Scenario 3: Create User with Registered Email (Success)
```bash
# 1. Login as Admin
POST /api/auth/login
{
  "email": "admin@fsr.com",
  "password": "Admin@123"
}

# 2. Create user with registered Ping email
POST /api/admin/users
Authorization: Bearer {token}
{
  "email": "john.doe@fsr.com",
  "userName": "johndoe",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "1234567890",
  "password": "SecurePass@123",
  "roleName": "Manager"
}

# Expected: 201 Created - User created successfully
```

## ?? Code Implementation

### AuthService.cs (Login with Ping Check)
```csharp
public async Task<LoginResponse> LoginAsync(LoginRequest request)
{
    // STEP 1: Check if the email is registered with Ping
    var registeredPingUser = await _authDb.RegisteredPingUsers
        .FirstOrDefaultAsync(rpu => rpu.Email == request.Email && rpu.IsActive);

    if (registeredPingUser == null)
        throw new UnauthorizedAccessException(
            "Access denied. Your email is not registered with Ping Identity. " +
            "Please contact your administrator.");

    // STEP 2: Support login with email or username
    var user = await _userRepo.GetByEmailOrUserNameAsync(request.Email);

    if (user == null)
        throw new UnauthorizedAccessException("Invalid credentials");

    // STEP 3: Check if user is active
    if (!user.IsActive)
        throw new UnauthorizedAccessException("User account is inactive");

    // STEP 4: Verify password
    if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        throw new UnauthorizedAccessException("Invalid credentials");

    // STEP 5: Generate JWT token and return response...
}
```

### UserManagementService.cs (User Creation with Ping Check)
```csharp
public async Task<UserWithRolesDto> CreateUserWithRoleAsync(CreateUserRequest request)
{
    // STEP 1: Check if the email is registered with Ping
    var registeredPingUser = await _authDb.RegisteredPingUsers
        .FirstOrDefaultAsync(rpu => rpu.Email == request.Email && rpu.IsActive);

    if (registeredPingUser == null)
        throw new InvalidOperationException(
            $"Email '{request.Email}' is not registered with Ping Identity. " +
            "Only users registered with Ping can be created in the system.");

    // STEP 2-6: Continue with user creation...
}
```

### Adding New Ping Registered Emails

To add new emails to the Ping whitelist, update `AuthDbSeeder.cs`:

```csharp
private static void SeedRegisteredPingUsers(AuthDbContext context)
{
    if (context.RegisteredPingUsers.Any())
        return;

    var registeredPingUsers = new List<RegisteredPingUser>
    {
        // Add new entries here
        new RegisteredPingUser 
        { 
            Email = "newuser@fsr.com", 
            FirstName = "New", 
            LastName = "User",
            RegisteredDate = DateTime.UtcNow,
            IsActive = true,
            Notes = "Description here"
        }
    };

    context.RegisteredPingUsers.AddRange(registeredPingUsers);
    context.SaveChanges();
}
```

## ?? Migration History

### Migration: `AddRegisteredPingUsersTable`
**File:** `20251230051132_AddRegisteredPingUsersTable.cs`

**Changes:**
- Creates `RegisteredPingUsers` table
- Adds unique index on `Email` column
- Automatically applied on application startup

## ?? Business Logic

### Why This Approach?

1. **Security**: Ensures only authorized emails can be used to create accounts
2. **Control**: Admins have full control over who can have accounts via database seeding
3. **Simplicity**: No API endpoints means no additional attack surface
4. **Audit Trail**: Track when emails were registered with Ping
5. **Flexibility**: Can activate/deactivate Ping registrations without deleting users
6. **Compliance**: Meets requirement for Ping Identity integration

### Design Decision: No CRUD APIs

**Why no API endpoints for RegisteredPingUsers?**
- **Simplified Security**: Fewer endpoints = smaller attack surface
- **Controlled Management**: Changes require code deployment, ensuring proper review
- **Seed Data Pattern**: Follows .NET best practices for reference data
- **Reduced Complexity**: No need for additional authorization checks or validation logic

### Future Enhancements

1. **Ping API Integration**: Connect to actual Ping Identity API to sync users automatically
2. **Admin UI**: Create a separate admin tool for managing Ping registered users
3. **Bulk Import**: Script-based CSV import for batch adding emails
4. **Email Notifications**: Notify users when they're registered in Ping
5. **Audit Logs**: Track which admin added which emails and when

## ?? Related Documentation

- [RBAC-GUIDE.md](./RBAC-GUIDE.md) - Main RBAC documentation
- [README.md](./README.md) - Project overview
- [Migrations README](./FSR.UM.Infrastructure.SqlServer.Migrations/README.md) - Database migration guide

## ?? Troubleshooting

### Issue: "Access denied. Your email is not registered with Ping Identity" error when logging in

**Solution:** 
1. Check the database `RegisteredPingUsers` table to see if your email exists
2. Verify the email has `IsActive = true`
3. If not registered:
   - Add it to `AuthDbSeeder.cs` in the `SeedRegisteredPingUsers()` method
   - Either delete the database and restart the app (dev/test) or manually insert into DB
4. Contact your administrator to get your email registered

### Issue: "Email not registered with Ping" error when creating user

**Solution:** 
1. Check the database `RegisteredPingUsers` table to see if email exists
2. If not, add it to `AuthDbSeeder.cs` in the `SeedRegisteredPingUsers()` method
3. Either:
   - Delete the database and restart the app (for dev/test)
   - Manually insert into the database
   - Create and run a new migration with the additional emails

### Issue: Admin user can't login after implementing Ping check

**Solution:**
- The admin email (admin@fsr.com) is already in the seeded RegisteredPingUsers data
- Check if database migration ran successfully
- Verify RegisteredPingUsers table exists and has data
- Check if admin@fsr.com exists with `IsActive = true` in RegisteredPingUsers table

### Issue: Need to add many emails at once

**Solution:**
1. Update `AuthDbSeeder.cs` with all new emails
2. Create a migration script to insert them:
```bash
cd FSR.UM.Infrastructure.SqlServer.Migrations
dotnet ef migrations add AddNewPingUsers --context AuthDbContext --output-dir Migrations/AuthDb
```
3. Update the seeder to check for specific emails instead of `Any()`:
```csharp
if (!context.RegisteredPingUsers.Any(rpu => rpu.Email == "specific@email.com"))
{
    // Add specific user
}
```

### Issue: Can't access user creation endpoint

**Solution:**
- Ensure you're logged in as Admin
- Check your JWT token includes Admin role
- Verify token hasn't expired

## ?? Support

For issues or questions:
- Check error messages carefully - they provide specific guidance
- Review the Swagger documentation at `/swagger`
- Check the `RegisteredPingUsers` table in the database
- Review application logs for detailed error information

---

**Last Updated:** December 2024  
**Version:** 1.1  
**Feature:** Ping Identity Integration (Simplified - No CRUD APIs)
