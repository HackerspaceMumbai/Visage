# Performance Optimization for Aspire Integration Tests

## Problem
Integration tests were taking **1000+ seconds** because each test was starting its own Aspire app instance (SQL Server, multiple services, etc.).

## Solution
Use a **shared test fixture** (`AspireAppFixture`) that:
- Starts the Aspire app **once** before all tests
- Reuses the same app instance across all tests
- Disposes it **once** after all tests complete

## Performance Impact
- **Before**: ~1000+ seconds (33 tests × 30-40 seconds per test)
- **After**: ~60-90 seconds (one-time Aspire startup + fast test execution)
- **Speedup**: **10-15x faster** ⚡

## How to Use

### 1. Apply the fixture to your test class
```csharp
[ClassDataSource<AspireAppFixture>(Shared = SharedType.Globally)]
public class YourIntegrationTests(AspireAppFixture fixture)
{
    // Tests go here
}
```

### 2. Use fixture instead of creating new app
```csharp
// ❌ OLD WAY (slow - starts new app every test)
[Test]
public async Task Your_Test()
{
    var builder = await DistributedApplicationTestingBuilder
        .CreateAsync<Projects.Visage_AppHost>();
    await using var app = await builder.BuildAsync();
    await app.StartAsync();
    
    var httpClient = app.CreateHttpClient("service-name");
    // ... test code
}

// ✅ NEW WAY (fast - reuses shared app)
[Test]
public async Task Your_Test()
{
    // Wait for your service to be ready
    await fixture.ResourceNotificationService
        .WaitForResourceAsync("service-name", KnownResourceStates.Running)
        .WaitAsync(TimeSpan.FromSeconds(90));
    
    var httpClient = fixture.CreateHttpClient("service-name");
    // ... test code
}
```

## Files Updated
- ✅ `AspireAppFixture.cs` - Shared fixture implementation
- ✅ `RegistrationDbTests.cs` - Converted to use fixture

## Files To Update
Apply the same pattern to these files:
- `DraftSaveTests.cs`
- `DraftRetrievalTests.cs`
- `DraftDeletionTests.cs`
- `FrontEndHomeTests.cs`
- `SqlServerIntegrationTests.cs`

## Pattern Template
```csharp
using Aspire.Hosting;
using FluentAssertions;
using TUnit.Core;

namespace Visage.Test.Aspire;

[ClassDataSource<AspireAppFixture>(Shared = SharedType.Globally)]
public class YourIntegrationTests(AspireAppFixture fixture)
{
    [Test]
    public async Task Your_Test_Name()
    {
        // Wait for service
        await fixture.ResourceNotificationService
            .WaitForResourceAsync("your-service", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(90));
        
        // Create HTTP client
        var httpClient = fixture.CreateHttpClient("your-service");
        
        // Test logic
        var response = await httpClient.GetAsync("/your-endpoint");
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
```

## Notes
- E2E tests (`ProfileDraftPersistenceTests`) don't use this fixture - they expect app to be running externally
- The fixture waits for SQL Server and databases during initialization
- All integration tests share the same app state - ensure tests don't conflict
- Use unique test data (random emails, GUIDs) to avoid state conflicts between tests
