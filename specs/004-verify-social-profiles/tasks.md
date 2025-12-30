---
description: "Task list for Verified social profile linking feature implementation"
---

# Tasks: Verified social profile linking

**Input**: Design documents from `/specs/004-verify-social-profiles/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/social-profile-linking-api.yaml`, `quickstart.md`

**Tests**:
- **Aspire integration tests are mandatory** (TUnit + FluentAssertions) in `tests/Visage.Test.Aspire/`.
- These endpoints require Auth0 access tokens, so most tests here should be `[Category("RequiresAuth")]`.
- Playwright E2E is optional for this feature (use it only if we want a browser-level regression guard for the Connect flow).
- bUnit tests are optional and should be added only for critical Blazor UI logic that can’t be covered by integration tests.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing (P1 → P2 → P3).

## Format: `T### [P?] [US#] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[US#]**: Which user story this task belongs to (US1/US2/US3)
- Each task includes exact file paths

## Path conventions (Visage)
- Backend: `Visage.Services.Registrations/` (Minimal API)
- Frontend shared UI: `Visage.FrontEnd/Visage.FrontEnd.Shared/`
- Frontend web host: `Visage.FrontEnd/Visage.FrontEnd.Web/`
- Shared contracts/models: `Visage.Shared/Models/`
- Tests: `tests/Visage.Test.Aspire/`

---

## Phase 1: Setup (Shared prep)

- [x] T001 [P] [US1] Re-read quickstart + ensure OAuth secrets are configured for local dev (`setup-oauth-secrets.bat`, `Visage.AppHost/AppHost.cs`)
- [x] T002 [P] [US1] Identify and decide what to do with the currently *orphaned* direct OAuth code under `Visage.FrontEnd.Web/` (not part of any `.csproj`): either move it into the web project or delete it after re-implementing in-project
- [x] T003 [P] [US1] Add/confirm baseline test filter expectations so new tests don’t surprise CI (see `Directory.Build.props`, `tests/TEST-BASELINE.md`)

**Checkpoint**: Ready to start implementation work.

---

## Phase 2: Foundational (Blocking prerequisites)

> ⚠️ **CRITICAL**: Finish this phase before implementing any user story tasks below.

- [x] T010 [P] Add shared DTO for disconnect requests: create `Visage.Shared/Models/SocialDisconnectDto.cs` (provider enum/string: `linkedin|github`)
- [x] T011 [P] Add shared model for audit events: create `Visage.Shared/Models/SocialVerificationEvent.cs` (per `data-model.md`)
- [x] T012 Update EF Core context to support audit + uniqueness:
  - `Visage.Services.Registrations/RegistrantDB.cs`: add `DbSet<SocialVerificationEvent>`
  - `Visage.Services.Registrations/RegistrantDB.cs`: add filtered unique indexes for verified LinkedIn/GitHub profiles (SQL Server `HasFilter(...)`)
- [x] T013 Create EF Core migration for:
  - new `SocialVerificationEvents` table
  - filtered unique indexes on `Registrants.LinkedInProfile` and `Registrants.GitHubProfile` (only when verified)
  - files under `Visage.Services.Registrations/Migrations/`
  - Use Aspire: `aspire exec --resource registrations-api --workdir ./Visage.Services.Registrations -- dotnet ef migrations add AddSocialVerificationAuditAndUniqueIndexes`
  - (then) `aspire exec --resource registrations-api --workdir ./Visage.Services.Registrations -- dotnet ef database update`- [x] T014 [P] Add a minimal `.http` section stub for new/updated endpoints in `Visage.Services.Registrations/app.http` (link-callback, status, disconnect)

**Checkpoint**: Schema + shared types exist; API work can proceed.

---

## Phase 3: User Story 1 — Link and verify required social profile(s) (Priority: P1) 🎯 MVP

**Goal**: Replace free-form profile entry with verified Connect flows (LinkedIn/GitHub), store verified URLs + timestamps, and enforce occupation rules.

**Independent Test**: A user can link LinkedIn/GitHub (via direct OAuth), see “Verified” state on `/registration/mandatory`, and registration blocks when the required profile isn’t verified.

### Tests for US1 (mandatory)

- [x] T020 [P] [US1] Add integration test: `GET /api/profile/social/status` returns expected shape + defaults when no social profiles are verified  
  File: `tests/Visage.Test.Aspire/SocialProfileStatusTests.cs` (`[Category("RequiresAuth")]`)
- [x] T021 [P] [US1] Add integration test: `POST /api/profile/social/link-callback` persists verification fields for provider `linkedin`  
  File: `tests/Visage.Test.Aspire/SocialProfileLinkingTests.cs` (`[Category("RequiresAuth")]`)
- [x] T022 [P] [US1] Add integration test: `POST /api/profile/social/link-callback` persists verification fields for provider `github`  
  File: `tests/Visage.Test.Aspire/SocialProfileLinkingTests.cs` (`[Category("RequiresAuth")]`)
- [x] T023 [P] [US1] Add integration test: uniqueness enforcement returns `409 Conflict` when trying to verify a profile URL already verified for another registrant  
  File: `tests/Visage.Test.Aspire/SocialProfileUniquenessTests.cs` (`[Category("RequiresAuth")]`)
- [x] T024 [P] [US1] Add integration test: audit events are created on link success/failure  
  File: `tests/Visage.Test.Aspire/SocialProfileAuditTests.cs` (`[Category("RequiresAuth")]`)

### Backend implementation for US1

- [x] T030 [US1] Update `POST /api/profile/social/link-callback` to be the authoritative persistence point for **direct provider OAuth**:
  - File: `Visage.Services.Registrations/ProfileApi.cs`
  - Normalize/canonicalize incoming `ProfileUrl` (trim; consider provider-specific canonicalization rules)
  - Add app-level uniqueness check (FR-006) and return `409` ProblemDetails per `contracts/social-profile-linking-api.yaml`
  - On success: set `<Provider>Profile`, `Is<Provider>Verified=true`, `<Provider>VerifiedAt=UtcNow`
  - Write `SocialVerificationEvent` rows for success/failure
- [x] T031 [P] [US1] Update OpenAPI metadata to remove Auth0-claim language and reflect direct OAuth flow  
  File: `Visage.Services.Registrations/ProfileApi.cs` (the `.WithOpenApi(...)` description)
- [x] T032 [P] [US1] Ensure `GET /api/profile/social/status` continues to work and includes timestamps  
  File: `Visage.Services.Registrations/ProfileApi.cs`

### Frontend web host (direct OAuth) for US1

> Note: the current direct OAuth code lives under `Visage.FrontEnd.Web/` and is not part of the web project. US1 must implement direct OAuth inside `Visage.FrontEnd/Visage.FrontEnd.Web/`.

- [x] T040 [P] [US1] Move/recreate OAuth configuration types inside the web project:
  - Create `Visage.FrontEnd/Visage.FrontEnd.Web/Configuration/OAuthOptions.cs`
  - Bind `OAuth` section to these options in `Visage.FrontEnd/Visage.FrontEnd.Web/Program.cs`
- [x] T041 [P] [US1] Move/recreate `DirectOAuthService` inside the web project:
  - Create `Visage.FrontEnd/Visage.FrontEnd.Web/Services/DirectOAuthService.cs`
  - Register it in DI in `Visage.FrontEnd/Visage.FrontEnd.Web/Program.cs`
- [x] T042 [US1] Add session (or equivalent protected state store) for OAuth state + returnUrl:
  - `Visage.FrontEnd/Visage.FrontEnd.Web/Program.cs`: `AddDistributedMemoryCache`, `AddSession`, `UseSession`
  - Ensure state is validated on callback (CSRF protection)
- [x] T043 [US1] Implement direct OAuth start endpoints that redirect to providers:
  - `GET /oauth/linkedin/start?returnUrl=...`
  - `GET /oauth/github/start?returnUrl=...`
  - File: `Visage.FrontEnd/Visage.FrontEnd.Web/Program.cs`
- [x] T044 [US1] Implement direct OAuth callback endpoints:
  - `GET /oauth/linkedin/callback?code=...&state=...`
  - `GET /oauth/github/callback?code=...&state=...`
  - Validate state, exchange code, fetch provider profile URL via `DirectOAuthService`
  - Call Registrations API `POST /api/profile/social/link-callback` **with the current user’s Auth0 access token** (use `HttpContext.GetTokenAsync("access_token")`)
  - Redirect back to the original returnUrl with a safe status query string (e.g., `?social=linkedin&result=success|error`)
  - File: `Visage.FrontEnd/Visage.FrontEnd.Web/Program.cs`
- [ ] T045 [P] [US1] Remove or quarantine the orphaned direct OAuth code under `Visage.FrontEnd.Web/` once the in-project implementation is in place (avoid future confusion)

### Frontend shared UI for US1

- [x] T050 [P] [US1] Update `Visage.FrontEnd/Visage.FrontEnd.Shared/Services/SocialAuthService.cs` to initiate **direct OAuth**:
  - Change `GetLinkedInAuthUrlAsync()` → returns `/oauth/linkedin/start?returnUrl=...`
  - Change `GetGitHubAuthUrlAsync()` → returns `/oauth/github/start?returnUrl=...`
  - Ensure returnUrl points back to `/registration/mandatory` (not the old Auth0-claim callback page)
- [ ] T051 [US1] Decide what to do with the old Auth0 claim-based callback page:
  - Deprecate or delete `Visage.FrontEnd/Visage.FrontEnd.Shared/Pages/OAuthCallback.razor`
  - If kept, ensure it can’t be reached from the Connect buttons
- [x] T052 [US1] Update `Visage.FrontEnd/Visage.FrontEnd.Shared/Components/MandatoryRegistration.razor.cs`:
  - After return from OAuth, refresh social status (`GetSocialStatusAsync`) and populate verified URLs into the registrant model
  - Improve error handling for 409 conflict (show actionable message per FR-009)
- [ ] T053 [P] [US1] Accessibility pass on Connect buttons and Verified display:
  - Ensure links/buttons have correct accessible names
  - Ensure status updates use `aria-live` where appropriate
  - File: `Visage.FrontEnd/Visage.FrontEnd.Shared/Components/MandatoryRegistration.razor`

**Checkpoint**: US1 works end-to-end and can be demonstrated.

---

## Phase 4: User Story 2 — Review, disconnect, and relink (Priority: P2)

**Goal**: Users can disconnect a previously verified profile and relink a different one.

**Independent Test**: Link → disconnect → link another → status updates; mandatory registration blocks again when required profile is disconnected.

### Tests for US2 (mandatory)

- [x] T060 [P] [US2] Add integration test for `POST /api/profile/social/disconnect` clears verification fields and records audit event  
  File: `tests/Visage.Test.Aspire/SocialProfileDisconnectTests.cs` (`[Category("RequiresAuth")]`)

### Backend implementation for US2

- [ ] T070 [US2] Add `POST /api/profile/social/disconnect` endpoint per contract:
  - File: `Visage.Services.Registrations/ProfileApi.cs`
  - Request: `SocialDisconnectDto`
  - Behavior: clear URL + verified flag + timestamp; create `SocialVerificationEvent` row
  - Response: return updated `SocialConnectionStatusDto`
- [x] T071 [P] [US2] Add/extend `.http` examples for disconnect flow  
  File: `Visage.Services.Registrations/app.http`

### Frontend shared UI for US2

- [x] T080 [P] [US2] Extend `ISocialAuthService` + implementation to call disconnect endpoint (no longer just clearing local UI state)  
  File: `Visage.FrontEnd/Visage.FrontEnd.Shared/Services/SocialAuthService.cs`
- [ ] T081 [US2] Update disconnect handlers to:
  - call backend disconnect
  - refresh status
  - show friendly errors
  File: `Visage.FrontEnd/Visage.FrontEnd.Shared/Components/MandatoryRegistration.razor.cs`

**Checkpoint**: US2 works independently and doesn’t break US1.

---

## Phase 5: User Story 3 — Curation visibility (Priority: P3)

**Goal**: Curators can view verified social links and trust them.

**Independent Test**: After linking in US1, the data surfaced for curation includes the verified links + “verified” indicators.

- [ ] T090 [US3] Confirm the curation data path and update it to include verified social links:
  - If using Registrations API listing: `Visage.Services.Registrations/Program.cs` (`GET /register`) already returns `Registrant` including social fields; ensure it is appropriately protected for curator use
  - If there is (or should be) a curator UI: add a minimal read-only page to display registrant social links and verification flags
- [x] T091 [P] [US3] Add an integration test that verifies the curation/list endpoint returns the verified fields after linking  
  File: `tests/Visage.Test.Aspire/RegistrantCurationSocialFieldsTests.cs` (`[Category("RequiresAuth")]` if endpoint is protected)

---

## Phase 6: Polish & cross-cutting concerns

- [ ] T100 [P] Documentation polish:
  - Update `specs/004-verify-social-profiles/quickstart.md` if endpoints/paths changed
  - Ensure `docs/Direct-OAuth-Profile-Verification.md` remains accurate
- [ ] T101 [P] Security hardening:
  - Ensure OAuth state is single-use and time-bounded
  - Ensure returnUrl is validated to prevent open redirects
  - Ensure no provider access tokens are stored/logged
- [ ] T102 [P] Remove or redirect legacy free-form registration page if it’s still reachable:
  - `Visage.FrontEnd/Visage.FrontEnd.Shared/Pages/UserRegistration.razor`
- [ ] T103 Run quickstart validation end-to-end (manual): OAuth connect → verified badge → submit registration

---

## Dependencies & execution order

### Phase dependencies
- **Phase 1 (Setup)**: no dependencies
- **Phase 2 (Foundational)**: blocks all story work
- **US1 (P1)**: depends on Phase 2
- **US2 (P2)**: depends on US1 (because it builds on persistence + status)
- **US3 (P3)**: depends on US1 (because it consumes verified data)
- **Polish**: after whichever stories we choose to ship

### Parallel opportunities
- Most model/DTO additions are [P]
- Most tests are [P] (separate test files)
- Web host endpoints (`Program.cs`) are *not* safe to parallelize heavily—coordinate to avoid merge conflicts

## Implementation strategy

### MVP (ship US1)
1. Phase 1 → Phase 2
2. Implement US1 tests (make them fail)
3. Implement US1 backend + web host + UI
4. Run the US1 test subset and manual quickstart

### Incremental
- Add US2 disconnect + test coverage
- Add US3 curation surfacing
