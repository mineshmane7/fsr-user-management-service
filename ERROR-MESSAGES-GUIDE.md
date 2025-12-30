# ?? API Error Messages Guide

## Overview

This document describes all possible error messages returned by the FSR Property Management API, with specific focus on authentication and authorization errors.

## ?? Login Endpoint Error Messages

### Endpoint: `POST /api/auth/login`

The login endpoint provides **specific error messages** for each failure scenario to help users understand exactly what went wrong.

---

## Error Scenarios & Messages

### 1?? Email Not Registered with Ping Identity

**Scenario:** User's email is not in the `RegisteredPingUsers` table

**HTTP Status:** `401 Unauthorized`

**Error Response:**
```json
{
  "error": "Authentication Failed",
  "message": "Access denied. Your email is not registered with Ping Identity. Please contact your administrator to register your email.",
  "timestamp": "2024-12-30T10:30:00Z"
}
```

**Test Case:**
```bash
POST /api/auth/login
{
  "email": "notregistered@example.com",
  "password": "SomePassword123"
}
```

**What to do:**
- Contact your system administrator
- Request to have your email added to the Ping Identity whitelist
- Administrator needs to update `AuthDbSeeder.cs` with your email

---

### 2?? Ping Registration Deactivated

**Scenario:** User's email exists in `RegisteredPingUsers` but `IsActive = false`

**HTTP Status:** `401 Unauthorized`

**Error Response:**
```json
{
  "error": "Authentication Failed",
  "message": "Your Ping Identity registration has been deactivated. Please contact your administrator for assistance.",
  "timestamp": "2024-12-30T10:30:00Z"
}
```

**Test Case:**
```bash
# Simulate by setting IsActive = false in database
UPDATE RegisteredPingUsers SET IsActive = 0 WHERE Email = 'test@fsr.com';

POST /api/auth/login
{
  "email": "test@fsr.com",
  "password": "SomePassword123"
}
```

**What to do:**
- Contact your system administrator
- Your Ping registration was intentionally deactivated
- Administrator needs to reactivate your registration in the database

---

### 3?? User Account Not Found

**Scenario:** Email is registered with Ping, but no user account has been created in the system

**HTTP Status:** `401 Unauthorized`

**Error Response:**
```json
{
  "error": "Authentication Failed",
  "message": "No account found with this email. Please contact your administrator to create your user account.",
  "timestamp": "2024-12-30T10:30:00Z"
}
```

**Test Case:**
```bash
# Use a Ping-registered email that doesn't have a user account
POST /api/auth/login
{
  "email": "john.doe@fsr.com",  # In RegisteredPingUsers but no Users entry
  "password": "SomePassword123"
}
```

**What to do:**
- Your email is authorized (Ping registered)
- But your user account hasn't been created yet
- Contact administrator to create your user account via `POST /api/admin/users`

---

### 4?? User Account Deactivated

**Scenario:** User account exists but `IsActive = false` in the `Users` table

**HTTP Status:** `401 Unauthorized`

**Error Response:**
```json
{
  "error": "Authentication Failed",
  "message": "Your user account has been deactivated. Please contact your administrator for assistance.",
  "timestamp": "2024-12-30T10:30:00Z"
}
```

**Test Case:**
```bash
# Simulate by setting IsActive = false in Users table
UPDATE Users SET IsActive = 0 WHERE Email = 'admin@fsr.com';

POST /api/auth/login
{
  "email": "admin@fsr.com",
  "password": "Admin@123"
}
```

**What to do:**
- Your user account has been intentionally deactivated
- Contact your system administrator
- Administrator needs to reactivate your account in the database

---

### 5?? Invalid Password

**Scenario:** Email and user account are valid, but password is incorrect

**HTTP Status:** `401 Unauthorized`

**Error Response:**
```json
{
  "error": "Authentication Failed",
  "message": "Invalid password. Please check your credentials and try again.",
  "timestamp": "2024-12-30T10:30:00Z"
}
```

**Test Case:**
```bash
POST /api/auth/login
{
  "email": "admin@fsr.com",
  "password": "WrongPassword123"
}
```

**What to do:**
- Double-check your password (case-sensitive)
- Ensure no extra spaces
- If forgotten, contact administrator for password reset
- Try again with correct password

---

### 6?? Unexpected Server Error

**Scenario:** Any unexpected error during login process

**HTTP Status:** `500 Internal Server Error`

**Error Response:**
```json
{
  "error": "Login Error",
  "message": "An unexpected error occurred during login. Please try again later.",
  "timestamp": "2024-12-30T10:30:00Z"
}
```

**What to do:**
- Try again in a few moments
- If problem persists, contact system administrator
- Administrator should check server logs for details

---

## ? Successful Login

**HTTP Status:** `200 OK`

**Success Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-12-30T11:30:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "admin@fsr.com",
  "userName": "admin",
  "firstName": "System",
  "lastName": "Administrator",
  "roles": ["Admin"],
  "permissions": ["Create", "View", "Edit", "Delete", "Archive", "BulkEdit", "BulkExport", "BulkImport"]
}
```

---

## ?? Login Flow Diagram

```
User Login Request (Email + Password)
        ?
???????????????????????????????????????????????
? 1. Check: Email in RegisteredPingUsers?     ?
?    - Not found ? Error #1 (Not registered)  ?
?    - Found but inactive ? Error #2 (Inactive)?
???????????????????????????????????????????????
        ? PASS
???????????????????????????????????????????????
? 2. Check: User account exists?              ?
?    - Not found ? Error #3 (No account)      ?
???????????????????????????????????????????????
        ? PASS
???????????????????????????????????????????????
? 3. Check: User account active?              ?
?    - Inactive ? Error #4 (Account disabled) ?
???????????????????????????????????????????????
        ? PASS
???????????????????????????????????????????????
? 4. Check: Password correct?                 ?
?    - Incorrect ? Error #5 (Wrong password)  ?
???????????????????????????????????????????????
        ? PASS
???????????????????????????????????????????????
? 5. Generate JWT Token                       ?
?    ? Success: Return token + user info     ?
???????????????????????????????????????????????
```

---

## ?? Testing All Error Scenarios

### Test Script for Postman or curl

```bash
# Test 1: Email not in Ping
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"notregistered@example.com","password":"test123"}'
# Expected: Error #1

# Test 2: Valid email but wrong password
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fsr.com","password":"WrongPassword"}'
# Expected: Error #5

# Test 3: Successful login
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fsr.com","password":"Admin@123"}'
# Expected: Success with JWT token

# Test 4: Ping-registered but no user account
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"john.doe@fsr.com","password":"test123"}'
# Expected: Error #3 (if user account not created yet)
```

---

## ?? User Creation Error Messages

### Endpoint: `POST /api/admin/users`

### Error: Email Not Registered with Ping

**HTTP Status:** `400 Bad Request`

**Error Response:**
```json
{
  "error": "Email 'newuser@example.com' is not registered with Ping Identity. Only users registered with Ping can be created in the system."
}
```

### Error: User Already Exists

**HTTP Status:** `400 Bad Request`

**Error Response:**
```json
{
  "error": "User with email 'admin@fsr.com' already exists"
}
```

### Error: Unauthorized (Not Admin)

**HTTP Status:** `403 Forbidden`

**Error Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Forbidden",
  "status": 403
}
```

---

## ?? Error Summary Table

| Error | HTTP Status | Trigger | Message Keywords | User Action |
|-------|-------------|---------|------------------|-------------|
| **#1** Not in Ping | 401 | Email not in RegisteredPingUsers | "not registered with Ping Identity" | Contact admin to register |
| **#2** Ping Inactive | 401 | RegisteredPingUsers.IsActive = false | "registration has been deactivated" | Contact admin to reactivate |
| **#3** No User Account | 401 | User not in Users table | "No account found" | Contact admin to create account |
| **#4** Account Inactive | 401 | Users.IsActive = false | "account has been deactivated" | Contact admin to reactivate |
| **#5** Wrong Password | 401 | BCrypt verify fails | "Invalid password" | Check password and retry |
| **#6** Server Error | 500 | Unexpected exception | "unexpected error occurred" | Contact admin, check logs |

---

## ?? Troubleshooting Guide

### User Can't Login - Diagnostic Steps

1. **Check error message carefully** - It tells you exactly what's wrong
2. **Error #1 (Not in Ping)?** 
   - Admin: Check `RegisteredPingUsers` table
   - Admin: Add email via `AuthDbSeeder.cs`
3. **Error #2 (Ping Inactive)?**
   - Admin: Update `RegisteredPingUsers` set `IsActive = 1`
4. **Error #3 (No Account)?**
   - Admin: Create user via `POST /api/admin/users`
5. **Error #4 (Account Inactive)?**
   - Admin: Update `Users` set `IsActive = 1`
6. **Error #5 (Wrong Password)?**
   - User: Double-check password
   - Admin: Reset password if needed

---

## ?? Best Practices

### For Developers
- ? Always return specific error messages
- ? Log detailed errors server-side
- ? Never expose sensitive information in error messages
- ? Use appropriate HTTP status codes
- ? Include timestamp in error responses

### For Administrators
- ? Monitor error logs regularly
- ? Keep RegisteredPingUsers up to date
- ? Provide clear onboarding process for new users
- ? Document internal procedures for user activation

### For Users
- ? Read error messages carefully
- ? Contact administrator when needed
- ? Provide exact error message when reporting issues
- ? Don't share your password with anyone

---

## ?? Related Documentation

- [PING-INTEGRATION-GUIDE.md](./PING-INTEGRATION-GUIDE.md) - Ping Identity integration details
- [LOGIN-PING-CHECK-SUMMARY.md](./LOGIN-PING-CHECK-SUMMARY.md) - Login flow enhancement summary
- [README.md](./README.md) - Main project documentation

---

**Last Updated:** December 2024  
**Version:** 1.0  
**Feature:** Enhanced Error Messages for Login Flow
