# Visage.Test.Aspire - Integration & E2E Tests

## Quick Start

### Running Tests Without Auth (Basic Tests)

```powershell
# Run all tests that don't require authentication
dotnet test --filter "Category!=RequiresAuth&Category!=E2E"
```

### Prerequisites

1. **Docker or Podman** - Required for Aspire to spin up SQL Server and other dependencies
   - Verify: `docker info`

2. **Playwright Browsers** (E2E tests only) - Install after building:
   ```powershell
   dotnet build tests\Visage.Test.Aspire
   pwsh tests\Visage.Test.Aspire\bin\Debug\net10.0\playwright.ps1 install
   ```

## Environment Variables

### Required for Auth-Dependent Tests (`Category=RequiresAuth`)

```powershell
# Auth0 Configuration
$env:AUTH0_DOMAIN = "your-tenant.auth0.com"
$env:AUTH0_CLIENT_ID = "your-client-id"
$env:AUTH0_CLIENT_SECRET = "your-client-secret"
$env:AUTH0_AUDIENCE = "https://your-api-audience"

# Test User Credentials
$env:TEST_USER_EMAIL = "test.playwright@hackmum.in"
$env:TEST_USER_PASSWORD = "your-test-password"
```

### Required for E2E Tests (`Category=E2E`)

```powershell
# Frontend base URL
$env:TEST_BASE_URL = "https://localhost:5001"
```

### Optional: External Services Mode

```powershell
# Point tests at externally running Aspire app
$env:TEST_USE_EXTERNAL_SERVICES = "true"
$env:TEST_SERVICE_FRONTENDWEB_URL = "https://localhost:7261"
$env:TEST_SERVICE_REGISTRATIONS_API_URL = "https://localhost:7159"
$env:TEST_SERVICE_EVENTING_URL = "https://localhost:7185"
```

## Test Categories

| Category | Description | Requires Auth | Requires Playwright |
|----------|-------------|---------------|---------------------|
| `RequiresAuth` | Tests that need Auth0 | ✅ | ❌ |
| `E2E` | Browser automation tests | ✅ | ✅ |
| `DraftPersistence` | Draft save/restore | ✅ | Varies |
| `Smoke` | Quick sanity checks | Varies | Varies |

### Running Specific Test Categories

```powershell
# Run only smoke tests
dotnet test --filter "Category=Smoke"

# Run tests WITHOUT auth
dotnet test --filter "Category!=RequiresAuth"

# Run draft tests
dotnet test --filter "Category=DraftPersistence"

# Run E2E tests (requires Playwright + Auth0)
dotnet test --filter "Category=E2E"
```

## Troubleshooting

### Tests Timeout Waiting for Resources

**Symptom**: `TimeoutException: The operation has timed out` for `frontendweb`, `eventing`, etc.

**Solution**: Ensure Docker/Podman is running: `docker info`

### Playwright Tests Fail with "Executable doesn't exist"

**Symptom**: `PlaywrightException: Executable doesn't exist at C:\Users\...\chrome-headless-shell.exe`

**Solution**: Install Playwright browsers:
```powershell
pwsh tests\Visage.Test.Aspire\bin\Debug\net10.0\playwright.ps1 install
```

### Auth Tests Fail with "AUTH0_DOMAIN not set"

**Symptom**: `InvalidOperationException: AUTH0_DOMAIN not set`

**Solution**: Either set Auth0 environment variables OR skip auth tests:
```powershell
dotnet test --filter "Category!=RequiresAuth"
```

---

# Visage E2E Tests - Security

## Security: Password Grant Restrictions

This test suite uses Auth0 Resource Owner Password Grant for automated E2E testing. To prevent security vulnerabilities:

### ✅ Security Measures in Place

1. **Email Whitelist** - `Auth0TestHelper.cs` only allows specific test emails:
   - `test.playwright@hackmum.in`
   - `e2e.test@hackmum.in`
   - `ci.test@hackmum.in`

2. **Environment Check** - Password Grant is disabled in `Production` environments

3. **Auth0 Action** - Server-side whitelist in Auth0 Login flow (see Auth0 Dashboard → Actions)

4. **Separate Test Application** - Uses dedicated Auth0 app credentials (not production)

### 🔒 Best Practices

- **Never** commit test credentials to source control
- **Never** use production user emails in tests
- **Never** enable Password Grant on production Auth0 applications
- **Always** use dedicated test users with minimal privileges
- **Always** rotate test credentials regularly (every 90 days)

### 🚨 If Password Grant Must Be Disabled

Switch to UI-based authentication in tests:

1. Remove token-based auth calls
2. Use Playwright to automate Auth0 login UI
3. Trade-off: Slower tests, more flaky, but no Password Grant required

### 📋 Adding a New Test User

1. **Auth0 Dashboard** → Users → Create User
2. Email: `{purpose}.test@hackmum.in` (e.g., `ci.test@hackmum.in`)
3. Add email to `AllowedTestEmails` in `Auth0TestHelper.cs`
4. Add email to Auth0 Action whitelist
5. Set `TEST_USER_EMAIL` and `TEST_USER_PASSWORD` environment variables

### 🔍 Verifying Security

Run this command to verify email whitelist enforcement:

```pwsh
# Should throw SecurityException
$env:TEST_USER_EMAIL = "unauthorized@example.com"
dotnet test --filter "FullyQualifiedName~Auth0TestHelper"
```

### 🎯 Running tests that require Auth0

Some tests require a valid Auth0 tenant or use the `Auth0TestHelper` via the resource owner password grant.
These are marked with a `Category` attribute of `RequiresAuth`. To omit them in default runs (e.g., local development or CI), use the following:

```pwsh
# Run all tests except those requiring Auth0 and health probes
dotnet test --filter "Category!=RequiresAuth&Category!=AspireHealth"
```

To run only the Auth0-dependent tests explicitly:

```pwsh
dotnet test --filter "Category=RequiresAuth"
```

When running the `RequiresAuth` tests, ensure `AUTH0_*` environment variables are set via `setup-test-env.ps1` or CI secrets.

> ⚠️ Note: When running `dotnet test` the test host is started and TUnit assembly-level hooks may start the Aspire app during discovery/test run. Even if tests are filtered out, test host initialization may still run, which creates resource startup overhead. In CI, use a dedicated job to run `RequiresAuth` tests with secrets configured and separate job for default tests.

Note: The test assembly start hook checks that Docker or Podman is available before attempting to start the Aspire app to prevent resources from being started when no container runtime is available. This reduces wasted resources in CI and local runs.

### CI Example (Azure Pipelines)

Add a separate job with only `RequiresAuth` tests to your pipeline that sets Auth0 secrets and runs after provisioning secrets:

```yaml
- job: Run_Default_Tests
   steps:
   - script: dotnet test --filter "Category!=RequiresAuth&Category!=AspireHealth"

- job: Run_Auth_Tests
   dependsOn: Run_Default_Tests
   condition: succeeded()
   variables:
      - name: AUTH0_DOMAIN
         value: $(AUTH0_DOMAIN)
      - name: AUTH0_CLIENT_ID
         value: $(AUTH0_CLIENT_ID)
      - name: AUTH0_CLIENT_SECRET
         value: $(AUTH0_CLIENT_SECRET)
   steps:
   - script: dotnet test --filter "Category=RequiresAuth"
```

### 📚 References

- [Auth0 Resource Owner Password Grant](https://auth0.com/docs/get-started/authentication-and-authorization-flow/resource-owner-password-flow)
- [Auth0 Actions](https://auth0.com/docs/customize/actions)
- [OWASP Password Grant Risks](https://owasp.org/www-community/attacks/Password_Spraying_Attack)
