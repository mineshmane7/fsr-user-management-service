# Quick Fix for Migration Error

## Problem
The User model's `Id` property was changed from `int` to `Guid`, but the existing database has an IDENTITY column that cannot be automatically converted.

## Solution: Drop and Recreate the Database

### Step 1: Drop the existing databases
```bash
# From the Migrations directory
cd FSR.UM.Infrastructure.SqlServer.Migrations

# Drop AuthDb
dotnet ef database drop --context AuthDbContext --force

# Drop PropertyDb  
dotnet ef database drop --context PropertyDbContext --force
```

### Step 2: Run the application
The auto-migration in `Program.cs` will:
1. Create fresh databases
2. Apply all migrations (including the new RBAC migration)
3. Seed data automatically

```bash
cd ../FSR.UM.Api
dotnet run
```

### Expected Result
? CyanAuth database created  
? CyanPropertyManagement database created  
? All tables created with correct schema  
? Default permissions seeded  
? Default roles seeded  
? Admin user created (admin@fsr.com / Admin@123)

---

## Alternative: Manual Database Drop (SQL Server Management Studio)

1. Open SQL Server Management Studio
2. Connect to `localhost`
3. Right-click on `CyanAuth` database ? Delete
4. Right-click on `CyanPropertyManagement` database ? Delete
5. Run the application: `dotnet run`

---

## Verification

After the application starts successfully, you should see:
```
? Database migrations and seeding completed successfully.
```

Then test the login:
1. Open Swagger: `https://localhost:<port>/`
2. Use `POST /api/auth/login`
3. Login with:
   ```json
   {
     "email": "admin@fsr.com",
     "password": "Admin@123"
   }
   ```
