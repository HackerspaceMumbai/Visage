# Research: SQL Server Aspire Integration

**Feature**: SQL Server Aspire Integration  
**Date**: 2025-10-17  
**Purpose**: Research best practices and patterns for integrating SQL Server as a first-class Aspire resource

## Executive Summary

This research covers the integration of SQL Server into .NET Aspire as a managed resource, replacing standalone SQL Server configuration. Key findings include using `Aspire.Hosting.SqlServer` NuGet package for AppHost registration, leveraging Aspire's built-in health checks and connection string management, and ensuring proper service dependency ordering with `.WaitFor()`.

## Research Areas

### 1. Aspire SQL Server Hosting Patterns

**Decision**: Use `AddSqlServer()` extension method with named resources and automatic connection string injection

**Rationale**:
- .NET Aspire 10 provides first-class SQL Server hosting support via `Aspire.Hosting.SqlServer` package
- Named resources allow multiple databases per SQL Server instance
- Connection strings are automatically injected into dependent services via Aspire's service discovery
- Health checks are built-in and report to the Aspire dashboard without additional configuration
- Supports both containerized (local dev) and external (production) SQL Server instances

**Implementation Pattern**:
```csharp
// In Visage.AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sql")
    .WithHealthCheck();

var registrationDb = sqlServer.AddDatabase("registrationdb");
var eventingDb = sqlServer.AddDatabase("eventingdb");

builder.AddProject<Projects.Visage_Services_Registrations>("registrations")
    .WithReference(registrationDb)
    .WaitFor(registrationDb);

builder.AddProject<Projects.Visage_Services_Eventing>("eventing")
    .WithReference(eventingDb)
    .WaitFor(eventingDb);
```

**Alternatives Considered**:
- **Manual connection string management**: Rejected - Defeats the purpose of Aspire orchestration, requires manual appsettings.json configuration
- **Single shared database**: Rejected - Violates separation of concerns, makes independent service deployment difficult
- **Separate SQL Server instances per service**: Rejected - Unnecessary resource overhead for current scale

**References**:
- [.NET Aspire SQL Server integration](https://learn.microsoft.com/en-us/dotnet/aspire/database/sql-server-integration)
- [Aspire.Hosting.SqlServer package documentation](https://www.nuget.org/packages/Aspire.Hosting.SqlServer)

---

### 2. EF Core Configuration with Aspire-Managed Connections

**Decision**: Use `AddSqlServerDbContext<T>()` extension in service startup with named connection from Aspire

**Rationale**:
- Aspire provides `Aspire.Microsoft.EntityFrameworkCore.SqlServer` client package for services
- `AddSqlServerDbContext<T>()` automatically configures DbContext with Aspire-injected connection string
- Connection pooling is handled automatically with sensible defaults
- Health checks for database connectivity are included
- Migrations can run automatically on startup using `ApplyMigrations()` extension

**Implementation Pattern**:
```csharp
// In services/Visage.Services.Registrations/Program.cs
builder.AddSqlServerDbContext<RegistrantDB>("registrationdb");

// DbContext remains unchanged - connection string injected automatically
```

**Alternatives Considered**:
- **Manual DbContext configuration with connection string**: Rejected - Bypasses Aspire's connection management and health checks
- **Connection string from appsettings.json**: Rejected - Requires manual configuration, doesn't integrate with Aspire dashboard

**References**:
- [Aspire EF Core SQL Server integration](https://learn.microsoft.com/en-us/dotnet/aspire/database/ef-sql-server-integration)
- [Aspire.Microsoft.EntityFrameworkCore.SqlServer package](https://www.nuget.org/packages/Aspire.Microsoft.EntityFrameworkCore.SqlServer)

---

### 3. Service Dependency Management and Startup Order

**Decision**: Use `.WaitFor()` to ensure database resources are ready before dependent services start

**Rationale**:
- Aspire's `.WaitFor()` extension ensures SQL Server is healthy before starting dependent services
- Prevents "database not available" errors during startup
- Integrates with Aspire's health check system
- Provides automatic retry logic during startup
- Essential for reliable containerized deployments

**Implementation Pattern**:
```csharp
builder.AddProject<Projects.Visage_Services_Registrations>("registrations")
    .WithReference(registrationDb)
    .WaitFor(registrationDb);  // Service won't start until database is healthy
```

**Alternatives Considered**:
- **Manual retry logic in services**: Rejected - Duplicates Aspire functionality, harder to maintain
- **No dependency management**: Rejected - Unreliable startup, especially in containerized environments

**References**:
- [Aspire orchestration patterns](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/app-host-overview)

---

### 4. Connection String Security and Environment Configuration

**Decision**: Use Aspire parameters for connection string components, avoiding plain-text secrets

**Rationale**:
- Aspire parameters support environment-specific configuration
- Connection strings can reference Azure Key Vault, user secrets, or environment variables
- Development uses default containerized SQL Server
- Production uses external SQL Server with secure connection string injection
- No secrets in source control

**Implementation Pattern**:
```csharp
// AppHost can use parameters for production configuration
var sqlPassword = builder.AddParameter("sql-password", secret: true);
var sqlServer = builder.AddSqlServer("sql", password: sqlPassword);
```

**Alternatives Considered**:
- **Connection strings in appsettings.json**: Rejected - Security risk, doesn't support environment-specific config
- **Environment variables only**: Rejected - Aspire parameters provide better integration and type safety

**References**:
- [Aspire external parameters](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/external-parameters)

---

### 5. Testing Aspire-Orchestrated Services with SQL Server

**Decision**: Use `Aspire.Hosting.Testing` package to create integration tests with real Aspire-managed SQL Server

**Rationale**:
- Tests run against actual Aspire orchestration, not mocks
- Validates entire service dependency chain
- Ensures connection string injection works correctly
- Tests health checks and startup order
- Uses `DistributedApplicationTestingBuilder` for test scenarios

**Implementation Pattern**:
```csharp
[TestClass]
public class SqlServerIntegrationTests
{
    [TestMethod]
    public async Task RegistrationService_Connects_To_SqlServer()
    {
        await using var app = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Visage_AppHost>();
        
        await app.StartAsync();
        
        var httpClient = app.CreateHttpClient("registrations");
        var response = await httpClient.GetAsync("/health");
        
        response.Should().BeSuccessful();
    }
}
```

**Alternatives Considered**:
- **Mock database connections**: Rejected - Doesn't test actual Aspire integration
- **Test against separate SQL Server**: Rejected - Doesn't validate Aspire orchestration

**References**:
- [Testing .NET Aspire apps](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/testing)
- [Aspire.Hosting.Testing package](https://www.nuget.org/packages/Aspire.Hosting.Testing)

---

### 6. EF Core Migration Strategy with Aspire

**Decision**: Run migrations automatically on service startup using `EnsureCreated()` or `Migrate()` for development

**Rationale**:
- Ensures database schema is always up-to-date
- Works seamlessly with Aspire's containerized SQL Server
- No manual migration steps required for developers
- Production deployments can use separate migration jobs if needed

**Implementation Pattern**:
```csharp
// In Program.cs after service startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RegistrantDB>();
    await dbContext.Database.MigrateAsync();  // Apply pending migrations
}
```

**Alternatives Considered**:
- **Manual migration execution**: Rejected - Error-prone, slows development
- **Database.EnsureCreated()**: Considered for dev only - Doesn't respect migrations

**References**:
- [EF Core migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Applying migrations at runtime](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)

---

## Open Questions and Decisions

### Resolved

1. **Q**: Should we use separate databases per service or a shared database?  
   **A**: Separate databases (`registrationdb` and `eventingdb`) for service independence and future scalability

2. **Q**: How do we handle migrations in production vs development?  
   **A**: Auto-migrate on startup for development; production can use the same pattern or dedicated migration jobs

3. **Q**: Should we use containerized SQL Server for local development?  
   **A**: Yes, Aspire automatically provisions containerized SQL Server with `AddSqlServer()` for local development

### Remaining (None)

All technical questions resolved through research.

## Recommended Approach

1. **AppHost Changes**:
   - Add `Aspire.Hosting.SqlServer` NuGet package
   - Register SQL Server resource with two databases
   - Configure service dependencies with `.WaitFor()`

2. **Service Changes**:
   - Add `Aspire.Microsoft.EntityFrameworkCore.SqlServer` to both services
   - Replace manual DbContext configuration with `AddSqlServerDbContext<T>()`
   - Remove connection strings from appsettings.json
   - Add migration execution on startup

3. **Testing**:
   - Add `Aspire.Hosting.Testing` package to test project
   - Create integration tests for database connectivity
   - Validate health checks and service startup order

4. **Security**:
   - Use Aspire parameters for production connection strings
   - Leverage user secrets for local development
   - No connection strings in source control

## Next Steps

Proceed to Phase 1: Design (data-model.md, contracts/, quickstart.md)
