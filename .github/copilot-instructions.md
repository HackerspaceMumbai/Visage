

# Copilot Instructions for Visage

## Project Overview
Visage is a modular, Aspire-orchestrated .NET 9 solution for managing large-scale OSS community events, with a focus on inclusiveness, privacy, and reliability. The architecture is designed for scalability, maintainability, and rapid developer onboarding.

## Architecture & Key Components
- **Frontend:** Hybrid Blazor (Web, MAUI, and Shared UI) for a single codebase across web and mobile. See `Visage.FrontEnd/`.
- **Blazor Render Modes:** The app uses InteractiveServer as the default app-wide render mode (set in `App.razor`). This ensures consistent behavior for Auth0 authentication and Aspire service discovery. Avoid per-page render mode overrides unless explicitly justified.
- **Backend:** Minimal APIs per service (see `services/`), each with its own `.http` file for ad-hoc testing. All APIs use EF Core and [StrictId](https://www.nuget.org/packages/StrictId) for data access and identity.
- **AppHost:** `Visage.AppHost` orchestrates all services using .NET Aspire. All services must be registered as project resources and use health checks, OpenTelemetry, and Scalar OpenAPI for documentation.
- **Cloudinary Node Service:** `services/CloudinaryImageSigning` provides secure image upload signatures, containerized and integrated via Aspire.
- **Data Models:** Shared via `Visage.Shared/Models/` and `Visage.FrontEnd.Shared/Models/`. Use StrictId for all entity IDs.
- **Authentication:** Auth0 is used for user authentication and authorization. All profile endpoints require `profile:read-write` scope.


## Developer Workflows
- **Build:** Use `dotnet build` or the provided Azure Pipelines config (`azure-pipelines.yml`).
- **Test:**
  - Integration: `dotnet test tests/Visage.Tests.Integration/Visage.Tests.Integration.csproj`
  - E2E: Playwright (see `tests/`). Example: `pwsh ./scripts/run-playwright.ps1`
  - Load: NBomber. Example: `dotnet nbomber run --config ./tests/NBomberConfig.json`
  - Security: OWASP ZAP
  - Mutation: Stryker
  - Unit: Only for critical Blazor components (bunit)
- **Run Locally:** Use Aspire for orchestration. All services must be referenced in `Visage.AppHost/AppHost.cs` and use `.WaitFor()` to ensure correct startup order.- **Aspire Commands (9.5+):** 
  - `aspire exec --resource <resource-name> -- <command>`: Execute commands with automatic environment variable injection (connection strings, service discovery URIs)
    - Enable with: `aspire config set features.execCommandEnabled true`
    - Use `--workdir` flag to specify working directory: `aspire exec --resource <name> --workdir /app/tools -- <command>`
    - Use `--start-resource` to wait for resource to be running before executing
  - `aspire update` (preview): Automatically update Aspire packages and templates across your solution
  - `aspire config set <key> <value>`: Manage feature flags and CLI settings
  - **Failure Protocol**: If `aspire run` fails repeatedly (2+ consecutive failures), STOP immediately and report the error details to the user. Do not continue silent debugging cycles that waste time.

### Styling and DaisyUI (project-specific guidance)

- **Canonical input**: The single source of truth for Tailwind + DaisyUI configuration is `Visage.FrontEnd/Visage.FrontEnd.Shared/Styles/input.css`. Edit theme values and plugin configuration there.
- **Component-scoped CSS**: Use Blazor component-scoped CSS files (`Component.razor.css`) placed beside the `.razor` file for component-specific styles (focus outlines, small layout/visual tweaks). Avoid global overrides unless absolutely necessary.
- **Migration**: If you see standalone theme files (for example `wwwroot/css/daisy-theme.css`), migrate their variables into `input.css` and then remove their `<link>` from `App.razor` to avoid duplication.
- **Rebuild stylesheet**: Run the Tailwind CLI before building or testing the app to regenerate the compiled stylesheet. Example (PowerShell):

```pwsh
pnpm --prefix Visage.FrontEnd/Visage.FrontEnd.Shared install
pnpm --prefix Visage.FrontEnd/Visage.FrontEnd.Shared run buildcss
```

Add this to your CI pipeline so `output.css` is regenerated during CI builds.

- **API Testing:** Use `.http` files in each service for ad-hoc API calls.
- **EF Core Migrations:** Always use `aspire exec` to run EF Core commands in the context of Aspire resources:
  ```pwsh
  # Enable aspire exec feature (one-time setup)
  aspire config set features.execCommandEnabled true
  
  # Drop database
  aspire exec --resource eventing -- dotnet ef database drop --force
  
  # Add migration
  aspire exec --resource eventing -- dotnet ef migrations add MigrationName
  
  # Update database
  aspire exec --resource eventing -- dotnet ef database update
  
  # Use --workdir flag for commands in specific directories (Aspire 9.5+)
  aspire exec --resource eventing --workdir /app/migrations -- dotnet ef migrations script
  
  # Wait for resource to start before executing (Aspire 9.5+)
  aspire exec --start-resource eventing -- dotnet ef database update
  ```
  This ensures connection strings and other environment variables are correctly injected from the Aspire app model.

### Aspire 9.5 Features
Visage uses .NET Aspire 9.5+ which includes several enhancements relevant to our development workflow:

- **Enhanced `aspire exec`**: 
  - `--workdir` (`-w`) flag for running commands in specific directories
  - `--start-resource` waits for resource to be running before executing
  - Improved error messages and fail-fast argument validation
  - Better help text for developer experience

- **`aspire update` (preview)**: Automatically detect and update outdated Aspire packages and templates
  ```pwsh
  # Update all Aspire packages to latest compatible versions
  aspire update
  ```

- **HTTP Health Probes**: Configure startup, readiness, and liveness probes for resources
  ```csharp
  var api = builder.AddProject<Projects.Api>("api")
      .WithHttpProbe(ProbeType.Readiness, "/health/ready");
  ```

- **Resource Lifecycle Events**: Register callbacks for resource stopped events
  ```csharp
  var api = builder.AddProject<Projects.Api>("api")
      .OnResourceStopped(async (resource, stoppedEvent, cancellationToken) =>
      {
          await ResetSystemState();
      });
  ```

- **Enhanced Wait Patterns**: 
  - `WaitFor`: Waits for dependency to be Running AND pass all health checks
  - `WaitForStart`: Waits only for dependency to reach Running (ignores health checks)
  - `WaitForCompletion`: Waits for dependency to reach terminal state

For complete Aspire 9.5 features, see [What's new in Aspire 9.5](https://learn.microsoft.com/en-us/dotnet/aspire/whats-new/dotnet-aspire-9.5)
- **Containerization:** All services are containerized for local and cloud deployment. Node services use `nodemon` for hot reload.

## Project Conventions & Patterns
- **Minimal APIs:** All endpoints in a single file per service. Use Scalar OpenAPI for documentation.
- **Blazor Render Modes:** 
  - **Default Strategy:** InteractiveServer app-wide (set in `App.razor` on `<Routes>` component) for consistent authentication and service discovery
  - **When to Override:** Only override per-page if you need InteractiveWebAssembly for specific client-side features that don't require server data or auth
  - **Navigation Issue:** Mixing render modes (e.g., InteractiveAuto → InteractiveServer) can break Blazor's SPA navigation. Prefer consistent app-wide strategy.
  - **Auth Requirement:** InteractiveServer is required for Auth0 server-side cookie authentication and Aspire service discovery URIs
- **Form Validation:**
  - Use `EditForm` with both `DataAnnotationsValidator` and custom business rule validation via `ValidationMessageStore`
  - Include `<ValidationMessage For="@(() => model.Property)" />` for inline field-specific error messages
  - Use `OnValidSubmit` for successful submissions and `OnInvalidSubmit` for user feedback
  - Wrap submit handlers in try/catch with `isSubmitting` state to provide visual feedback during async operations
- **DateTime Comparisons:**
  - Use full `DateTime` comparisons for event status classification, not date-only comparisons
  - Example: `var eventDateTime = evt.StartDate.ToDateTime(evt.StartTime); Status = eventDateTime > DateTime.Now ? EventStatus.Upcoming : EventStatus.Completed`
  - This prevents same-day future events from being incorrectly marked as Completed
- **UI Consistency:**
  - Maintain consistent visual styling across component states (e.g., use buttons for both active and disabled states, not mixing badges and buttons)
  - Use DaisyUI classes consistently for alignment and visual hierarchy
- **EF Core:** Use Repository and Unit of Work patterns. Configure StrictId in `OnModelCreating`.
- **Service Defaults:** All services must call `AddServiceDefaults()` for health checks, OpenTelemetry, and service discovery.
- **Commit Messages:** Imperative, sentence case, no trailing dot. All commits must be signed off.
- **Variable Naming:** Use descriptive names with auxiliary verbs (e.g., `IsLoading`, `HasError`).
- **Splitting:** Split files/functions when they grow too large. Do not refactor unrelated code.
- **Testing:** All new code must be covered by integration or E2E tests. Document new tests in the `tests/` directory.
- **Configuration:** Use environment variables for secrets and service URLs. See `appsettings.json` and Aspire parameters.

## Integration Points
- **Cloudinary:** Secure image upload via `CloudinaryImageSigning` Node service. Use `/sign-upload` endpoint for signatures.
- **Auth0:** All user profile endpoints require valid JWT with `profile:read-write` scope.
- **OpenTelemetry:** All services must emit traces and metrics. See `Visage.ServiceDefaults/Extensions.cs`.
- **Scalar OpenAPI:** All APIs must expose OpenAPI docs at `/scalar/v1` in development.

## Examples
- **Registering a new service in Aspire:** See `Visage.AppHost/Program.cs` for `.AddProject<>()` and `.WaitFor()` usage.
- **Blazor Render Mode Setup:** See `Visage.FrontEnd/Visage.FrontEnd.Web/Components/App.razor` for app-wide InteractiveServer configuration
- **Form Validation Pattern:** See `Visage.FrontEnd/Visage.FrontEnd.Shared/Pages/ScheduleEvent.razor` for EditForm with ValidationMessageStore and inline ValidationMessage components
- **Event Status Classification:** See `Visage.FrontEnd/Visage.FrontEnd.Shared/Pages/Home.razor` for DateTime-based status logic in MapToViewModels
- **Minimal API with EF Core:** See `services/Visage.Services.Eventing/Program.cs` and `EventDB.cs`.
- **Profile API:** See `services/Visage.Services.Registrations/ProfileApi.cs` for user profile endpoints and authorization checks.

## References
- [README.md](../README.md) for architecture, testing, and deployment overview
- [Visage.AppHost/AppHost.cs](../Visage.AppHost/AppHost.cs) for orchestration patterns
- [Visage.ServiceDefaults/Extensions.cs](../Visage.ServiceDefaults/Extensions.cs) for service defaults
- [services/CloudinaryImageSigning/app.js](../services/CloudinaryImageSigning/app.js) for Node integration

---
If any section is unclear or incomplete, please provide feedback for further refinement.
