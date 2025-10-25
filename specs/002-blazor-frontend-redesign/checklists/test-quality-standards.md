# Test Quality Standards for Visage

## Purpose
This document defines quality standards for integration and E2E tests to prevent regressions and catch configuration issues early.

## Root Cause Analysis: Service Discovery Bug
**Date**: October 21, 2025  
**Issue**: AppHost registered Eventing service as `"event-api"` instead of `"eventing"`, breaking Aspire service discovery  
**Impact**: Frontend DNS errors ("No such host is known (eventing:443)")  
**Why Tests Didn't Catch It**: 
1. Tests only waited for frontend, not backend services
2. Tests used test infrastructure's `CreateHttpClient` instead of exercising frontend's Program.cs code path
3. No network monitoring or assertions on API call success
4. No validation that service names match between AppHost and client code

## Mandatory Test Requirements

### 1. Service Health Validation
**Rule**: ALL tests must wait for ALL services the component depends on

```csharp
// ❌ BAD - Only waits for frontend
await resourceNotificationService.WaitForResourceAsync("frontendweb", KnownResourceStates.Running);

// ✅ GOOD - Waits for all dependencies
await resourceNotificationService.WaitForResourceAsync("eventing", KnownResourceStates.Running);
await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running);
await resourceNotificationService.WaitForResourceAsync("frontendweb", KnownResourceStates.Running);
```

### 2. Service Discovery Validation
**Rule**: At least ONE test must explicitly validate service discovery configuration

```csharp
[Test]
public async Task AspireServiceDiscoveryExposesRequiredBackendServices()
{
    // Create HttpClients using expected service names
    var client = app.CreateHttpClient("eventing");
    client.Should().NotBeNull();
    client.BaseAddress.Should().NotBeNull();
    
    // Validate health endpoint is reachable
    var health = await client.GetAsync("/health");
    health.Should().HaveStatusCode(HttpStatusCode.OK);
}
```

### 3. Network Monitoring in Playwright Tests
**Rule**: E2E tests must monitor network requests and assert no failures

```csharp
// Monitor failed API calls
page.Response += (_, response) =>
{
    if (response.Url.Contains("/events") && !response.Ok)
    {
        apiCallFailed = true;
        apiErrorMessage = $"API call failed: {response.Status}";
    }
};

// Monitor JS/network errors
page.PageError += (_, error) =>
{
    if (error.Contains("eventing") || error.Contains("No such host"))
    {
        apiCallFailed = true;
        apiErrorMessage = $"DNS/Network error: {error}";
    }
};

// Assert no errors occurred
apiCallFailed.Should().BeFalse($"Frontend should call services successfully, but got: {apiErrorMessage}");
```

### 4. Integration Test Coverage
**Rule**: Tests must cover the actual code path used by the application

```csharp
// ❌ BAD - Bypasses frontend's HttpClient configuration
var apiClient = app.CreateHttpClient("eventing");
await apiClient.PostAsJsonAsync("/events", event);

// ✅ GOOD - Exercises frontend's actual IEventService implementation
var frontendClient = app.CreateHttpClient("frontendweb");
await frontendClient.GetAsync("/"); // Frontend internally calls IEventService
```

### 5. Explicit Success Assertions
**Rule**: Don't just check UI rendering; assert on data flow

```csharp
// ❌ BAD - Only checks UI elements exist
await page.GetByText("Event Title").IsVisibleAsync();

// ✅ GOOD - Verifies data loaded successfully from API
await page.GetByText("Event Title").IsVisibleAsync();
apiCallFailed.Should().BeFalse(); // No API errors
var response = await apiClient.GetFromJsonAsync<Event[]>("/events");
response.Should().ContainSingle(e => e.Title == "Event Title"); // Data matches
```

### 6. Blazor Rendering Exception Detection
**Rule**: Playwright tests must capture console errors and check for error UI

**Why**: Blazor rendering exceptions (like `Guid.Parse` failures) don't always trigger `PageError` events. They may be caught by Blazor's error boundary and displayed in the UI or logged to console.

```csharp
// ❌ BAD - Doesn't check for rendering exceptions
await page.GotoAsync(baseUrl);
await page.GetByText("Event Title").IsVisibleAsync();

// ✅ GOOD - Monitors console errors and error UI
var consoleErrors = new List<string>();
page.Console += (_, msg) =>
{
    if (msg.Type == "error")
    {
        consoleErrors.Add(msg.Text);
    }
};

await page.GotoAsync(baseUrl);

// Check for Blazor error UI
var hasErrorUI = await page.Locator("text=/Unable to load events/i").IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 })
    || await page.Locator(".alert-error").IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 });
hasErrorUI.Should().BeFalse("Page should not display error UI");

await page.GetByText("Event Title").IsVisibleAsync();

// Assert no console errors (includes Blazor exceptions)
consoleErrors.Should().BeEmpty($"Page should not have console errors, but got: {string.Join(", ", consoleErrors)}");
```

**Common Blazor Exceptions to Catch**:
- `System.FormatException`: Invalid data format (e.g., Guid parsing)
- `System.NullReferenceException`: Missing required data
- `System.InvalidOperationException`: State errors
- `System.Net.Http.HttpRequestException`: API call failures

## Test Types and Responsibilities

### Unit Tests (bunit)
- **Scope**: Individual Blazor components in isolation
- **Required**: Mock IEventService, IRegistrationService
- **Validates**: Component rendering logic, event handling, state management

### Integration Tests (Aspire Testing)
- **Scope**: Multi-service interactions via Aspire orchestration
- **Required**: 
  - Wait for all service dependencies
  - Validate service discovery works
  - Assert on API responses, not just status codes
- **Validates**: Service-to-service communication, configuration, dependency injection

### E2E Tests (Playwright)
- **Scope**: Full user workflows in real browser
- **Required**: 
  - Monitor network requests
  - Assert no console/network errors
  - Validate data flows end-to-end
- **Validates**: Complete user scenarios, visual rendering, accessibility

## Test Review Checklist

Before merging tests, confirm:

- [ ] All service dependencies are explicitly awaited
- [ ] Service discovery is validated (HttpClient creation + health check)
- [ ] Network monitoring is enabled (Playwright tests)
- [ ] Console error monitoring is enabled (Playwright tests)
- [ ] Blazor error UI is checked (alert-error, error messages)
- [ ] Explicit assertions on API call success/failure
- [ ] Tests exercise actual application code paths, not test shortcuts
- [ ] Error messages are descriptive and include context
- [ ] Tests are deterministic (no race conditions or flaky waits)
- [ ] Test names follow convention: `MethodName_Condition_ExpectedResult`

## Continuous Improvement

When a bug escapes to production/manual testing:
1. Root cause analysis: Why didn't tests catch it?
2. Update this document with new standards
3. Add regression test following new standards
4. Review similar tests for same gaps

## References
- [FrontEndHomeTests.cs](../../tests/Visage.Test.Aspire/FrontEndHomeTests.cs) - Reference implementation
- [Aspire Testing Docs](https://learn.microsoft.com/en-us/dotnet/aspire/testing)
- [Playwright Best Practices](https://playwright.dev/dotnet/docs/best-practices)
