# Implementation Plan: Verified social profile linking

**Branch**: `004-verify-social-profiles` | **Date**: 2025-12-13 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-verify-social-profiles/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Replace free-form LinkedIn/GitHub entry in mandatory registration with verified “Connect” flows that prove account control, store the verified profile URL + verification timestamp, and make the verified links available for curation.

This repo already contains the UI affordances and persistence endpoints (T087/T088). The remaining work is to:

- make the verification flow authoritative (direct OAuth instead of Auth0 social connection claim parsing),
- enforce uniqueness (one social account cannot be verified for multiple registrants),
- persist disconnect actions, and
- add an auditable trail of link/unlink/verify outcomes.

## Technical Context

**Language/Version**: C# with .NET 10 (pinned in `global.json`)  
**Primary Dependencies**:

- .NET Aspire service discovery + service defaults (`Visage.ServiceDefaults`)
- Blazor (Hybrid, InteractiveServer as app-wide default render mode)
- Minimal APIs for backend services (`Visage.Services.Registrations`)
- Auth0 OIDC (primary auth), with bearer access tokens for profile APIs
- EF Core + SQL Server (Aspire-managed `registrationdb`)
- Scalar OpenAPI (`/scalar/v1` in dev)
- DaisyUI 5 + Tailwind CSS 4

**Storage**: SQL Server (`registrationdb`) via Aspire (`Visage.AppHost/AppHost.cs`)  
**Testing**:

- Integration tests: TUnit + FluentAssertions (existing pattern in `tests/`)
- Aspire health endpoint tests (mandatory) in `tests/Visage.Test.Aspire/HealthEndpointTests.cs`
- E2E: Playwright (existing repo approach; add/extend only if needed)

**Target Platform**: Web (Blazor Server/Hybrid), with shared UI library for future MAUI reuse  
**Project Type**: Web application (frontend Blazor + backend Minimal API service)  
**Performance Goals**:

- Social status API: fast (<200ms typical) and safe to call on registration page load
- OAuth callback processing: bounded latency; avoid long-running work in UI thread

**Constraints**:

- Must keep InteractiveServer render mode consistent to avoid SPA navigation breakage
- Must minimize personal data: store verified profile URL + timestamps only (no access tokens)
- Must enforce `profile:read-write` scope for profile mutation endpoints
- Must be resilient to user cancel/denial and provider errors

**Scale/Scope**:

- LinkedIn/GitHub verification used for event curation; expected burst traffic during event announcements
- Scope is limited to verification + persistence + curation visibility (no automated scoring)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Aspire-First Architecture**: No new services required; uses existing Aspire-registered `registrations-api` + `frontendweb`
- [x] **Minimal API Design**: Extend existing endpoints in `Visage.Services.Registrations/ProfileApi.cs`; document contract in `specs/004-verify-social-profiles/contracts/`
- [x] **Integration Testing Priority**: Add/extend integration tests for social link, status, and conflict cases; avoid relying solely on unit tests
- [x] **Blazor Hybrid UI**: Keep “Connect” UI in `Visage.FrontEnd.Shared/Components/MandatoryRegistration.razor*`
- [x] **Observability**: Log link/unlink/verify outcomes; rely on ServiceDefaults OpenTelemetry instrumentation for HTTP traces
- [x] **Security & Privacy**: OAuth state/CSRF handling; endpoints require Auth0 access token with `profile:read-write`; store only verified URLs + timestamps
- [x] **Technology Showcase**: Uses .NET 10, Aspire service discovery, Blazor InteractiveServer, and Scalar OpenAPI
- [x] **Blazor Render Mode Strategy**: Do not override per-page render modes; keep app-wide InteractiveServer defaults
- [x] **Identity Provider Abstraction**: Primary authentication stays OIDC/Auth0; social verification behind `ISocialAuthService` (no provider SDK in shared UI)
- [x] **DaisyUI Styling**: Use DaisyUI components already present (alerts, buttons, badges, loading spinners)

*If any checks fail, justify in Complexity Tracking section below.*

## Project Structure

### Documentation (this feature)

```
specs/004-verify-social-profiles/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
# Backend: Registrations service (Minimal API)
Visage.Services.Registrations/
├── ProfileApi.cs                # EXTEND: social link/status/disconnect endpoints + conflict handling
├── RegistrantDB.cs              # EXTEND: audit entity/table + filtered unique indexes (if needed)
├── Program.cs                   # (only if new DI services are required)
└── app.http                     # EXTEND: test requests for social endpoints

# Frontend: shared UI components
Visage.FrontEnd/Visage.FrontEnd.Shared/
├── Components/
│   ├── MandatoryRegistration.razor
│   └── MandatoryRegistration.razor.cs
├── Pages/
│   └── OAuthCallback.razor      # DEPRECATE/REPLACE if moving fully to direct provider OAuth
└── Services/
    └── SocialAuthService.cs     # UPDATE: initiate direct OAuth flow; persist verified URLs to backend

# Frontend: web host (OAuth endpoints + middleware)
Visage.FrontEnd/Visage.FrontEnd.Web/
├── Program.cs                   # UPDATE: add direct OAuth endpoints + session middleware
├── Services/DirectOAuthService.cs
└── Configuration/OAuthOptions.cs

# Aspire orchestration
Visage.AppHost/AppHost.cs        # already wires OAuth secrets into frontendweb

# Tests
tests/Visage.Test.Aspire/HealthEndpointTests.cs
tests/Visage.Tests.Integration/  # EXTEND: add coverage for social endpoints and uniqueness conflicts
```

**Structure Decision**: Web application. This feature extends the existing Blazor Hybrid frontend and the existing Registrations Minimal API service; no new projects should be introduced.

## Phase 0: Research

**Goal**: Decide the authoritative verification path (Auth0 social connection claims vs direct provider OAuth) and document security/privacy handling (state, CSRF, data minimization).

**Output**: `research.md`

## Phase 1: Design

**Goal**: Define data model + API contracts.

**Outputs**:

- `data-model.md`
- `contracts/social-profile-linking-api.yaml`
- `quickstart.md`

## Phase 2: Tasks

**Goal**: Generate implementable tasks and acceptance-checklist for code changes.

**Output (later)**: `tasks.md` via `/speckit.tasks`

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

