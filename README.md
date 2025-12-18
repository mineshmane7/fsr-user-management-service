# FSR User Management Service

A modern .NET 8 User Management Service built with clean architecture principles, designed to handle user authentication, authorization, and property management.

## 🏗️ Project Structure

The solution follows a clean architecture pattern with clear separation of concerns:

```
FSR.UserManagement/
├── FSR.UM.Api/                              # API Layer - Entry point
│   ├── Endpoints/                           # Minimal API endpoints
│   │   ├── Endpoints.cs                     # Endpoint registration
│   │   └── UserEndpoints.cs                 # User-related endpoints
│   ├── Program.cs                           # Application startup
│   └── appsettings.json                     # Configuration settings
│
├── FSR.UM.Core/                             # Core Domain Layer
│   ├── Models/                              # Domain entities
│   │   ├── User.cs                          # User entity
│   │   ├── Role.cs                          # Role entity
│   │   ├── Permission.cs                    # Permission entity
│   │   ├── OrgTier.cs                       # Organization tier entity
│   │   ├── Property.cs                      # Property entity
│   │   └── Unit.cs                          # Unit entity
│   └── Interfaces/                          # Core abstractions
│       ├── IUserService.cs                  # User service contract
│       └── IPropertyRepository.cs           # Property repository contract
│
├── FSR.UM.Infrastructure/                   # Business Logic Layer
│   └── Services/
│       └── UserService.cs                   # User service implementation
│
├── FSR.UM.Infrastructure.SqlServer/         # Data Access Layer
│   ├── Db/                                  # Database contexts
│   │   ├── ApplicationDbContext.cs          # Main DB context
│   │   ├── AuthDb/
│   │   │   └── AuthDbContext.cs            # Authentication DB context
│   │   └── PropertyDb/
│   │       └── PropertyDbContext.cs        # Property management DB context
│   ├── Repositories/
│   │   └── PropertyRepository.cs           # Property repository implementation
│   └── DependencyInjection.cs              # Service registration
│
└── FSR.UM.Infrastructure.SqlServer.Migrations/  # EF Core Migrations
```

## 🎯 Architecture Overview

This project implements **Clean Architecture** principles:

- **FSR.UM.Api**: The presentation layer that exposes RESTful endpoints using ASP.NET Core Minimal APIs
- **FSR.UM.Core**: The domain layer containing business entities and interfaces (no dependencies on other projects)
- **FSR.UM.Infrastructure**: The application layer containing business logic implementations
- **FSR.UM.Infrastructure.SqlServer**: The infrastructure layer for data persistence using SQL Server
- **FSR.UM.Infrastructure.SqlServer.Migrations**: Database migration management

### Key Design Patterns
- **Repository Pattern**: Abstraction for data access operations
- **Dependency Injection**: Loose coupling between layers
- **Interface Segregation**: Clean contracts between layers
- **Separation of Concerns**: Each project has a single responsibility

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB (or SQL Server instance)
- Visual Studio 2022 or Visual Studio Code

### Database Configuration

The application uses two separate databases:

1. **CyanAuth**: Authentication and user management
2. **CyanPropertyManagement**: Property and unit management

Connection strings are configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "CyanAuth": "Server=(localdb)\\mssqllocaldb;Database=CyanAuth;...",
    "CyanPropertyManagement": "Server=(localdb)\\mssqllocaldb;Database=CyanPropertyManagement;..."
  }
}
```

### Running the Application

1. **Clone the repository**
   ```bash
   git clone https://github.com/mineshmane7/fsr-user-management-service
   cd FSR.UserManagement
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the API**
   ```bash
   cd FSR.UM.Api
   dotnet run
   ```

4. **Access Swagger UI**
   - Navigate to: `https://localhost:<port>/swagger`
   - Swagger UI provides interactive API documentation

## 📦 Dependencies

### Core Packages
- **ASP.NET Core 8.0**: Web framework
- **Swashbuckle.AspNetCore**: API documentation (Swagger/OpenAPI)
- **Entity Framework Core**: Data access (SQL Server provider)

## 🔑 Key Features

- **User Management**: CRUD operations for user accounts
- **Property Management**: Handle properties and units
- **Role-Based Access**: Support for roles and permissions
- **Organization Hierarchy**: Multi-tier organization structure
- **RESTful API**: Clean minimal API endpoints
- **Swagger Documentation**: Interactive API documentation
- **CORS Enabled**: Cross-origin resource sharing configured

## 🛠️ Development

### Project Dependencies

```
FSR.UM.Api
  ├── FSR.UM.Core
  ├── FSR.UM.Infrastructure
  │   └── FSR.UM.Core
  └── FSR.UM.Infrastructure.SqlServer
      ├── FSR.UM.Core
      └── FSR.UM.Infrastructure
          └── FSR.UM.Core

FSR.UM.Infrastructure.SqlServer.Migrations
  └── FSR.UM.Infrastructure.SqlServer
      ├── FSR.UM.Core
      └── FSR.UM.Infrastructure
          └── FSR.UM.Core
```

**Dependency Flow:**
- `FSR.UM.Core`: No dependencies (domain layer)
- `FSR.UM.Infrastructure`: References `FSR.UM.Core`
- `FSR.UM.Infrastructure.SqlServer`: References `FSR.UM.Core` and `FSR.UM.Infrastructure`
- `FSR.UM.Infrastructure.SqlServer.Migrations`: References `FSR.UM.Infrastructure.SqlServer`
- `FSR.UM.Api`: References `FSR.UM.Core`, `FSR.UM.Infrastructure`, and `FSR.UM.Infrastructure.SqlServer`

### Adding New Features

1. Define models in `FSR.UM.Core/Models`
2. Create interfaces in `FSR.UM.Core/Interfaces`
3. Implement services in `FSR.UM.Infrastructure/Services`
4. Add repository implementations in `FSR.UM.Infrastructure.SqlServer/Repositories`
5. Register endpoints in `FSR.UM.Api/Endpoints`

## 📝 API Endpoints

API endpoints are registered through:
- `UserEndpoints.cs`: User management operations
- Extensible endpoint registration pattern via `Endpoints.cs`

Access the full API documentation via Swagger UI when running the application.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/YourFeature`)
3. Commit your changes (`git commit -m 'Add some feature'`)
4. Push to the branch (`git push origin feature/YourFeature`)
5. Open a Pull Request