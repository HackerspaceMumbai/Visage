# Implementation Plan: Registration Flow Optimization with Profile Completion & Smart Redirect

**Branch**: `003-registration-flow-optimization` | **Date**: October 28, 2025 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-registration-flow-optimization/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

**Primary Requirement**: Reduce user registration friction by splitting the monolithic 616-line UserRegistration.razor form into a focused mandatory registration flow (Personal Information) and a separate optional AIDE (Accessibility, Inclusiveness, Diversity, Equity) profile completion flow. Implement smart redirect logic that routes returning users with complete profiles directly to the home page while guiding new users through only the essential fields first.

**Technical Approach**:
1. Create server-side Profile Completion API endpoint in Registrations service that checks mandatory field presence and returns completion status
2. Implement client-side caching (5-minute TTL) for profile status to minimize API calls while maintaining freshness
3. Refactor UserRegistration.razor into separate components: MandatoryRegistration.razor (P1 fields only) and unified ProfileEdit.razor (both sections with visual separation)
4. Add AIDE completion nudging via post-submission success message and dismissible home page banner with server-side dismissal tracking
5. Implement draft registration auto-save with 24-hour server-side retention
6. Update Home.razor routing logic to use new profile completion API instead of Auth0 claims

## Technical Context

**Language/Version**: C# 12 with .NET 10 (pinned in `global.json`)

**Primary Dependencies**:

- Blazor (InteractiveServer render mode for auth-dependent forms)
- Aspire for service orchestration and observability
- Auth0.NET for OIDC authentication (abstracted via interfaces)
- EF Core 10 with PostgreSQL for user profiles and preferences
- DaisyUI 5 + Tailwind CSS 4 for UI components
- FluentValidation for form validation (if not already in use, or DataAnnotations + ValidationMessageStore pattern per constitution)

**Storage**: PostgreSQL database for:

- User profile data (mandatory + AIDE fields)
- Profile completion status tracking
- Draft registrations with 24-hour TTL
- User preferences (AIDE banner dismissal timestamp)

**Testing**:

- Integration tests with TUnit + Fluent Assertions for Profile Completion API
- Playwright E2E tests for registration flow (new user, returning user, partial save scenarios)
- bunit tests for critical Blazor form components (MandatoryRegistration.razor validation)

**Target Platform**: Web (Blazor Server/Hybrid) with future mobile support via existing MAUI infrastructure

**Project Type**: Web application - Frontend (Blazor Hybrid) + Backend (Minimal API service)

**Performance Goals**:

- Profile completion check API: <200ms latency (FR-009, SC-007)
- Client-side cache TTL: 5 minutes to balance freshness and performance
- Post-authentication home page load: <2 seconds (SC-003)
- Mandatory registration completion time: <3 minutes (SC-001)

**Constraints**:

- Must not break existing UserRegistration.razor usage until migration complete
- Must work with Auth0 claims but not tightly couple to Auth0 SDK (IAuthenticationService abstraction per constitution)
- Draft registration data must be secure and auto-expire after 24 hours
- Client-side caching must handle cache invalidation on profile updates

**Scale/Scope**:

- Expected concurrent users during registration surges: 1000+ (SC-002 reference)
- Mandatory fields: 13 (FirstName, LastName, Email, MobileNumber, GovtId, GovtIdLast4Digits, AddressLine1, City, State, PostalCode, OccupationStatus + occupation-specific)
- Optional AIDE fields: ~15-20 accessibility/inclusiveness attributes
- 3 new Blazor components (MandatoryRegistration.razor, ProfileEdit.razor, AideCompletionBanner.razor)
- 1 new Minimal API service or endpoints added to existing Registrations service
- Target 25% voluntary AIDE completion within 30 days (SC-005)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Aspire-First Architecture**: Profile Completion API will be added to existing `Visage.Services.Registrations` project already registered in AppHost with proper database dependencies
- [x] **Minimal API Design**: New `/api/profile/completion-status` endpoint added to `ProfileApi.cs` with Scalar OpenAPI docs; updated `.http` file for testing
- [x] **Integration Testing Priority**: Integration tests planned for Profile Completion API endpoint, E2E Playwright tests for full registration flows (new user, returning user, partial save)
- [x] **Blazor Hybrid UI**: New components (MandatoryRegistration.razor, ProfileEdit.razor, AideCompletionBanner.razor) created in `Visage.FrontEnd.Shared/` for cross-platform reuse
- [x] **Observability**: OpenTelemetry traces for profile completion checks, metrics for registration abandonment, draft save operations logged
- [x] **Security & Privacy**: Profile completion endpoint requires Auth0 JWT with `profile:read-write` scope; input validation via DataAnnotations + custom ValidationMessageStore; draft data encrypted at rest; DPDP-compliant data retention (24-hour auto-delete for drafts)
- [x] **Technology Showcase**: Uses .NET 10 Blazor InteractiveServer render mode, Aspire service discovery for API calls, EF Core 10 with modern Repository pattern, client-side caching pattern
- [x] **Blazor Render Mode Strategy**: InteractiveServer (app-wide default per constitution) appropriate for authenticated forms with server-side validation and Auth0 integration; no render mode mixing across navigation
- [x] **Identity Provider Abstraction**: Profile completion check uses abstracted user claims via `IUserClaimsProvider`; no direct Auth0 SDK calls in ProfileEdit.razor or MandatoryRegistration.razor components
- [x] **DaisyUI Styling**: Form components use existing DaisyUI patterns (collapse panels, badges for required/optional indicators, input/textarea/select styling per `.vscode/daisyui.md`); AideCompletionBanner uses alert component with dismissible behavior

*All checks pass. No complexity violations to justify.*

## Project Structure

### Documentation (this feature)

```text
specs/003-registration-flow-optimization/
├── spec.md              # Feature specification (complete)
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output - Best practices research
├── data-model.md        # Phase 1 output - Database schema design
├── quickstart.md        # Phase 1 output - Developer setup guide
├── contracts/           # Phase 1 output - API contracts
│   └── profile-completion-api.yaml  # OpenAPI spec for profile endpoint
├── checklists/          # Quality gates (existing)
│   └── requirements.md  # Requirement validation (complete)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created yet)
```

### Source Code (repository root)

```text
# Backend: Registrations Service (extend existing)
Visage.Services.Registrations/
├── ProfileApi.cs        # EXTEND: Add /api/profile/completion-status endpoint
├── Program.cs           # UPDATE: Register new repositories and services
├── RegistrantDB.cs      # EXTEND: Add ProfileCompletionStatus, DraftRegistration, UserPreferences entities
├── appsettings.json     # UPDATE: Add draft retention policy config (24h TTL)
└── app.http             # UPDATE: Add test requests for new endpoint

# Shared Models
Visage.Shared/
└── Models/
    ├── ProfileCompletionStatusDto.cs  # NEW: Profile completion response model
    ├── DraftRegistrationDto.cs        # NEW: Draft save/restore model
    └── UserPreferencesDto.cs          # NEW: Banner dismissal preferences

# Frontend: Blazor Shared Components
Visage.FrontEnd.Shared/
├── Components/
│   ├── MandatoryRegistration.razor      # NEW: Focused mandatory-only form
│   ├── MandatoryRegistration.razor.cs   # NEW: Component code-behind
│   ├── MandatoryRegistration.razor.css  # NEW: Component-scoped styles
│   ├── ProfileEdit.razor                # NEW: Unified profile editor (mandatory + AIDE)
│   ├── ProfileEdit.razor.cs             # NEW: Component code-behind
│   ├── ProfileEdit.razor.css            # NEW: Component-scoped styles
│   ├── AideCompletionBanner.razor       # NEW: Dismissible AIDE nudge banner
│   └── AideCompletionBanner.razor.cs    # NEW: Banner logic with dismissal
├── Pages/
│   ├── UserRegistration.razor   # DEPRECATE: Mark as legacy, redirect to MandatoryRegistration
│   └── Home.razor               # UPDATE: Add AideCompletionBanner, use new profile API
├── Services/
│   ├── IProfileService.cs       # NEW: Profile completion check abstraction
│   ├── ProfileService.cs        # NEW: HTTP client for profile API with caching
│   └── DraftRegistrationService.cs  # NEW: Auto-save draft logic
└── Styles/
    └── input.css                # UPDATE: Add banner and form progress indicator styles

# Frontend: Web Project (entry point)
Visage.FrontEnd.Web/
└── Components/
    └── Routes.razor             # UPDATE: Add /profile/edit route for unified profile page

# Database Migrations
Visage.Services.Registrations/Migrations/
└── [Timestamp]_AddProfileCompletionTracking.cs  # NEW: EF Core migration

# Integration Tests
tests/Visage.Tests.Integration/
└── RegistrationFlowTests.cs     # NEW: Profile API, draft save, routing logic tests

# E2E Tests
tests/Visage.E2E.Playwright/
└── RegistrationFlowTests.cs     # NEW: New user flow, returning user redirect, partial save
```

**Structure Decision**: Web application structure (Option 2 from template). This feature extends the existing Blazor Hybrid web app with new frontend components in `Visage.FrontEnd.Shared/` (for cross-platform reuse) and backend endpoints in the existing `Visage.Services.Registrations` Minimal API service. No new projects required; all changes fit within the current Aspire-orchestrated architecture.

## Complexity Tracking

**No Constitution violations.** All implementation aligns with Aspire-First, Minimal API, Integration Testing, Blazor Hybrid, Observability, Security, Render Mode, IdP Abstraction, and DaisyUI styling principles.

## Phase 0: Research (GATE PASSED ✓)

**Status**: Complete - Research tasks documented in [research.md](./research.md)

**Key Research Questions**:

1. Client-side caching strategy for profile completion status (5-minute TTL)
2. PostgreSQL schema design for profile completion tracking
3. Blazor multi-section form validation patterns
4. Auth0 custom claims refresh for profile completion status
5. Draft registration auto-save patterns

**Constitution Check**: ✅ All gates passed before research phase

**Findings**: Research document created with recommended patterns from constitution and best practices. Key decisions:

- Use Blazor ProtectedSessionStorage for client-side caching (secure, auto-expires on session end)
- Use computed columns in PostgreSQL for `IsProfileComplete` flag (efficient queries)
- Use EditForm + DataAnnotationsValidator + ValidationMessageStore pattern (per constitution)
- API-based profile status check (not JWT claims) to maintain IdP abstraction
- Debounced auto-save every 30 seconds for draft registration

## Phase 1: Design & Contracts (GATE PASSED ✓)

**Status**: Complete - Design artifacts created

**Deliverables**:

1. ✅ [data-model.md](./data-model.md): Database schema with ERD, entity definitions, migration strategy
2. ✅ [contracts/profile-completion-api.yaml](./contracts/profile-completion-api.yaml): OpenAPI 3.1 spec for all profile endpoints
3. ✅ [quickstart.md](./quickstart.md): Developer setup guide with environment config, testing scenarios

**Design Decisions**:

- **Database Schema**: Extended `UserProfile` with completion status columns; new `DraftRegistration` and `UserPreferences` tables
- **API Contract**: RESTful endpoints for profile completion check, draft save/restore, banner dismissal
- **Caching Strategy**: 5-minute client-side TTL with manual invalidation on profile updates
- **Migration Strategy**: Backfill existing users with computed profile completion status

**Constitution Re-Check**: ✅ All gates still pass after design phase

## Phase 2: Task Breakdown (PENDING)

**Status**: Not started - Run `/speckit.tasks` command to generate tasks.md

**Estimated Tasks** (will be broken down in detail):

- Backend API implementation (~5-7 tasks)
- Database migrations and schema changes (~3-4 tasks)
- Frontend component development (~8-10 tasks)
- Service layer and caching implementation (~4-5 tasks)
- Integration and E2E test creation (~6-8 tasks)
- Documentation and deployment updates (~2-3 tasks)

**Next Command**: Run `/speckit.tasks` to generate detailed task breakdown with estimates and dependencies

## Agent Context Updates

**Files to Update for AI Agent Context** (before implementation):

1. `.github/copilot-instructions.md`:
   - Add note about profile completion API patterns
   - Document client-side caching strategy (5-min TTL)
   - Reference new ProfileService abstraction

2. `Visage.FrontEnd.Shared/README.md` (create if not exists):
   - Document MandatoryRegistration.razor usage
   - Document ProfileEdit.razor unified profile pattern
   - Document AideCompletionBanner dismissal logic

3. `Visage.Services.Registrations/README.md` (extend):
   - Document profile completion API endpoints
   - Document draft registration retention policy (24h)
   - Document user preferences storage

## Next Steps

### Immediate Actions (Before Implementation)

1. **Run `/speckit.tasks`** to generate detailed task breakdown
2. **Validate Research Findings**: Review [research.md](./research.md) and confirm best practices with team
3. **Review API Contract**: Validate [contracts/profile-completion-api.yaml](./contracts/profile-completion-api.yaml) with backend team
4. **Test Database Schema**: Review [data-model.md](./data-model.md) and validate entity relationships

### Development Phase

1. **Setup Development Environment**: Follow [quickstart.md](./quickstart.md)
2. **Create Feature Branch**: `git checkout -b 003-registration-flow-optimization` (already done)
3. **Implement Tasks**: Work through tasks.md in priority order (P1 → P2 → P3)
4. **Run Tests**: Execute integration and E2E tests after each major component
5. **Update Documentation**: Keep quickstart.md and README files current

### Quality Gates (Before Merge)

- [ ] All integration tests pass (Profile API, draft save, routing)
- [ ] All E2E tests pass (new user flow, returning user redirect, AIDE banner)
- [ ] Scalar OpenAPI docs updated and accessible
- [ ] `.http` file includes all new endpoints with examples
- [ ] Constitution compliance re-verified (all 10 gates pass)
- [ ] Code review completed (focus on IdP abstraction, security, caching)
- [ ] Performance tested (profile completion check <200ms, registration <3min)

### Post-Implementation

- [ ] Deploy to staging environment
- [ ] User acceptance testing (UAT) with real Auth0 users
- [ ] Monitor telemetry (registration abandonment rate, AIDE completion rate)
- [ ] Iterate based on SC metrics (SC-001 to SC-007)
- [ ] Document lessons learned in [research.md](./research.md)

## Risk Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Cache invalidation failures cause stale profile status | High - Users with complete profiles may be routed to registration | Implement aggressive cache invalidation on all profile updates; add cache version header |
| Draft auto-save overwhelms API with requests | Medium - Increased server load during registration surges | Use 30-second debounce; implement rate limiting on draft endpoint |
| Auth0 custom claims not refreshing after profile update | Medium - Users may see incorrect completion status until re-login | Use API-based status check (not JWT claims); cache client-side only |
| Existing UserRegistration.razor usage breaks during migration | High - Disrupts existing users mid-registration | Feature flag new registration flow; gradual rollout; keep legacy form as fallback |
| 24-hour draft retention not sufficient for all users | Low - Users may lose progress if they return after 24h | Monitor draft expiration metrics; extend TTL if needed (configurable) |

## Success Metrics (Post-Launch)

Track these metrics to validate success criteria from [spec.md](./spec.md):

- **SC-001**: Registration completion time (target: <3 minutes) → Monitor via Application Insights
- **SC-002**: Registration abandonment rate (target: 40% reduction) → Track incomplete registrations in DB
- **SC-003**: Post-auth home page load time (target: <2 seconds) → Monitor via Playwright performance tests
- **SC-004**: First-attempt validation success rate (target: 90%) → Track form submission failures
- **SC-005**: AIDE profile voluntary completion rate (target: 25% within 30 days) → Track `IsAideProfileComplete` flag
- **SC-006**: Draft data loss incidents (target: 0 within 24h window) → Monitor failed draft restores
- **SC-007**: Profile completion API latency (target: <200ms) → Monitor via OpenTelemetry traces

## Related Specifications

- **Predecessor**: None (foundational registration flow improvement)
- **Successor**: TBD - Event curation logic using AIDE profile data (deferred from this spec)
- **Dependencies**: Auth0 authentication (existing), Registrations service (existing)

---

**Plan Version**: 1.0.0  
**Last Updated**: October 28, 2025  
**Status**: Ready for Task Breakdown (`/speckit.tasks`)
