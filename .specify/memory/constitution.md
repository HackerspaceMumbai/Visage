<!--
SYNC IMPACT REPORT
==================
Version change: 1.0.0 → 1.1.0
@@Version change: 1.1.0 → 1.2.0
@@Version change: 1.2.0 → 1.3.0
@@Version change: 1.3.0 → 1.4.0
Modified principles: 
  - Principle IV: Blazor Hybrid UI Consistency → Expanded with render mode strategy
  - Principle VI: Security & Privacy by Design → Expanded with IdP abstraction
@@  - Principle VIII: Blazor Render Mode Strategy → Expanded with navigation best practices and architectural guidance
@@  - Principle I: Aspire-First Architecture → Added mandatory health endpoint testing requirement
@@  - Principle III: Integration Testing → Promoted health endpoint testing to mandatory priority #2
@@  - Technology Stack Requirements: Approved Patterns → Added form validation, DateTime comparison, and UI consistency patterns
@@  - Code Quality Gates → Added render mode verification, form validation checks, and DateTime validation requirements
@@  - Code Quality Gates → Added mandatory health endpoint testing gate (Gate #2)
Added sections:
  - Principle VIII: Blazor Render Mode Strategy (new)
  - Principle IX: Identity Provider Abstraction (new)
  - DaisyUI styling requirements in Technology Stack
  - Three new prohibited patterns (IdP coupling, WASM for auth, render mode mixing)
  - Two new quality gates (render mode, DaisyUI styling)
@@  - Blazor render mode navigation rules (mixing modes breaks SPA routing)
@@  - Form validation pattern: EditForm + DataAnnotationsValidator + ValidationMessageStore + inline ValidationMessage
@@  - DateTime comparison pattern: Use full DateTime (not DateOnly) for time-sensitive logic
@@  - UI consistency rule: Maintain visual styling across component states
@@  - Architectural guidance for Visage: InteractiveServer app-wide default for Auth0/Aspire
@@  - Mandatory health endpoint testing in Principle I (all Aspire services)
@@  - Automated health endpoint test suite requirement in Principle III (#2 priority)
@@  - Quality gate #2: All health endpoint tests must pass before merge
Removed sections: N/A
Templates status:
  ✅ plan-template.md - Updated (added render mode, IdP abstraction, and DaisyUI checks)
  @@  ✅ plan-template.md - Review recommended (render mode navigation checks)
  @@  ✅ plan-template.md - Review recommended (health endpoint testing checklist)
  ✅ spec-template.md - Compatible (no changes needed)
  ✅ tasks-template.md - Compatible (no changes needed)
Follow-up TODOs:
  - Review copilot-instructions.md for IdP abstraction guidance (manual review recommended)
  - Add Blazor render mode decision guide to blazor-guidance.md (manual task)
  - Document DaisyUI integration pattern in frontend documentation (manual task)
@@  - ✅ copilot-instructions.md updated with render mode, form validation, and DateTime patterns
@@  - ✅ copilot-instructions.md updated with mandatory health endpoint testing pattern and examples
@@  - Consider adding Blazor navigation troubleshooting guide to documentation
@@  - Consider documenting form validation pattern examples in frontend documentation
@@  - Verify all existing Aspire services have health endpoint tests (T051 automated)
@@Version 1.4.0 Changes (2025-11-11):
@@  - Added Principle XI: Test-Driven Workflow Discipline (NEW)
@@  - Quality Gates: Added Gate #0 - Baseline verification with TEST-BASELINE.md requirement
@@  - Efficiency protocols: Token budget awareness, context-first reading, minimal documentation
@@  - Test execution discipline: Stop after 2 failures, compare against baseline, isolate test runs
@@  - Documentation policy: Only create docs when explicitly requested or establishing new patterns
@@Follow-up TODOs:
@@  - Create tests/TEST-BASELINE.md documenting current 19 pre-existing test failures
@@  - Update copilot-instructions.md with test-driven workflow patterns
@@  - Consider adding test execution checklists to developer documentation
@@Version 1.4.0 Changes Continued:
@@  - Principle IX: Added UI Change Verification Protocol (MANDATORY: run Tailwind CLI before viewing UI changes)
@@  - Principle IX-B: Blazor Prerendering State Management (NEW) - double lifecycle pattern for InteractiveServer/Auto
@@  - Anti-patterns: HttpContext access without null checks, state initialization without persistence
@@  - State persistence pattern: Check HttpContext availability, cache data between render phases
-->

# Visage Constitution

## Core Principles

### I. Aspire-First Architecture

Every service MUST be orchestrated through .NET Aspire. All services MUST:

- Be registered as project resources in `Visage.AppHost/Program.cs`
- Use `.WaitFor()` to declare startup dependencies explicitly
- Call `AddServiceDefaults()` to enable health checks, OpenTelemetry, and service discovery
- Expose health endpoints at `/health` and `/alive` in development environments
- **Have automated health endpoint tests** in `tests/Visage.Test.Aspire/HealthEndpointTests.cs` verifying both `/health` and `/alive` return 200 OK
- Be containerizable for local development and cloud deployment

**Rationale**: Aspire orchestration ensures consistent service discovery, observability, and
deployment patterns across the entire solution, showcasing .NET 10's modern distributed
application capabilities. Automated health endpoint tests ensure all services meet operational
readiness requirements and prevent deployment of misconfigured services.

### II. Minimal API Design

Backend services MUST use Minimal APIs with the following constraints:

- All endpoints for a logical domain MUST be in a single file (e.g., `ProfileApi.cs`)
- Each service MUST expose Scalar OpenAPI documentation at `/scalar/v1` in development
- Each service MUST provide an `.http` file for ad-hoc testing
- Use EF Core with Repository and Unit of Work patterns for data access
- Configure StrictId for all entity IDs in `OnModelCreating`

**Rationale**: Minimal APIs reduce boilerplate, improve readability, and showcase .NET 10's
lightweight API capabilities. Single-file-per-domain keeps related endpoints together for
maintainability.

### III. Integration Testing Over Unit Testing (NON-NEGOTIABLE)

Testing strategy MUST prioritize:

1. **Integration tests** (primary) - Use TUnit and Fluent Assertions for API endpoints,
   database interactions, and service-to-service communication
2. **Health endpoint tests** (mandatory) - All Aspire services MUST have automated tests verifying
   `/health` and `/alive` endpoints return 200 OK in `tests/Visage.Test.Aspire/HealthEndpointTests.cs`
3. **E2E tests** - Use Playwright for critical user journeys across the Blazor Hybrid UI
4. **Load tests** - Use NBomber for performance validation under expected load
5. **Security tests** - Use OWASP ZAP for vulnerability scanning
6. **Mutation tests** - Use Stryker for test quality validation
7. **Unit tests** (minimal) - Use bunit ONLY for critical Blazor components

**Rationale**: As a real-world meetup management platform with over-subscribed events,
integration tests validate actual behavior across service boundaries and data stores. Health
endpoint tests ensure operational readiness and prevent deployment of misconfigured services.
This approach prioritizes confidence in production behavior over isolated component testing.

### IV. Blazor Hybrid UI Consistency

The frontend MUST maintain a single codebase for web and mobile using Blazor Hybrid:

- Shared UI components in `Visage.FrontEnd.Shared/`
- Web-specific implementation in `Visage.FrontEnd.Web/` and `Visage.FrontEnd.Web.Client/`
- Mobile-specific implementation in `Visage.FrontEnd/` (MAUI)
- Shared data models in `Visage.FrontEnd.Shared/Models/`

**Rationale**: Blazor Hybrid maximizes code reuse across platforms, reducing maintenance
burden and ensuring consistent user experience for event organizers and attendees across
devices.

### V. Observability First

All services MUST emit telemetry through OpenTelemetry:

- Traces for all API requests and inter-service calls
- Metrics for application health and business KPIs
- Structured logging for debugging and audit trails
- Integration with Azure Monitor and Application Insights in production

Implementation via `Visage.ServiceDefaults/Extensions.cs` ensures consistent instrumentation.

**Rationale**: With high-stakes events (over-subscribed registrations, real-time check-ins),
comprehensive observability enables rapid troubleshooting and data-driven capacity planning.

### VI. Security & Privacy by Design

Given DPDP compliance requirements (India's GDPR equivalent):

- All user profile endpoints MUST require Auth0 JWT with `profile:read-write` scope
- Sensitive data MUST reside within India jurisdiction
- All inputs MUST be validated using parameterized queries
- All commits MUST be signed off
- Regular security scanning with OWASP ZAP

**Rationale**: As a community platform handling personal data for large-scale events, security
and privacy compliance are non-negotiable for legal and ethical reasons.

### VII. Technology Showcase

The solution MUST showcase the latest .NET 10 features:

- .NET Aspire for orchestration and observability
- Minimal APIs for lightweight, performant backend services
- Blazor Hybrid for cross-platform UI with a single codebase
- EF Core with modern patterns (StrictId, Repository/UoW)
- Native AOT compilation where applicable
- C# 14 features where appropriate

**Rationale**: Visage serves as a reference implementation for the OSS community in Mumbai,
demonstrating .NET 10's capabilities for building modern, scalable, distributed applications.

### VIII. Blazor Render Mode Strategy

Blazor components MUST use the appropriate render mode based on their security and performance requirements:

- **Static SSR (default)**: Public pages without user interaction (marketing, docs, landing pages)
- **InteractiveServer**: Authenticated pages requiring real-time updates, secure data access, or server-side validation (user profiles, admin dashboards, check-in flows)
- **InteractiveWebAssembly**: Client-side interactive features that don't require server data or authentication (theme toggles, UI animations, offline-capable features)

### IX. DaisyUI Build (INPUT → OUTPUT)

- **Input file**: The DaisyUI / Tailwind source used by the frontend is `Visage.FrontEnd/Visage.FrontEnd.Shared/Styles/input.css` (this is the LLM-friendly, human-editable source that imports Tailwind and the DaisyUI plugins). Do not create an alternate input file; this is the single source of truth for DaisyUI theme configuration.
- **Generated output**: The compiled stylesheet consumed by the web project is `Visage.FrontEnd/Visage.FrontEnd.Shared/wwwroot/output.css`.
- **How to regenerate**: Run the Tailwind CLI via pnpx from the repository root to compile `input.css` into `output.css`. Example command (uses Tailwind v4 as required by DaisyUI 5):

```pwsh
pnpx tailwindcss@4 -i Visage.FrontEnd/Visage.FrontEnd.Shared/Styles/input.css -o Visage.FrontEnd/Visage.FrontEnd.Shared/wwwroot/output.css --minify
```

- **CI / developer note**: Add this command to your local dev workflow or CI pipeline so `output.css` is regenerated whenever `input.css` (or the theme config in `input.css`) changes. The repository currently includes a committed `output.css` for convenience — treat it as a generated artifact and regenerate when making style/theme edits.
- **Reference**: For DaisyUI usage rules, conventions, and plugin configuration, see the LLM-friendly guide at `.vscode/daisyui.md` in the repo. Follow that guidance when editing `input.css` (theme blocks, plugins, and allowed patterns).

#### Migration & Component Styling Guidance (added 2025-10-21)

- **Single source of truth**: Do not duplicate theme variables in multiple files. Migrate any existing theme files (for example `wwwroot/css/daisy-theme.css`) into the canonical `input.css` so the compiled `output.css` contains the authoritative styling.
- **Component-scoped styles**: Prefer component-scoped CSS files for Blazor components (`Component.razor.css`) placed next to the component. Use these files for component-specific visual rules (focus rings, layout tweaks, minor overrides) rather than sprinkling bespoke styles into global stylesheets.
- **Remove redundant links**: Do not include duplicate theme stylesheet links in `App.razor` or other host pages if their variables are already compiled into `_content/Visage.FrontEnd.Shared/output.css`.
- **CI / pipeline**: Add a pipeline step that runs the Tailwind build before the .NET build. Example (PowerShell / Azure Pipelines) to run from repository root:

```pwsh
pnpm --prefix Visage.FrontEnd/Visage.FrontEnd.Shared install
pnpm --prefix Visage.FrontEnd/Visage.FrontEnd.Shared run buildcss
```

- **Generated artifact policy**: `Visage.FrontEnd/Visage.FrontEnd.Shared/wwwroot/output.css` is a generated artifact. It may be committed for convenience in local development, but CI and release pipelines MUST regenerate it from `input.css` before packaging/deploying.

**UI Change Verification Protocol (MANDATORY)**:

Before viewing any UI changes in the browser:

1. **ALWAYS run Tailwind CLI first** to regenerate `output.css`:

   ```pwsh
   pnpx tailwindcss@4 -i Visage.FrontEnd/Visage.FrontEnd.Shared/Styles/input.css -o Visage.FrontEnd/Visage.FrontEnd.Shared/wwwroot/output.css --minify
   ```

2. **Then** build and run Aspire:

   ```pwsh
   dotnet build
   dotnet run --project Visage.AppHost
   ```

**Common Mistake**: Viewing UI without regenerating CSS results in stale styles—DaisyUI changes won't appear even though Blazor code changed.

- **InteractiveAuto**: Pages requiring initial fast load with subsequent rich interactivity (event listings with filtering, registration forms with client validation)

Components MUST NOT:

- Use InteractiveWebAssembly for pages accessing secure APIs or user data
- Use InteractiveServer for purely cosmetic interactions (increases server load unnecessarily)
- Mix render modes within a single component without justification
@@- Mix render modes across navigation boundaries (e.g., InteractiveAuto → InteractiveServer) as this breaks Blazor's SPA routing
@@- Override app-wide render mode settings on individual pages unless explicitly justified and documented

**Rationale**: Correct render mode selection ensures optimal security (server-side for auth),
performance (client-side for UI), and resource efficiency. As a high-traffic event platform,
minimizing server connections for non-critical features reduces infrastructure costs while
maintaining security for sensitive operations.
@@Consistent render mode strategy prevents navigation issues and runtime errors when navigating between pages.
@@
@@**Architectural Guidance for Visage**:
@@- Set InteractiveServer as the app-wide default in `App.razor` on the `<Routes>` component for applications using Auth0 server-side authentication and Aspire service discovery
@@- Use InteractiveAuto sparingly and only when justified (e.g., initial fast load with subsequent rich interactivity)
@@- Avoid per-page render mode overrides; centralize render mode strategy for predictable navigation behavior

### IX-B. Blazor Prerendering State Management

Components with InteractiveServer or InteractiveAuto render modes undergo **two-phase lifecycle**:

#### Phase 1: Server Prerendering (Static HTML)

- Component renders once on server during initial page load
- State initialization happens, but no SignalR connection exists
- No user interaction is possible
- HTTP context available via `HttpContext`

#### Phase 2: Interactive Rendering (SignalR Connected)

- Component re-initializes after SignalR connects
- State initialization happens AGAIN
- User interactions now work
- HTTP context NOT available

**Critical Rules:**

1. **Always check `if (HttpContext is not null)`** before accessing request-specific data during prerendering
2. **Initialize state in `OnInitializedAsync`** - it runs in BOTH phases
3. **Dispose resources in `Dispose`** - prevent memory leaks from double initialization
4. **Avoid HttpContext in interactive phase** - use cascading parameters or service state instead
5. **Use `[SupplyParameterFromQuery]` carefully** - only available during prerendering, null during interactive

**Anti-Patterns:**

```csharp
// ❌ BAD: Assumes HttpContext always available
public string UserId => HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

// ✅ GOOD: Checks for prerendering phase
public string UserId => HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? _cachedUserId;
```

**State Persistence Pattern:**

```csharp
private string? _userId;

protected override async Task OnInitializedAsync()
{
    // Runs twice: prerender + interactive
    if (HttpContext is not null)
    {
        // First render (prerender): Get from HTTP context
        _userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
    else if (_userId is null)
    {
        // Second render (interactive): Load from service/state
        _userId = await AuthService.GetCurrentUserIdAsync();
    }
}
```

**Rationale**: Prerendering provides fast initial page loads (good for SEO and perceived performance),
but the double lifecycle often causes confusion with state management. Understanding this pattern
prevents NullReferenceExceptions and duplicate data loads.

### X. Identity Provider Abstraction

Authentication MUST be implemented through an abstraction layer to enable IdP replacement:

- All authentication logic MUST be encapsulated behind interfaces (e.g., `IAuthenticationService`, `IUserClaimsProvider`)
- Auth0-specific code MUST be isolated in implementation classes, never in business logic or UI
- JWT validation, claims extraction, and scope checking MUST work with any OIDC-compliant provider
- Configuration MUST use generic keys (`Authentication:Authority`, `Authentication:ClientId`) mapped to provider-specific values
- No direct references to Auth0 SDK types in shared models, DTOs, or domain logic

**Design Goal**: Enable migration to Keycloak, Microsoft Entra ID, or other OIDC providers with
configuration changes and single implementation class updates—no business logic rewrites.

**Rationale**: As an OSS project, Visage may be deployed by communities using different IdPs.
By abstracting authentication, we enable deployment flexibility while maintaining the same
codebase. This also future-proofs against vendor lock-in and demonstrates enterprise-grade
architectural patterns.

### XI. Test-Driven Workflow Discipline

All testing activities MUST follow this disciplined workflow:

**Before Writing Tests:**

1. Read and understand existing test patterns (e.g., `SqlServerIntegrationTests.cs`)
2. Verify dependencies are correctly configured (e.g., health endpoints anonymous in `Extensions.cs`)
3. Document baseline test status BEFORE adding new tests
4. Identify exact test scope and acceptance criteria

**During Test Execution:**

1. Run minimal test subset first using filters (prefer isolation)
2. If filter fails to isolate, build project to verify compilation BEFORE full test run
3. Stop after 2 consecutive test failures—report issue, don't debug silently
4. Compare results against baseline to distinguish new vs. pre-existing failures

**After Test Implementation:**

1. Document ONLY what user explicitly requests (no unsolicited milestone docs)
2. Update constitution/instructions ONLY when creating new patterns/requirements
3. Verify test can be isolated and re-run independently

**Efficiency Protocols:**

- **Token Budget Awareness**: Avoid repetitive full test suite runs; use build verification first
- **Context-First**: Read configuration files BEFORE creating dependent tests
- **Minimal Documentation**: Only create docs when establishing new patterns, not for routine tasks
- **Baseline Tracking**: Always document pre-existing test failures in a `TEST-BASELINE.md` file

**Rationale**: Test-driven workflows require discipline to avoid wasted cycles. By establishing
baselines, verifying configuration, and using targeted execution, we minimize debugging time
and token consumption while maintaining confidence in test results.

## Technology Stack Requirements

### Mandatory Stack

- **Runtime**: .NET 10 (pinned version in `global.json`)
- **Orchestration**: .NET Aspire
- **Backend**: Minimal APIs, EF Core 10, StrictId
- **Frontend**: Blazor Hybrid (Web + MAUI)
- **UI Framework**: DaisyUI 5 (Tailwind CSS components) for consistent, accessible UI
- **Authentication**: Auth0 (abstracted via interfaces for provider replaceability)
- **Image Storage**: Cloudinary (via Node.js signing service)
- **Testing**: TUnit, Fluent Assertions, Playwright, NBomber, OWASP ZAP, Stryker, bunit
- **Mocking**: NSubstitute
- **Observability**: OpenTelemetry, Azure Monitor, Application Insights

### Approved Patterns

- Repository and Unit of Work for data access
- Service Defaults for cross-cutting concerns (health, telemetry, discovery)
- Shared models in `Visage.Shared/` for backend DTOs
- Shared UI components in `Visage.FrontEnd.Shared/` styled with DaisyUI
- Authentication abstraction via interfaces (`IAuthenticationService`, `IUserClaimsProvider`)
- Blazor render modes: Static SSR (default), InteractiveServer (auth/secure), InteractiveWebAssembly (client-side), InteractiveAuto (hybrid)
@@- Form validation: EditForm with DataAnnotationsValidator + custom ValidationMessageStore + inline ValidationMessage components for field-specific errors
@@- DateTime comparisons: Use full DateTime for event status classification (not DateOnly) to prevent same-day future events from being misclassified
@@- UI consistency: Maintain consistent visual styling across component states (e.g., buttons for both active/disabled states)
- Aspire CLI tools (9.5+):
  - `aspire exec --resource <name> -- <command>`: Execute commands in resource context with automatic environment variable injection (requires feature flag)
  - `aspire exec --workdir /path -- <command>`: Run commands in specific working directory (Aspire 9.5+)
  - `aspire update`: Keep Aspire packages and templates current (preview)
  - `aspire config`: Manage feature flags and CLI configuration
  - `aspire run` failure protocol: If aspire run fails repeatedly (2+ attempts), STOP and report the issue with error details to the user immediately. Do not continue debugging in silence.
- Containerization for all services (Docker/Podman)

### Prohibited Patterns

- Unit-of-Work bypass (direct DbContext usage outside repositories)
- Global state in stateless services
- Synchronous I/O in API endpoints
- Unscoped service injection in singleton services
- Hard-coded secrets or connection strings
- Direct Auth0 SDK references in business logic, domain models, or shared UI components
- InteractiveWebAssembly render mode for authenticated or secure pages
- Mixing render modes without architectural justification

## Development Workflow & Quality Gates

### Code Quality Gates

Before merging any PR:

0. **Baseline Verification**: Document current test status in `tests/TEST-BASELINE.md` if not present
   - List all pre-existing test failures with root causes
   - Distinguish new failures from technical debt
   - Update baseline when resolving pre-existing issues
1. All integration tests MUST pass (`dotnet test tests/Visage.Tests.Integration/`)
2. **All health endpoint tests MUST pass** (`tests/Visage.Test.Aspire/HealthEndpointTests.cs`) - Every Aspire service MUST have `/health` and `/alive` endpoints returning 200 OK
3. All E2E tests MUST pass for modified user journeys
4. Scalar OpenAPI documentation MUST be up-to-date for API changes
5. `.http` files MUST include examples for new endpoints
6. Service registration in AppHost MUST be correct with proper `.WaitFor()` dependencies
7. Observability: New endpoints MUST emit appropriate traces and metrics
8. Security: Authentication scopes MUST be correctly applied to protected endpoints (no provider-specific code in business logic)
9. Blazor components: Render mode MUST be appropriate for security/performance requirements
@@   - App-wide render mode MUST be documented in App.razor
@@   - Per-page overrides MUST be justified in PR description
@@   - Navigation between pages with different render modes MUST be tested
10. UI components: MUST use DaisyUI classes for styling consistency and accessibility
@@11. Form validation: MUST include both ValidationSummary and inline ValidationMessage components
@@12. DateTime logic: MUST use full DateTime (not DateOnly) for time-sensitive business logic like event status classification

### Commit Conventions

- Imperative mood, sentence case, no trailing period
- All commits MUST be signed off (Developer Certificate of Origin)
- Example: `Add user profile endpoint with Auth0 integration`

### Refactoring Rules

- Split files/functions when they exceed ~500 lines or become difficult to reason about
- Do NOT refactor unrelated code in feature branches
- Preserve existing API contracts unless explicitly breaking change

### Performance Expectations

@@**Version**: 1.3.0 | **Ratified**: 2025-10-17 | **Last Amended**: 2025-10-24

- Blazor UI: Initial load < 2 seconds, interactions < 100ms perceived latency
- Database queries: Use indexes, avoid N+1 queries
- Load testing: MUST handle 1000 concurrent users for registration surges

### Documentation Requirements

- All new services: README.md with quickstart, architecture diagram, API overview
- All new features: User stories in spec.md with acceptance criteria
- All architectural decisions: Document in `/design` or architecture decision records (ADRs)

## Governance

### Amendment Process

1. Propose amendment with rationale and impact analysis
2. Discuss with project maintainers and community
3. Update constitution with semantic version bump:
   - MAJOR: Backward-incompatible principle removal/redefinition
   - MINOR: New principle or materially expanded guidance
   - PATCH: Clarifications, wording improvements, typo fixes
4. Update dependent templates (plan, spec, tasks)
5. Commit with sign-off

### Compliance Review

- Constitution compliance MUST be verified in all PR reviews
- Architectural deviations MUST be justified and documented in plan.md Complexity Tracking
- Annual constitution review to ensure alignment with .NET evolution and community needs

### Authority

- This constitution supersedes all other practices and conventions
- For runtime development guidance, refer to `.github/copilot-instructions.md` and prompt files
  in `.github/prompts/`
- Complexity must be justified: if constitution rules are violated, document in plan.md

**Version**: 1.3.0 | **Ratified**: 2025-10-17 | **Last Amended**: 2025-10-24
