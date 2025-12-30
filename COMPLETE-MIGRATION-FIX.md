# Complete Migration Fix - Step by Step

## Problem
The migration is trying to convert User.Id from `int` IDENTITY to `Guid`, which SQL Server cannot do. We need to start fresh.

## Solution: Remove Old Migrations and Create Fresh Ones

### Step 1: Delete the Databases
```powershell
# Navigate to Migrations directory
cd FSR.UM.Infrastructure.SqlServer.Migrations

# Drop both databases
dotnet ef database drop --context AuthDbContext --force
dotnet ef database drop --context PropertyDbContext --force
```

### Step 2: Delete ALL Old Migration Files

Delete these directories completely:
```
FSR.UM.Infrastructure.SqlServer.Migrations/Migrations/AuthDb/
FSR.UM.Infrastructure.SqlServer.Migrations/Migrations/PropertyDb/
```

**In Windows Explorer:**
1. Navigate to: `FSR.UM.Infrastructure.SqlServer.Migrations\Migrations\`
2. Delete the `AuthDb` folder
3. Delete the `PropertyDb` folder

**OR via PowerShell (from Migrations directory):**
```powershell
# Make sure you're in the Migrations directory
cd FSR.UM.Infrastructure.SqlServer.Migrations

# Remove old migrations
Remove-Item -Recurse -Force .\Migrations\AuthDb\
Remove-Item -Recurse -Force .\Migrations\PropertyDb\
```

### Step 3: Create Fresh Migrations

```powershell
# Still in Migrations directory
# Create fresh migration for AuthDb with RBAC
dotnet ef migrations add InitialCreateWithRBAC --context AuthDbContext --output-dir Migrations/AuthDb

# Create fresh migration for PropertyDb
dotnet ef migrations add InitialCreate --context PropertyDbContext --output-dir Migrations/PropertyDb
```

### Step 4: Run the Application

```powershell
# Navigate to API project
cd ..\FSR.UM.Api

# Run the application
dotnet run
```

## Expected Result

You should see:
```
? Database migrations and seeding completed successfully.
```

The application will:
1. ? Create fresh `CyanAuth` database with correct Guid-based User table
2. ? Create fresh `CyanPropertyManagement` database
3. ? Apply all migrations
4. ? Seed 8 standard permissions
5. ? Seed 3 default roles (Admin, Manager, User)
6. ? Seed admin user (admin@fsr.com / Admin@123)
7. ? Assign permissions to roles
8. ? Assign Admin role to admin user

## Verification

### 1. Check Console Output
You should see successful migration messages, no errors.

### 2. Test Login via Swagger
```
1. Open: https://localhost:<port>/
2. Navigate to: POST /api/auth/login
3. Login with:
   {
     "email": "admin@fsr.com",
     "password": "Admin@123"
   }
4. You should receive a JWT token with roles and permissions
```

### 3. Verify Database Tables (Optional)
In SQL Server Management Studio or Azure Data Studio:

**CyanAuth database should have:**
- Users (with Id as uniqueidentifier/Guid)
- Roles
- Permissions
- UserRoleAssignments
- RolePermissionAssignments
- OrgTiers

**CyanPropertyManagement database should have:**
- Properties
- Units

## Troubleshooting

### If migrations fail:
```powershell
# Clean and rebuild
dotnet clean
dotnet build
```

### If database drop fails (database in use):
1. Stop the application (Ctrl+C)
2. Close any SQL Server Management Studio connections
3. Try the drop command again

### To verify migrations are deleted:
```powershell
# List migrations (should show none)
dotnet ef migrations list --context AuthDbContext
dotnet ef migrations list --context PropertyDbContext
```

## Important Notes

?? **This will delete all existing data!**  
? Safe for development environment  
? Do NOT use this approach in production with existing data  

For production with existing data, you would need a different strategy (data migration scripts).
