# Technology Stack

## Core Framework
- **.NET 10.0** - Primary development platform
- **Aspire** - Distributed application framework for orchestration and service management
- **C#** with nullable reference types enabled

## Frontend Technologies
- **Blazor Hybrid** - Web application framework
- **Blazor WebAssembly** - Client-side web applications
- **.NET MAUI** - Cross-platform mobile and desktop applications
- **Shared UI Library** - Common components across platforms

## Backend Services
- **Minimal APIs** - Lightweight HTTP services
- **Entity Framework Core** - Data access with SQL Server
- **JWT Bearer Authentication** - Auth0 integration
- **Scalar OpenAPI** - API documentation and testing

## External Services
- **Azure OpenAI** - AI-powered features
- **Cloudinary** - Image storage and processing
- **Auth0** - Authentication and authorization
- **Azure Monitor & Application Insights** - Monitoring and telemetry
- **Microsoft Clarity** - User analytics

## Testing Framework
- **TUnit** - Primary testing framework
- **Fluent Assertions** - Assertion library
- **Playwright** - End-to-end testing
- **bUnit** - Blazor component testing (limited use)
- **NSubstitute** - Mocking framework
- **NBomber** - Load testing
- **OWASP ZAP** - Security testing
- **Stryker** - Mutation testing

## Development Tools
- **Docker/Podman** - Containerization for local development
- **Node.js** - For Cloudinary image signing service
- **Azure Developer CLI (azd)** - Deployment to Azure Container Apps

## Common Commands

### Build and Run
```bash
# Build entire solution
dotnet build

# Run Aspire AppHost (starts all services)
dotnet run --project Visage.AppHost

# Run specific service
dotnet run --project Visage.Services.Registrations
```

### Testing
```bash
# Run all tests
dotnet test

# Run integration tests
dotnet test tests/Visage.Tests.Integration/Visage.Tests.Integration.csproj

# Run Aspire tests
dotnet test tests/Visage.Test.Aspire/Visage.Test.Aspire.csproj
```

### Database
```bash
# Apply migrations
dotnet ef database update --project Visage.Services.Registrations
```

## Configuration
- Environment variables via `.env` file
- User secrets for sensitive data
- Aspire parameter system for service configuration
- Auth0 configuration for authentication flows