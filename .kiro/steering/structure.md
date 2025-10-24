# Project Structure

## Solution Organization

The Visage solution follows a modular architecture with clear separation of concerns:

```
Visage.sln
├── Visage.AppHost/              # Aspire orchestration host
├── Visage.ServiceDefaults/      # Shared service configuration
├── Visage.Shared/              # Common models and DTOs
├── Visage.FrontEnd/            # Frontend applications
├── Visage.Services.Registrations/ # Registration service
├── services/                   # Additional microservices
└── tests/                     # Test projects
```

## Key Directories

### Application Host
- **Visage.AppHost/** - Aspire application host that orchestrates all services
  - Configures service dependencies and communication
  - Manages environment variables and secrets
  - Integrates with Scalar API documentation

### Frontend Applications
- **Visage.FrontEnd/Visage.FrontEnd/** - .NET MAUI mobile/desktop app
- **Visage.FrontEnd/Visage.FrontEnd.Web/** - Blazor server-side web app
- **Visage.FrontEnd/Visage.FrontEnd.Web.Client/** - Blazor WebAssembly client
- **Visage.FrontEnd/Visage.FrontEnd.Shared/** - Shared UI components library

### Backend Services
- **Visage.Services.Registrations/** - Registration and profile management API
- **services/Visage.Services.Eventing/** - Event management service
- **services/CloudinaryImageSigning/** - Node.js service for image upload signing

### Shared Libraries
- **Visage.Shared/** - Common models, DTOs, and shared logic
- **Visage.ServiceDefaults/** - Common service configuration and extensions

### Testing Structure
- **tests/Visage.Test.Aspire/** - Aspire integration tests
- **tests/Visage.Tests.Integration/** - Service integration tests
- **tests/Visage.Tests.Unit/** - Unit tests
- **tests/Visage.Tests.EndToEnd/** - End-to-end tests with Playwright
- **tests/APITests/** - API-specific tests

## Naming Conventions

### Projects
- Use `Visage.` prefix for all .NET projects
- Service projects: `Visage.Services.{ServiceName}`
- Frontend projects: `Visage.FrontEnd.{Platform}`
- Test projects: `Visage.Tests.{TestType}` or `Visage.Test.{Framework}`

### Files
- API endpoints include accompanying `.http` files for testing
- Each service should have its own database context (e.g., `RegistrantDB.cs`)
- Minimal API endpoints organized in separate files (e.g., `ProfileApi.cs`)

## Configuration Files
- **appsettings.json** - Base application settings
- **appsettings.Development.json** - Development-specific overrides
- **launchSettings.json** - Development launch profiles
- **.env** - Environment variables for Aspire
- **docker-compose.yml** - Container orchestration (development)

## Architecture Patterns

### Service Communication
- HTTP/REST APIs between services
- Aspire service discovery and configuration
- JWT Bearer tokens for authentication
- OpenAPI/Scalar for API documentation

### Data Access
- Entity Framework Core with SQL Server
- Database-per-service pattern
- Connection strings managed through Aspire parameters

### Frontend Architecture
- Shared UI components across platforms
- Blazor Hybrid for web applications
- MAUI for cross-platform native apps
- Auth0 integration for authentication flows

## Development Guidelines

### File Organization
- Keep related functionality together in the same project
- Use folders to organize by feature within projects
- Place shared models in `Visage.Shared` project
- API-specific logic stays within service projects

### Dependencies
- Services should reference `Visage.ServiceDefaults` for common configuration
- Frontend projects reference `Visage.FrontEnd.Shared` for UI components
- All projects can reference `Visage.Shared` for common models
- Avoid circular dependencies between services