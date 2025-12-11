# Quickstart Guide: Registration Flow Optimization

**Branch**: `003-registration-flow-optimization` | **Date**: October 28, 2025
**Purpose**: Help developers set up, build, and test the registration flow optimization feature

## Prerequisites

- [x] .NET 10 SDK installed (version pinned in `global.json`)
- [x] Docker or Podman for SQL Server container
- [x] Git for version control
- [x] Visual Studio 2025 or VS Code with C# Dev Kit extension
- [x] Auth0 account with application configured (or test credentials)
- [x] pnpm for Tailwind CSS build (DaisyUI styling)

## Environment Setup

### 1. Clone and Switch to Feature Branch

```pwsh
git clone https://github.com/HackerspaceMumbai/Visage.git
cd Visage
git checkout 003-registration-flow-optimization
```

### 2. Configure User Secrets (Local Development)

Set Auth0 credentials and Cloudinary API keys:

```pwsh
# Navigate to AppHost project
cd Visage.AppHost

# Add Auth0 secrets
dotnet user-secrets set "Parameters:auth0-domain" "your-tenant.auth0.com"
dotnet user-secrets set "Parameters:auth0-clientid" "your-client-id"
dotnet user-secrets set "Parameters:auth0-clientsecret" "your-client-secret"
dotnet user-secrets set "Parameters:auth0-audience" "https://visage-api"

# Add Cloudinary secrets
dotnet user-secrets set "Parameters:cloudinary-cloudname" "your-cloud-name"
dotnet user-secrets set "Parameters:cloudinary-apikey" "your-api-key"
dotnet user-secrets set "Parameters:cloudinary-apisecret" "your-api-secret"

# Add Clarity project ID (optional)
dotnet user-secrets set "Parameters:clarity-projectid" "your-project-id"

cd ..
```

### 3. Build Tailwind CSS (DaisyUI Styling)

```pwsh
# Install pnpm globally if not already installed
npm install -g pnpm

# Navigate to shared frontend project
cd Visage.FrontEnd\Visage.FrontEnd.Shared

# Install dependencies
pnpm install

# Build Tailwind CSS (compiles input.css → output.css)
pnpm run buildcss

cd ..\..
```

### 4. Start Aspire AppHost

```pwsh
# From repository root
cd Visage.AppHost

# Run Aspire (starts all services)
dotnet run

# Or use Aspire CLI (if installed)
aspire run
```

**Expected Output**:

- Aspire Dashboard: `http://localhost:15888` (check this URL in browser)
- SQL Server: Started in container, migrations applied automatically
- Registrations API: `https://registrations-api` (service discovery)
- Frontend Web: `https://frontendweb` (service discovery)
- Cloudinary Signing Service: `http://localhost:XXXX` (check Aspire dashboard for port)

### 5. Apply Database Migrations

**Important**: Use `aspire exec` to run EF Core commands in the context of Aspire resources.

```pwsh
# Enable aspire exec feature (one-time setup)
aspire config set features.execCommandEnabled true

# Drop database (if you want a clean start)
aspire exec --resource registrations -- dotnet ef database drop --force

# Add migration for profile completion tracking
aspire exec --resource registrations -- dotnet ef migrations add AddProfileCompletionTracking

# Update database
aspire exec --resource registrations -- dotnet ef database update
```

**Verify Migration**:

- Open Aspire Dashboard → Resources → SQL Server → Connection String
- Connect to SQL Server using SQL Server Management Studio or Azure Data Studio
- Verify tables: `UserProfiles`, `DraftRegistrations`, `UserPreferences`

## Development Workflow

### Running the Application

**Option 1: Visual Studio 2025**

1. Open `Visage.sln` in Visual Studio
2. Set `Visage.AppHost` as the startup project
3. Press `F5` to run in debug mode
4. Aspire Dashboard opens automatically: `http://localhost:15888`

**Option 2: VS Code**

1. Open repository root in VS Code
2. Run task: `Terminal → Run Task → aspire: run`
3. Open Aspire Dashboard: `http://localhost:15888`

**Option 3: Command Line**

```pwsh
cd Visage.AppHost
dotnet run
```

### Testing the Feature

#### Test Scenario 1: New User Registration

1. Open frontend: Check Aspire Dashboard for `frontendweb` URL
2. Click "Sign In" (redirects to Auth0)
3. Create new Auth0 account or use test credentials
4. After authentication, verify redirect to `MandatoryRegistration.razor`
5. Fill mandatory fields (FirstName, LastName, MobileNumber, GovtId, etc.)
6. Select OccupationStatus (Student or Professional)
7. Fill occupation-specific fields (e.g., EducationalInstituteName for Student)
8. Submit form
9. Verify redirect to Home page with success message encouraging AIDE completion

#### Test Scenario 2: Draft Save

1. Start filling mandatory registration (new user)
2. Fill 3-4 fields (FirstName, LastName, Email)
3. Wait 30 seconds for auto-save (check browser console for "Draft saved" log)
4. Close browser tab
5. Re-open application, authenticate
6. Verify form is pre-filled with saved draft data

#### Test Scenario 3: Returning User with Complete Profile

1. Authenticate with existing user (profile completed)
2. Verify redirect to Home page (NOT registration page)
3. Verify AIDE completion banner shows (if AIDE profile incomplete)
4. Click "Dismiss" on banner
5. Refresh page → Verify banner does not show for 30 days

#### Test Scenario 4: Profile Completion API

Use `.http` file to test API directly:

```pwsh
# Open in VS Code with REST Client extension
code Visage.Services.Registrations/app.http
```

**Example API Requests**:

```http
### Get profile completion status
GET https://registrations-api/api/profile/completion-status?userId=auth0|123456789
Authorization: Bearer {{$auth0Token}}

### Save draft registration
POST https://registrations-api/api/profile/draft
Authorization: Bearer {{$auth0Token}}
Content-Type: application/json

{
  "userId": "auth0|123456789",
  "draftData": {
    "firstName": "Jane",
    "lastName": "Doe",
    "email": "jane.doe@example.com",
    "occupationStatus": "Student",
    "completionPercentage": 30
  }
}

### Dismiss AIDE banner
POST https://registrations-api/api/profile/preferences/aide-banner
Authorization: Bearer {{$auth0Token}}
Content-Type: application/json

{
  "userId": "auth0|123456789"
}
```

### Running Tests

#### Integration Tests

```pwsh
# Run all integration tests
dotnet test tests/Visage.Tests.Integration/

# Run specific test class
dotnet test tests/Visage.Tests.Integration/ --filter FullyQualifiedName~RegistrationFlowTests

# Run with verbose output
dotnet test tests/Visage.Tests.Integration/ --logger "console;verbosity=detailed"
```

**Expected Tests**:

- `ProfileCompletionApi_NewUser_ReturnsIncompleteStatus`
- `ProfileCompletionApi_ExistingUser_ReturnsCompleteStatus`
- `DraftRegistration_AutoSaves_AndRestores`
- `AideBannerDismissal_StoresPreference_AndSuppresses30Days`

#### E2E Tests (Playwright)

```pwsh
# Install Playwright browsers (one-time setup)
pwsh ./scripts/run-playwright.ps1 install

# Run E2E tests
pwsh ./scripts/run-playwright.ps1

# Run specific E2E test
dotnet test tests/Visage.E2E.Playwright/ --filter "NewUserRegistrationFlow"
```

**Expected E2E Tests**:

- `NewUserRegistrationFlow_CompleteMandatoryFields_RedirectsToHome`
- `ReturningUser_WithCompleteProfile_BypassesRegistration`
- `DraftSave_RecoversPartialProgress_OnReturn`
- `AideBanner_ShowsOnHome_AndDismisses`

## Troubleshooting

### Issue: Aspire run fails with "Missing closing '}' in statement block"

**Solution**: This was a bug in `.specify/scripts/powershell/common.ps1` (now fixed). Update to latest branch:

```pwsh
git pull origin 003-registration-flow-optimization
```

### Issue: SQL Server container fails to start

**Solution**: Check Docker/Podman is running:

```pwsh
docker ps  # or podman ps
```

If Docker Desktop is not running, start it and retry `aspire run`.

### Issue: Tailwind CSS styles not applied (DaisyUI components look unstyled)

**Solution**: Rebuild Tailwind CSS output:

```pwsh
cd Visage.FrontEnd\Visage.FrontEnd.Shared
pnpm run buildcss
cd ..\..
```

Verify `Visage.FrontEnd/Visage.FrontEnd.Shared/wwwroot/output.css` exists and is up-to-date.

### Issue: Auth0 authentication fails with "Invalid redirect URI"

**Solution**: Add your local development URL to Auth0 application settings:

1. Go to Auth0 Dashboard → Applications → Your App → Settings
2. Add to "Allowed Callback URLs": `https://localhost:7XXX/callback` (check Aspire Dashboard for actual port)
3. Add to "Allowed Logout URLs": `https://localhost:7XXX`
4. Save changes

### Issue: Profile completion API returns 401 Unauthorized

**Solution**: Ensure JWT token includes `profile:read-write` scope:

1. Check Auth0 API settings: `https://manage.auth0.com/dashboard/` → APIs → Visage API
2. Verify "RBAC Settings" → Enable "Add Permissions in the Access Token"
3. Re-authenticate to get fresh token with correct scopes

### Issue: EF Core migrations fail with "Could not find project"

**Solution**: Use `aspire exec` instead of direct `dotnet ef` commands:

```pwsh
# Wrong (fails in Aspire context)
dotnet ef migrations add MyMigration --project Visage.Services.Registrations

# Correct (Aspire context with environment variables)
aspire exec --resource registrations -- dotnet ef migrations add MyMigration
```

## Next Steps

- Read [plan.md](./plan.md) for full implementation plan
- Read [data-model.md](./data-model.md) for database schema details
- Read [contracts/profile-completion-api.yaml](./contracts/profile-completion-api.yaml) for API contract
- Read [research.md](./research.md) for best practices research (once Phase 0 complete)
- Implement tasks from [tasks.md](./tasks.md) (will be generated via `/speckit.tasks` command)

## Useful Commands

```pwsh
# Aspire CLI
aspire run                      # Start all services
aspire exec --resource <name>   # Execute command in resource context
aspire config                   # View/set Aspire configuration
aspire update                   # Update Aspire packages

# EF Core (via Aspire)
aspire exec --resource registrations -- dotnet ef migrations add <Name>
aspire exec --resource registrations -- dotnet ef database update
aspire exec --resource registrations -- dotnet ef migrations list
aspire exec --resource registrations -- dotnet ef database drop --force

# Build & Test
dotnet build                    # Build solution
dotnet test                     # Run all tests
dotnet test --filter <Filter>   # Run specific tests

# Tailwind CSS (DaisyUI)
cd Visage.FrontEnd/Visage.FrontEnd.Shared
pnpm install                    # Install dependencies
pnpm run buildcss               # Compile input.css → output.css
pnpm run watchcss               # Watch mode for dev
```

## Resources

- [Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Blazor Hybrid Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/)
- [DaisyUI Documentation](https://daisyui.com/)
- [Auth0 .NET SDK](https://auth0.com/docs/quickstart/webapp/aspnet-core)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [TUnit Testing](https://github.com/thomhurst/TUnit)
- [Playwright .NET](https://playwright.dev/dotnet/)
