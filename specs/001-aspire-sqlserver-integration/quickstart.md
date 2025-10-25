# Quickstart: SQL Server Aspire Integration

**Feature**: SQL Server Aspire Integration  
**Date**: 2025-10-17  
**Purpose**: Developer quickstart guide for working with Aspire-managed SQL Server

## Prerequisites

- .NET 10 SDK installed
- Docker Desktop running (for containerized SQL Server in development)
- Visual Studio 2025 or VS Code with C# Dev Kit
- Existing Visage solution cloned

## Quick Start (5 minutes)

### 1. Update AppHost (2 minutes)

Add the SQL Server hosting package and register SQL Server with databases:

```bash
cd Visage.AppHost
dotnet add package Aspire.Hosting.SqlServer
```

Update `Visage.AppHost/Program.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Register SQL Server as a managed resource
var sqlServer = builder.AddSqlServer("sql")
    .WithHealthCheck();

// Add databases for each service
var registrationDb = sqlServer.AddDatabase("registrationdb");
var eventingDb = sqlServer.AddDatabase("eventingdb");

// Update service registrations to reference databases and wait for them
var registrations = builder.AddProject<Projects.Visage_Services_Registrations>("registrations")
    .WithReference(registrationDb)
    .WaitFor(registrationDb);

var eventing = builder.AddProject<Projects.Visage_Services_Eventing>("eventing")
    .WithReference(eventingDb)
    .WaitFor(eventingDb);

builder.Build().Run();
```

### 2. Update Registration Service (1 minute)

Add the Aspire EF Core SQL Server package:

```bash
cd services/Visage.Services.Registrations
dotnet add package Aspire.Microsoft.EntityFrameworkCore.SqlServer
```

Update `Program.cs` to use Aspire-injected connection:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add service defaults (already exists)
builder.AddServiceDefaults();

// Replace manual DbContext configuration with Aspire-managed connection
builder.AddSqlServerDbContext<RegistrantDB>("registrationdb");

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RegistrantDB>();
    await dbContext.Database.MigrateAsync();
}

app.MapDefaultEndpoints();
// ... rest of app configuration
```

Remove connection string from `appsettings.json` (Aspire manages it now).

### 3. Update Eventing Service (1 minute)

Add the Aspire EF Core SQL Server package:

```bash
cd services/Visage.Services.Eventing
dotnet add package Aspire.Microsoft.EntityFrameworkCore.SqlServer
```

Update `Program.cs` (same pattern as Registration service):

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSqlServerDbContext<EventDB>("eventingdb");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EventDB>();
    await dbContext.Database.MigrateAsync();
}

app.MapDefaultEndpoints();
// ... rest of app configuration
```

Remove connection string from `appsettings.json`.

### 4. Run and Verify (1 minute)

```bash
cd Visage.AppHost
dotnet run
```

Open the Aspire dashboard (URL shown in console output, typically `https://localhost:17073/`).

**Verify**:
- ✅ SQL Server resource shows as "Healthy" (green status)
- ✅ Both `registrationdb` and `eventingdb` are listed as child resources
- ✅ Registration and Eventing services show as "Healthy"
- ✅ Services started in correct order (SQL Server → Databases → Services)

## Development Workflow

### Running the Solution

```bash
# From repository root
cd Visage.AppHost
dotnet run
```

The Aspire dashboard opens automatically, showing all resources and their health status.

### Accessing the Aspire Dashboard

- URL: Shown in console output (typically `https://localhost:17073/`)
- View SQL Server health status
- Monitor service dependencies and startup order
- View connection strings (in dev environment)
- Check logs and traces for all resources

### Database Management

#### View Database Schema

```bash
# SQL Server is containerized - connect via Aspire-provided connection string
# Get connection string from Aspire dashboard under SQL Server resource
```

Use SQL Server Management Studio or Azure Data Studio to connect to `localhost` with the credentials shown in the Aspire dashboard.

#### Run Migrations Manually

Migrations run automatically on service startup, but you can also run them manually:

```bash
cd services/Visage.Services.Registrations
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

#### Reset Database (Development)

```bash
# Stop the AppHost
# Delete Docker container:
docker ps
docker stop <sql-container-id>
docker rm <sql-container-id>

# Restart AppHost - fresh SQL Server container will be created
cd Visage.AppHost
dotnet run
```

### Troubleshooting

#### Services won't start

**Symptom**: Services stuck in "Starting" state

**Solution**:
1. Check Aspire dashboard for SQL Server health status
2. Ensure Docker Desktop is running
3. Check service logs for connection errors
4. Verify `.WaitFor()` is configured correctly in AppHost

#### Migration errors

**Symptom**: Service unhealthy with migration failure logs

**Solution**:
1. Review migration error in service logs (Aspire dashboard → Service → Logs)
2. Fix migration code
3. Restart AppHost
4. If needed, manually rollback: `dotnet ef database update <PreviousMigration>`

#### Connection string not found

**Symptom**: Service error about missing connection string

**Solution**:
1. Verify `WithReference(database)` is called for the service in AppHost
2. Check that database resource name matches (`"registrationdb"` or `"eventingdb"`)
3. Ensure `AddSqlServerDbContext<T>()` uses the same name

## Testing

### Integration Tests with Aspire

Create tests using `Aspire.Hosting.Testing`:

```csharp
[TestClass]
public class SqlServerIntegrationTests
{
    [TestMethod]
    public async Task Registration_Service_Connects_To_Database()
    {
        // Arrange
        await using var app = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Visage_AppHost>();
        
        await app.StartAsync();
        
        // Act
        var httpClient = app.CreateHttpClient("registrations");
        var response = await httpClient.GetAsync("/health");
        
        // Assert
        response.Should().BeSuccessful();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

Run tests:

```bash
cd tests/Visage.Tests.Integration
dotnet test
```

## Configuration

### Development Configuration

Aspire automatically configures SQL Server for development:

- **Server**: Containerized SQL Server on `localhost`
- **Authentication**: SQL authentication with generated credentials
- **Databases**: `registrationdb` and `eventingdb` auto-created
- **Health Checks**: Enabled by default

No manual configuration required!

### Production Configuration

For production, use Aspire parameters:

```csharp
// In Visage.AppHost/Program.cs
var sqlPassword = builder.AddParameter("sql-password", secret: true);
var sqlConnectionString = builder.AddParameter("sql-connection-string");

var sqlServer = builder.AddSqlServer("sql", password: sqlPassword)
    .WithConnectionStringParameter(sqlConnectionString);
```

Set values in:

- **Azure**: App Service configuration or Key Vault
- **Local production testing**: User secrets (`dotnet user-secrets set sql-password "YourPassword"`)

### Environment-Specific Settings

Aspire supports multiple environments:

```bash
# Development (default)
dotnet run

# Staging
dotnet run --environment Staging

# Production
dotnet run --environment Production
```

Configure environment-specific Aspire parameters in `appsettings.{Environment}.json` in the AppHost project.

## Next Steps

- Review [data-model.md](./data-model.md) for database architecture details
- See [plan.md](./plan.md) for full implementation plan
- Proceed to `/speckit.tasks` to generate implementation tasks
- Review Aspire documentation: [.NET Aspire SQL Server integration](https://learn.microsoft.com/en-us/dotnet/aspire/database/sql-server-integration)

## Common Commands Reference

```bash
# Run the entire solution
cd Visage.AppHost && dotnet run

# Add a new migration (Registration service)
cd services/Visage.Services.Registrations
dotnet ef migrations add <MigrationName>

# Add a new migration (Eventing service)
cd services/Visage.Services.Eventing
dotnet ef migrations add <MigrationName>

# Run integration tests
cd tests/Visage.Tests.Integration && dotnet test

# View Docker containers
docker ps

# Stop all Docker containers
docker stop $(docker ps -q)

# Remove SQL Server container (for fresh start)
docker rm <container-id>
```

## FAQ

**Q: Do I need to install SQL Server locally?**  
A: No! Aspire automatically provisions a containerized SQL Server for development.

**Q: How do I see the database schema?**  
A: Connect to `localhost` using the connection string from the Aspire dashboard with SQL Server Management Studio or Azure Data Studio.

**Q: Can I use a different database for local development?**  
A: Yes, but it's not recommended. Aspire's containerized SQL Server ensures consistency across all developers.

**Q: What happens to my existing connection strings?**  
A: Remove them from `appsettings.json` - Aspire manages connection strings automatically.

**Q: How do I deploy to Azure?**  
A: Use `azd up` (Azure Developer CLI) - Aspire generates infrastructure-as-code automatically. See Aspire deployment docs for details.

**Q: Do migrations run in production?**  
A: Yes, by default migrations run on service startup. For zero-downtime deployments, you may want to run migrations as a separate job before deploying new service versions.
