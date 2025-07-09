

# Copilot Instructions for Visage

## Project Overview
Visage is a modular, Aspire-orchestrated .NET 9 solution for managing large-scale OSS community events, with a focus on inclusiveness, privacy, and reliability. The architecture is designed for scalability, maintainability, and rapid developer onboarding.

## Architecture & Key Components
- **Frontend:** Hybrid Blazor (Web, MAUI, and Shared UI) for a single codebase across web and mobile. See `Visage.FrontEnd/`.
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
- **Run Locally:** Use Aspire for orchestration. All services must be referenced in `Visage.AppHost/Program.cs` and use `.WaitFor()` to ensure correct startup order.
- **API Testing:** Use `.http` files in each service for ad-hoc API calls.
- **Containerization:** All services are containerized for local and cloud deployment. Node services use `nodemon` for hot reload.

## Project Conventions & Patterns
- **Minimal APIs:** All endpoints in a single file per service. Use Scalar OpenAPI for documentation.
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
- **Minimal API with EF Core:** See `services/Visage.Services.Eventing/Program.cs` and `EventDB.cs`.
- **Profile API:** See `services/Visage.Services.Registrations/ProfileApi.cs` for user profile endpoints and authorization checks.

## References
- [README.md](../README.md) for architecture, testing, and deployment overview
- [Visage.AppHost/Program.cs](../Visage.AppHost/Program.cs) for orchestration patterns
- [Visage.ServiceDefaults/Extensions.cs](../Visage.ServiceDefaults/Extensions.cs) for service defaults
- [services/CloudinaryImageSigning/app.js](../services/CloudinaryImageSigning/app.js) for Node integration

---
If any section is unclear or incomplete, please provide feedback for further refinement.
