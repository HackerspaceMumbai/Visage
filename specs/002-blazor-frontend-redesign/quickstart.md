# Developer Quickstart Guide: Blazor Frontend with Events Display

**Feature**: 002-blazor-frontend-redesign  
**Purpose**: Get developers up and running with the new event display functionality  
**Estimated Setup Time**: 15-20 minutes

---

## Prerequisites

### Required Software

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 10.0.100-rc.2+ | C# compilation and runtime |
| Visual Studio 2022 | 17.13+ OR VS Code | IDE with Blazor/Aspire support |
| Docker Desktop | 4.20+ OR Podman | Container runtime for services |
| PowerShell | 7.0+ | Script execution |
| Node.js | 20+ LTS | (Optional) Tailwind CSS build tools |

### Verify Installation

\\\pwsh
# Check .NET version
dotnet --version
# Expected: 10.0.100-rc.2.25502.107 or newer

# Check Docker
docker --version
# Expected: Docker version 24.0+ or Podman equivalent

# Check PowerShell
\System.Management.Automation.PSVersionHashTable.PSVersion
# Expected: 7.0+
\\\

---

## Step 1: Clone and Build

\\\pwsh
# Clone repository (if not already done)
git clone https://github.com/HackerspaceMumbai/Visage.git
cd Visage

# Switch to feature branch
git checkout 002-blazor-frontend-redesign

# Restore dependencies
dotnet restore

# Build solution (web projects only to avoid MAUI platform errors)
dotnet build Visage.FrontEnd/Visage.FrontEnd.Web/Visage.FrontEnd.Web.csproj
dotnet build Visage.FrontEnd/Visage.FrontEnd.Shared/Visage.FrontEnd.Shared.csproj
\\\

---

## Step 2: Configure Environment

### Option A: Use Aspire Orchestration (Recommended)

\\\pwsh
# Start Aspire AppHost (starts all services including Eventing API)
cd Visage.AppHost
dotnet run
\\\

**Aspire Dashboard**: Opens automatically at \https://localhost:15888\ (or similar port)

**Service URLs** (discovered automatically):
- Frontend Web: \https://localhost:7XXX\ (shown in dashboard)
- Eventing API: \https://eventing\ (service discovery)
- Cloudinary Signing: \https://cloudinary-signing\

### Option B: Run Services Individually (Manual)

\\\pwsh
# Terminal 1: Start Eventing API
cd services/Visage.Services.Eventing
dotnet run

# Terminal 2: Start Frontend Web
cd Visage.FrontEnd/Visage.FrontEnd.Web
dotnet run

# Terminal 3: (Optional) Start Cloudinary signing service
cd services/CloudinaryImageSigning
npm install
npm start
\\\

---

## Step 3: Configure DaisyUI and Tailwind CSS

### Development Setup (CDN - Fast)

**File**: \Visage.FrontEnd.Web/Components/App.razor\

Add to \<head>\ section:
\\\html
<!-- DaisyUI + Tailwind CSS via CDN (development only) -->
<link href="https://cdn.jsdelivr.net/npm/daisyui@5/dist/full.css" rel="stylesheet" />
<script src="https://cdn.tailwindcss.com"></script>
<script>
  tailwind.config = {
    theme: {
      extend: {
        colors: {
          'hackmum-primary': '#FFC107',
          'hackmum-secondary': '#4DB6AC',
          'hackmum-accent': '#7986CB',
          'hackmum-dark': '#1A1A1A'
        }
      }
    }
  }
</script>
\\\

### Production Setup (Build-time - Optimized)

Coming in Phase 2 implementation - requires Tailwind CLI integration.

---

## Step 4: Seed Test Data (Optional)

\\\pwsh
# Run database migrations and seed events
cd services/Visage.Services.Eventing
dotnet ef database update

# Seed test events
dotnet run --seed-data
\\\

**Test Data**: Creates 10 upcoming events and 50 past events for testing.

---

## Step 5: Run Integration Tests

\\\pwsh
# Run all integration tests
dotnet test tests/Visage.Test.Aspire/Visage.Test.Aspire.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~EventApiIntegrationTests"

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"
\\\

**Expected Results**: All tests should pass. If any fail, check Aspire dashboard for service health.

---

## Step 6: Verify Frontend

### Access the Application

1. **Open Browser**: Navigate to \https://localhost:<port>\ (shown in Aspire dashboard or terminal output)
2. **Homepage**: Should display:
   - "Upcoming Events" section with event cards in responsive grid
   - "Past Events" section with virtualized list
   - Hackerspace Mumbai color palette styling
3. **Test Responsive Design**: Resize browser or use DevTools device toolbar
   - Mobile (< 768px): 1 column
   - Tablet (768-1024px): 2 columns
   - Desktop (> 1024px): 3 columns

### Verify Render Modes

**Chrome DevTools**:
1. Open DevTools (F12)
2. Go to **Network** tab
3. Reload page
4. Verify:
   - Initial HTML includes event cards (Static SSR working)
   - \lazor.web.js\ loads successfully (InteractiveAuto enabled)
   - No excessive API calls (caching working)

---

## Step 7: Run Playwright E2E Tests (Optional)

\\\pwsh
# Install Playwright browsers (first time only)
pwsh ./scripts/run-playwright.ps1 --install

# Run E2E tests
pwsh ./scripts/run-playwright.ps1
\\\

**Expected**: Tests verify homepage displays events, responsive layout works, and event details pages load.

---

## Troubleshooting

### Issue: "blazor.web.js" 404 Error

**Symptom**: Browser console shows 404 for \/_framework/blazor.web.js\

**Solution**:
\\\pwsh
# Stop all running processes
Get-Process -Name dotnet | Where-Object {.Path -like "*Visage*"} | Stop-Process -Force

# Clean and rebuild
dotnet clean
dotnet build Visage.FrontEnd/Visage.FrontEnd.Web/Visage.FrontEnd.Web.csproj

# Verify Web.Client project builds correctly
dotnet build Visage.FrontEnd/Visage.FrontEnd.Web.Client/Visage.FrontEnd.Web.Client.csproj

# Restart Aspire AppHost
cd Visage.AppHost
dotnet run
\\\

### Issue: Eventing API Returns Empty Results

**Symptom**: Homepage shows "No events available" even after seeding data

**Solution**:
\\\pwsh
# Check Eventing API health
curl https://localhost:<eventing-port>/health

# Verify database connection
cd services/Visage.Services.Eventing
dotnet ef database update --verbose

# Check for seed data
dotnet run --seed-data --verify
\\\

### Issue: DaisyUI Styles Not Loading

**Symptom**: Event cards appear unstyled or broken

**Solution**:
1. Verify CDN links in \App.razor\ are correct and accessible
2. Check browser console for CSP (Content Security Policy) errors
3. Hard refresh browser (Ctrl+Shift+R) to clear cached CSS

### Issue: Aspire Dashboard Won't Start

**Symptom**: \dotnet run\ in AppHost fails with port binding error

**Solution**:
\\\pwsh
# Find process using Aspire ports
netstat -ano | findstr "15888"
netstat -ano | findstr "18888"

# Kill process by PID
Stop-Process -Id <PID> -Force

# Restart AppHost
cd Visage.AppHost
dotnet run
\\\

---

## Development Workflow

### 1. Create a New Event Component

\\\pwsh
# Create component file
New-Item -Path "Visage.FrontEnd/Visage.FrontEnd.Shared/Components/Events/MyComponent.razor" -ItemType File

# Add component code with render mode
@rendermode InteractiveAuto

<div class="my-component">
    <!-- Your content here -->
</div>

@code {
    // Component logic
}
\\\

### 2. Test Component Locally

\\\pwsh
# Hot reload enabled - just save file and browser auto-refreshes
# Watch for build errors in terminal

# Manually restart if needed
# Ctrl+C in AppHost terminal, then dotnet run again
\\\

### 3. Add Integration Test

**File**: \	ests/Visage.Test.Aspire/MyComponentTests.cs\

\\\csharp
using TUnit.Assertions;
using TUnit.Core;

public class MyComponentTests
{
    [Test]
    public async Task MyComponent_Renders_Successfully()
    {
        // Arrange
        using var ctx = new TestContext();
        
        // Act
        var cut = ctx.RenderComponent<MyComponent>();
        
        // Assert
        cut.Should().NotBeNull();
        cut.Find(".my-component").Should().NotBeNull();
    }
}
\\\

### 4. Run Tests Before Committing

\\\pwsh
# Run all tests
dotnet test

# Format code
dotnet format

# Commit with sign-off
git add .
git commit -s -m "feat: Add MyComponent with tests"
git push origin 002-blazor-frontend-redesign
\\\

---

## Useful Commands

\\\pwsh
# View Aspire resource logs
dotnet run --project Visage.AppHost -- --logs

# Watch frontend for changes
dotnet watch --project Visage.FrontEnd/Visage.FrontEnd.Web

# Run performance profiling
dotnet run --project Visage.AppHost --configuration Release

# Generate code coverage report
dotnet test --collect:"XPlat Code Coverage"

# List all running Visage processes
Get-Process -Name dotnet | Where-Object {.Path -like "*Visage*"}
\\\

---

## Next Steps

1. **Review Specification**: Read \specs/002-blazor-frontend-redesign/spec.md\ for feature requirements
2. **Review Data Model**: Check \data-model.md\ for view models and validation rules
3. **Review API Contracts**: Check \contracts/api-contracts.md\ for endpoint specifications
4. **Review Research**: Read \esearch.md\ for technical decisions and patterns
5. **Start Implementation**: Wait for Phase 2 task breakdown via \/speckit.tasks\ command

---

## Getting Help

- **Constitution**: See \.specify/memory/constitution.md\ for project principles
- **GitHub Issues**: Report bugs or request features at https://github.com/HackerspaceMumbai/Visage/issues
- **Aspire Docs**: https://learn.microsoft.com/en-us/dotnet/aspire/
- **Blazor Docs**: https://learn.microsoft.com/en-us/aspnet/core/blazor/
- **DaisyUI Docs**: https://daisyui.com/docs/

**Developer Guide Status**: ✅ **Complete**
