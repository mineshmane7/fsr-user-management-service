# ?? Simplified Token Authentication in Swagger

## Overview

The API has been enhanced to automatically handle the JWT Bearer token prefix, eliminating the need for users to manually type "Bearer " in Swagger.

## ? What's New

### Before (Old Way)
Users had to manually add the "Bearer " prefix:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### After (New Way - Automatic!)
Users can paste the token directly without any prefix:
```
Authorization: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

The system **automatically** handles the "Bearer " prefix in the backend!

## ?? How It Works

### 1. JWT Bearer Event Handler

In `Program.cs`, we added a custom event handler:

```csharp
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var authorization = context.Request.Headers.Authorization.ToString();
        
        if (!string.IsNullOrEmpty(authorization))
        {
            // If token doesn't have "Bearer " prefix, add it automatically
            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = authorization;
            }
        }
        
        return Task.CompletedTask;
    }
};
```

**What this does:**
- Intercepts incoming requests
- Checks if Authorization header has "Bearer " prefix
- If missing, automatically extracts the token
- Processes authentication normally

### 2. Updated Swagger UI

The Swagger authorization dialog now displays:

```
JWT Authorization header.

You can paste the token directly without 'Bearer ' prefix.

Example: Just paste your token like this:
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

The system will automatically handle the 'Bearer ' prefix for you.
```

## ?? Usage Instructions

### Step-by-Step: Using Swagger Authorization

#### Step 1: Login
```http
POST /api/auth/login
{
  "email": "admin@fsr.com",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJlbWFpbCI6ImFkbWluQGZzci5jb20iLCJuYW1lIjoiYWRtaW4iLCJyb2xlIjoiQWRtaW4iLCJwZXJtaXNzaW9uIjpbIkNyZWF0ZSIsIlZpZXciLCJFZGl0IiwiRGVsZXRlIiwiQXJjaGl2ZSIsIkJ1bGtFZGl0IiwiQnVsa0V4cG9ydCIsIkJ1bGtJbXBvcnQiXSwianRpIjoiZjI3YTQyMGUtNzI5My00NTk2LWI4YzMtNTBhYWQ3ZWU4MmVmIiwiZXhwIjoxNzM1NTU4NzM4LCJpc3MiOiJGU1IuVXNlck1hbmFnZW1lbnQuQXBpIiwiYXVkIjoiRlNSLlVzZXJNYW5hZ2VtZW50LkNsaWVudCJ9.xyz123...",
  "expiresAt": "2024-12-30T11:30:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "admin@fsr.com",
  ...
}
```

#### Step 2: Copy Token
Copy **ONLY** the `accessToken` value (the long string):
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJlbWFpbCI6ImFkbWluQGZzci5jb20iLCJuYW1lIjoiYWRtaW4iLCJyb2xlIjoiQWRtaW4iLCJwZXJtaXNzaW9uIjpbIkNyZWF0ZSIsIlZpZXciLCJFZGl0IiwiRGVsZXRlIiwiQXJjaGl2ZSIsIkJ1bGtFZGl0IiwiQnVsa0V4cG9ydCIsIkJ1bGtJbXBvcnQiXSwianRpIjoiZjI3YTQyMGUtNzI5My00NTk2LWI4YzMtNTBhYWQ3ZWU4MmVmIiwiZXhwIjoxNzM1NTU4NzM4LCJpc3MiOiJGU1IuVXNlck1hbmFnZW1lbnQuQXBpIiwiYXVkIjoiRlNSLlVzZXJNYW5hZ2VtZW50LkNsaWVudCJ9.xyz123...
```

? **Don't copy:**
- The quotes `"`
- The "Bearer " prefix
- Just the token itself!

#### Step 3: Authorize in Swagger
1. Click the **?? Authorize** button at the top of Swagger UI
2. A dialog will pop up
3. **Paste your token directly** in the "Value" field
4. Click **"Authorize"**
5. Click **"Close"**

#### Step 4: Test Protected Endpoints
Now you can call any protected endpoint! The ?? icon will show as locked.

Example:
```http
GET /api/properties
```

The request will automatically include:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## ?? Both Ways Work!

### Option 1: Just Token (Recommended - Easier!)
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
? System automatically adds "Bearer " prefix

### Option 2: With "Bearer " Prefix (Traditional)
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
? System recognizes and processes normally

**Both work perfectly!** Choose whichever you prefer.

## ?? Testing

### Test Case 1: Without "Bearer " Prefix
```bash
curl -X GET "https://localhost:7000/api/properties" \
  -H "Authorization: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```
? **Result:** Works! System adds "Bearer " automatically

### Test Case 2: With "Bearer " Prefix
```bash
curl -X GET "https://localhost:7000/api/properties" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```
? **Result:** Works! System processes normally

### Test Case 3: In Swagger
**Paste in Authorization:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
? **Result:** Works! Protected endpoints accessible

## ?? Benefits

### For Users
? **Simpler** - No need to remember "Bearer " prefix  
? **Faster** - Just copy and paste the token  
? **Less Errors** - No more typos with "Bearer " spelling  
? **User-Friendly** - More intuitive experience  

### For Developers
? **Flexible** - Accepts both formats  
? **Backward Compatible** - Old way still works  
? **Standard Compliant** - Still follows JWT Bearer standard  
? **Better UX** - Reduces user confusion  

## ?? Technical Details

### Request Flow

```
User Pastes Token in Swagger
        ?
Swagger sends: Authorization: {token}
        ?
OnMessageReceived Event Fires
        ?
Check: Does it start with "Bearer "?
  ?? YES ? Use as-is
  ?? NO  ? Extract token, process authentication
        ?
JWT Validation
        ?
User Authenticated ?
```

### Code Location

**File:** `FSR.UM.Api\Program.cs`

**Lines:** JWT Authentication Configuration section

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // ... TokenValidationParameters ...
        
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Custom token extraction logic
            }
        };
    });
```

## ?? Common Scenarios

### Scenario 1: First Time User
**Question:** "How do I authenticate in Swagger?"

**Answer:**
1. Call `/api/auth/login` with your credentials
2. Copy the `accessToken` from response
3. Click ?? Authorize button
4. Paste token (no "Bearer " needed!)
5. Click Authorize
6. You're authenticated! ??

### Scenario 2: Token Expired
**Question:** "My requests are returning 401 Unauthorized"

**Answer:**
- Your token has expired (default: 60 minutes)
- Login again to get a new token
- Update authorization with new token

### Scenario 3: Using Postman
**Question:** "How do I use the token in Postman?"

**Answer - Two Options:**

**Option A (Simple):**
```
Header: Authorization
Value: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Option B (Traditional):**
- Click "Authorization" tab
- Select "Bearer Token" from Type dropdown
- Paste token in "Token" field
- Postman automatically adds "Bearer " prefix

### Scenario 4: Using curl
**Question:** "How do I use curl with this API?"

**Answer - Both work:**

**Without "Bearer ":**
```bash
curl -X GET "https://localhost:7000/api/properties" \
  -H "Authorization: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**With "Bearer ":**
```bash
curl -X GET "https://localhost:7000/api/properties" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## ?? Best Practices

### For API Users
? **Do:**
- Copy only the token value
- Store tokens securely
- Request new token when expired
- Use HTTPS in production

? **Don't:**
- Share your tokens with others
- Store tokens in version control
- Use expired tokens
- Include quotes or extra spaces

### For API Administrators
? **Do:**
- Educate users about this feature
- Update API documentation
- Test both token formats
- Monitor authentication logs

## ?? Related Documentation

- [ERROR-MESSAGES-GUIDE.md](./ERROR-MESSAGES-GUIDE.md) - Authentication error messages
- [PING-INTEGRATION-GUIDE.md](./PING-INTEGRATION-GUIDE.md) - Ping Identity integration
- [LOGIN-PING-CHECK-SUMMARY.md](./LOGIN-PING-CHECK-SUMMARY.md) - Login flow details

## ?? Troubleshooting

### Issue: Still getting 401 Unauthorized

**Check:**
1. ? Token is not expired (check `expiresAt` field)
2. ? Token is copied correctly (no extra spaces)
3. ? User has required permissions for the endpoint
4. ? User's Ping registration is active

### Issue: Swagger not accepting token

**Solution:**
1. Make sure you clicked "Authorize" button (?? icon)
2. Paste token in the Value field
3. Click "Authorize" button in the dialog
4. Click "Close"
5. Look for ?? lock icon on endpoints

### Issue: Token looks wrong

**Valid Token Format:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

**Characteristics:**
- Three parts separated by dots (`.`)
- Base64 encoded strings
- No spaces
- Alphanumeric with dots and underscores

## ?? Feature Summary

| Feature | Status | Description |
|---------|--------|-------------|
| Token without "Bearer " | ? Supported | System auto-adds prefix |
| Token with "Bearer " | ? Supported | Traditional method works |
| Swagger UI Integration | ? Updated | Clear instructions for users |
| Backward Compatible | ? Yes | Old implementations still work |
| Security | ? Maintained | No security compromises |

---

**Last Updated:** December 2024  
**Version:** 1.0  
**Feature:** Automatic Bearer Token Prefix Handling
