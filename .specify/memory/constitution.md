<!--
SYNC IMPACT REPORT
==================
Version change: 1.0.0 → 1.1.0
Modified principles: 
  - Principle IV: Blazor Hybrid UI Consistency → Expanded with render mode strategy
  - Principle VI: Security & Privacy by Design → Expanded with IdP abstraction
Added sections:
  - Principle VIII: Blazor Render Mode Strategy (new)
  - Principle IX: Identity Provider Abstraction (new)
  - DaisyUI styling requirements in Technology Stack
  - Three new prohibited patterns (IdP coupling, WASM for auth, render mode mixing)
  - Two new quality gates (render mode, DaisyUI styling)
Removed sections: N/A
Templates status:
  ✅ plan-template.md - Updated (added render mode, IdP abstraction, and DaisyUI checks)
  ✅ spec-template.md - Compatible (no changes needed)
  ✅ tasks-template.md - Compatible (no changes needed)
Follow-up TODOs:
  - Review copilot-instructions.md for IdP abstraction guidance (manual review recommended)
  - Add Blazor render mode decision guide to blazor-guidance.md (manual task)
  - Document DaisyUI integration pattern in frontend documentation (manual task)
-->

# Visage Constitution

## Core Principles

### I. Aspire-First Architecture

Every service MUST be orchestrated through .NET Aspire. All services MUST:

- Be registered as project resources in `Visage.AppHost/Program.cs`
- Use `.WaitFor()` to declare startup dependencies explicitly
- Call `AddServiceDefaults()` to enable health checks, OpenTelemetry, and service discovery
- Expose health endpoints at `/health` and `/alive` in development environments
- Be containerizable for local development and cloud deployment

**Rationale**: Aspire orchestration ensures consistent service discovery, observability, and
deployment patterns across the entire solution, showcasing .NET 10's modern distributed
application capabilities.

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
2. **E2E tests** - Use Playwright for critical user journeys across the Blazor Hybrid UI
3. **Load tests** - Use NBomber for performance validation under expected load
4. **Security tests** - Use OWASP ZAP for vulnerability scanning
5. **Mutation tests** - Use Stryker for test quality validation
6. **Unit tests** (minimal) - Use bunit ONLY for critical Blazor components

**Rationale**: As a real-world meetup management platform with over-subscribed events,
integration tests validate actual behavior across service boundaries and data stores. This
approach prioritizes confidence in production behavior over isolated component testing.

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
- **InteractiveAuto**: Pages requiring initial fast load with subsequent rich interactivity (event listings with filtering, registration forms with client validation)

Components MUST NOT:

- Use InteractiveWebAssembly for pages accessing secure APIs or user data
- Use InteractiveServer for purely cosmetic interactions (increases server load unnecessarily)
- Mix render modes within a single component without justification

**Rationale**: Correct render mode selection ensures optimal security (server-side for auth),
performance (client-side for UI), and resource efficiency. As a high-traffic event platform,
minimizing server connections for non-critical features reduces infrastructure costs while
maintaining security for sensitive operations.

### IX. Identity Provider Abstraction

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

1. All integration tests MUST pass (`dotnet test tests/Visage.Tests.Integration/`)
2. All E2E tests MUST pass for modified user journeys
3. Scalar OpenAPI documentation MUST be up-to-date for API changes
4. `.http` files MUST include examples for new endpoints
5. Service registration in AppHost MUST be correct with proper `.WaitFor()` dependencies
6. Observability: New endpoints MUST emit appropriate traces and metrics
7. Security: Authentication scopes MUST be correctly applied to protected endpoints (no provider-specific code in business logic)
8. Blazor components: Render mode MUST be appropriate for security/performance requirements
9. UI components: MUST use DaisyUI classes for styling consistency and accessibility

### Commit Conventions

- Imperative mood, sentence case, no trailing period
- All commits MUST be signed off (Developer Certificate of Origin)
- Example: `Add user profile endpoint with Auth0 integration`

### Refactoring Rules

- Split files/functions when they exceed ~500 lines or become difficult to reason about
- Do NOT refactor unrelated code in feature branches
- Preserve existing API contracts unless explicitly breaking change

### Performance Expectations

- API endpoints: < 200ms p95 latency for typical requests
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

**Version**: 1.1.0 | **Ratified**: 2025-10-17 | **Last Amended**: 2025-10-19
