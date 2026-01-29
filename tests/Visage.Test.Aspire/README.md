# Visage.Test.Aspire - Integration & E2E Tests

## Quick Start

### Default CI-friendly run

Auth0-dependent tests are decorated with `[AuthRequired]` and will **skip automatically** when Auth0 env vars are missing.

```powershell
dotnet test --project tests\Visage.Test.Aspire\Visage.Test.Aspire.csproj
```

## Selecting tests (TUnit)

TUnit uses `--treenode-filter` (not vstest `--filter`).

Filter format: `/<Assembly>/<Namespace>/<Class>/<Test>`

```powershell
# Run only health checks

dotnet test --project tests\Visage.Test.Aspire\Visage.Test.Aspire.csproj --treenode-filter "/*/*/HealthEndpointTests/*"

# Run only DB connectivity/api smoke tests

dotnet test --project tests\Visage.Test.Aspire\Visage.Test.Aspire.csproj --treenode-filter "/*/*/RegistrationDbTests/*"

dotnet test --project tests\Visage.Test.Aspire\Visage.Test.Aspire.csproj --treenode-filter "/*/*/EventingDbTests/*"
```

### Selecting by category

Many tests use `[Category("...")]` (for example `RequiresAuth`, `AspireHealth`, `E2E`).

```powershell
# Run RequiresAuth tests

dotnet test --project tests\Visage.Test.Aspire\Visage.Test.Aspire.csproj --treenode-filter "/*/*/*/*[Category=RequiresAuth]"

# Run E2E tests

dotnet test --project tests\Visage.Test.Aspire\Visage.Test.Aspire.csproj --treenode-filter "/*/*/*/*[Category=E2E]"

# Exclude RequiresAuth + AspireHealth from a default run

dotnet test --project tests\Visage.Test.Aspire\Visage.Test.Aspire.csproj --treenode-filter "/*/*/*/*[Category!=RequiresAuth][Category!=AspireHealth]"
```

## Prerequisites

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

## Troubleshooting

### Auth tests are skipped

If you *want* to run auth tests, set the Auth0 env vars above. Otherwise, these tests should be skipped automatically.

---

# Visage E2E Tests - Security

## Security: Password Grant Restrictions

This test suite uses Auth0 Resource Owner Password Grant for automated E2E testing. To prevent security vulnerabilities:

### Security Measures in Place

1. **Email Whitelist** - `Auth0TestHelper.cs` only allows specific test emails:
   - `test.playwright@hackmum.in`
   - `e2e.test@hackmum.in`
   - `ci.test@hackmum.in`

2. **Environment Check** - Password Grant is disabled in `Production` environments

3. **Auth0 Action** - Server-side whitelist in Auth0 Login flow (see Auth0 Dashboard → Actions)

4. **Separate Test Application** - Uses dedicated Auth0 app credentials (not production)

### Best Practices

- Never commit test credentials to source control
- Never use production user emails in tests
- Never enable Password Grant on production Auth0 applications
- Always use dedicated test users with minimal privileges
- Always rotate test credentials regularly (every 90 days)

### Adding a New Test User

1. **Auth0 Dashboard** → Users → Create User
2. Email: `{purpose}.test@hackmum.in` (e.g., `ci.test@hackmum.in`)
3. Add email to `AllowedTestEmails` in `Auth0TestHelper.cs`
4. Add email to Auth0 Action whitelist
5. Set `TEST_USER_EMAIL` and `TEST_USER_PASSWORD` environment variables
