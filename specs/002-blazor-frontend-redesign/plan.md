# Implementation Plan: Blazor Hybrid Frontend with Events Display

**Branch**: `002-blazor-frontend-redesign` | **Date**: October 20, 2025 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/002-blazor-frontend-redesign/spec.md`

## Summary

Enhance the existing Blazor Hybrid frontend to display upcoming and past events on the homepage with responsive design and brand consistency. The implementation will add new Razor components to the shared UI library (`Visage.FrontEnd.Shared`), style them with DaisyUI to match the Hackerspace Mumbai website color palette, and integrate with existing backend event services. The approach leverages the current Blazor Hybrid architecture (Web + MAUI) to maximize code reuse while meeting performance targets (< 2s initial load, 90+ Lighthouse score).

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (pinned in global.json)  
**Primary Dependencies**: 
- Blazor Web + WebAssembly (Hybrid mode)
- DaisyUI 5.0 + Tailwind CSS 4.0
- Auth0.AspNetCore.Authentication (abstracted via interfaces)
- Microsoft.AspNetCore.Components.WebAssembly.Server
- EF Core 10 for data access

**Storage**: SQL Server (via existing `Visage.Services.Eventing` database)  
**Testing**: 
- Integration: TUnit + Fluent Assertions
- E2E: Playwright for user journeys
- Component: bunit for critical Blazor components only
- Load: NBomber for performance validation

**Target Platform**: 
- Web: Modern browsers (Chrome, Firefox, Safari, Edge - last 2 versions)
- Mobile: iOS 15+, Android 24+ via .NET MAUI

**Project Type**: Blazor Hybrid (Web + Mobile with shared UI)  
**Performance Goals**: 
- Initial page load < 2 seconds
- Interactive < 100ms perceived latency
- Lighthouse Performance: 90+ (desktop), 80+ (mobile)
- API response times < 200ms p95

**Constraints**: 
- WCAG 2.1 AA compliance (4.5:1 text contrast, 3:1 UI contrast)
- 44x44px minimum touch targets
- No horizontal scroll 320px-1920px
- Progressive enhancement (usable without JS for core content)

**Scale/Scope**: 
- Expected users: 2,500+ active community members
- Events displayed: ~10-20 upcoming, 100+ past (with pagination)
- Concurrent users during registration: 1000+
- Target devices: 60% mobile, 30% desktop, 10% tablet

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Aspire-First Architecture**: Frontend already registered in AppHost (`Visage.FrontEnd.Web` and `Visage.FrontEnd.Web.Client`), no new service registration needed
- [x] **Minimal API Design**: N/A - This is a frontend feature consuming existing backend APIs
- [x] **Integration Testing Priority**: Integration tests planned via Playwright for user journeys, bunit for critical components only
- [x] **Blazor Hybrid UI**: All new components will be in `Visage.FrontEnd.Shared/` for reuse across Web and MAUI platforms
- [x] **Observability**: Frontend telemetry via existing ServiceDefaults integration (browser telemetry, performance metrics)
- [x] **Security & Privacy**: Event viewing is public (no auth required); RSVP links redirect to existing auth-protected endpoints
- [x] **Technology Showcase**: Uses .NET 10 Blazor Hybrid, InteractiveServer/Auto render modes, modern CSS (Tailwind/DaisyUI)
- [x] **Blazor Render Mode Strategy**: 
  - Static SSR for event cards (public content, SEO-friendly)
  - InteractiveServer for event details with RSVP (requires auth integration)
  - InteractiveAuto for homepage with filters (fast initial load + rich interactivity)
- [x] **Identity Provider Abstraction**: Auth integration uses existing abstracted `IAuthenticationService` from ServiceDefaults
- [x] **DaisyUI Styling**: All components will use DaisyUI classes for consistency with Hackerspace Mumbai website (colors: #FFC107 primary, #4DB6AC secondary, #7986CB accent)

*All checks passed. No constitution violations.*

## Project Structure

### Documentation (this feature)

```text
specs/002-blazor-frontend-redesign/
├── spec.md              # Feature specification (completed)
├── plan.md              # This file (implementation plan)
├── research.md          # Phase 0: DaisyUI integration, render mode patterns, performance optimization
├── data-model.md        # Phase 1: Event entity model, view models for UI
├── quickstart.md        # Phase 1: Developer setup guide for frontend changes
├── contracts/           # Phase 1: API contracts for event endpoints (reference only - backend exists)
└── checklists/
    └── requirements.md  # Specification quality validation (completed)
```

### Source Code (existing Blazor Hybrid structure - will be enhanced)

```text
Visage.FrontEnd/
├── Visage.FrontEnd.Shared/          # ✨ PRIMARY WORK AREA - New components here
│   ├── Components/
│   │   └── Events/                   # NEW: Event display components
│   │       ├── EventCard.razor       # NEW: Individual event card component
│   │       ├── EventList.razor       # NEW: List of events (upcoming/past)
│   │       ├── EventGrid.razor       # NEW: Responsive grid layout
│   │       └── EmptyState.razor      # NEW: No events placeholder
│   ├── Pages/
│   │   └── Home.razor                # ENHANCED: Add event sections
│   ├── Models/
│   │   └── EventViewModel.cs         # NEW: View model for event display
│   └── Styles/
│       └── daisy-theme.css           # NEW: DaisyUI theme with Hackerspace colors
│
├── Visage.FrontEnd.Web/              # Web host (minimal changes)
│   ├── Program.cs                    # VERIFY: Render mode configuration
│   ├── Components/
│   │   └── App.razor                 # VERIFY: DaisyUI script/style references
│   └── wwwroot/
│       └── css/
│           └── app.css               # ENHANCED: Tailwind/DaisyUI config
│
├── Visage.FrontEnd.Web.Client/       # WebAssembly client (minimal changes)
│   └── Program.cs                    # VERIFY: Client-side service registration
│
└── Visage.FrontEnd/                  # MAUI mobile (benefits from shared components)
    └── (inherits from Shared)        # No changes needed - reuses shared UI

tests/
└── Visage.Test.Aspire/               # ENHANCED: Add frontend integration tests
    ├── FrontEndHomeTests.cs          # ENHANCED: Add event display tests
    └── EventComponentTests.cs        # NEW: bunit tests for critical components
```

**Structure Decision**: **Enhance existing Blazor Hybrid projects** rather than create new ones.

**Rationale**:

1. **Follows Constitution Principle IV** - Maintains single codebase for web and mobile
2. **Minimizes Complexity** - No additional projects; work in `Visage.FrontEnd.Shared`
3. **Maximizes Reuse** - All new components automatically available to both Web and MAUI
4. **Leverages Infrastructure** - Auth0, ServiceDefaults, AppHost integration already configured
5. **.NET Best Practice** - Blazor Hybrid pattern is recommended for shared UI across platforms

## Complexity Tracking

**No constitution violations** - All quality gates passed.

The implementation follows all Visage constitution principles:

- Uses existing Blazor Hybrid architecture (no new projects)
- Shared components in `Visage.FrontEnd.Shared` for maximum reuse
- Integration tests prioritized over unit tests
- DaisyUI styling for brand consistency and accessibility
- Appropriate render modes selected for security and performance

---

## Phase 0: Research & Technical Decisions

**Status**: ✅ **COMPLETE**  
**Output**: [`research.md`](./research.md) - Comprehensive technical research

### Static SSR Fallback Strategy (NFR-003 Implementation)

**Objective**: Ensure core content (upcoming events) is viewable without JavaScript

**Implementation**:
- **EventCard.razor**: Uses Static SSR by default - renders HTML with event name, date, location, image, and description on server
- **Home.razor**: Uses `@rendermode InteractiveAuto` with SSR fallback - initial render is Static SSR (shows event list), then hydrates to WebAssembly for interactivity
- **Degraded Experience**: Without JavaScript, users can view all upcoming events and click through to event details (if using standard `<a>` links). RSVP buttons, filtering, search, and pagination require JavaScript (acceptable tradeoff per Constitution Principle VIII)
- **SEO Benefit**: Static SSR ensures search engines can crawl and index event content
- **Validation**: Task T081 will verify SSR-only mode by disabling JavaScript in browser and confirming upcoming events list renders correctly

### Key Decisions Made

1. **DaisyUI 5 + Tailwind CSS 4 Integration**: Use CDN for development, build-time compilation for production with custom Hackerspace color palette in `tailwind.config.js`

2. **Blazor Render Mode Strategy**: 
   - **InteractiveAuto** for homepage (fast initial SSR + WebAssembly interactivity)
   - **Static SSR** for public event detail pages (SEO + fastest load)
   - **InteractiveServer** for authenticated pages (security compliance)

3. **Responsive Grid Patterns**: DaisyUI responsive utilities with 1/2/3/4 column layouts at mobile/tablet/desktop/wide breakpoints, WCAG 2.1 compliant touch targets

4. **Performance Optimization**: 
   - Virtualization with `<Virtualize>` component for past events (85% DOM reduction)
   - Lazy loading with `loading="lazy"` and Cloudinary `f_auto,q_auto` transformations (90% bandwidth reduction)
   - In-memory caching with 5min TTL for upcoming events, 1hr for past events
   - Target: < 2s page load, 90+ Lighthouse desktop score

5. **Existing Event API Review**: Current `Visage.Services.Eventing` API provides all needed endpoints. No backend changes required.
   - GET /events/upcoming
   - GET /events/past?page={page}&pageSize={pageSize}
   - GET /events/{id}

**All research findings validated against Constitution principles and NFR-001 through NFR-005 requirements.**

---

## Phase 1: Data Model & Contracts

**Status**: ✅ **COMPLETE**  
**Outputs**: Data model, API contracts, quickstart guide, updated Copilot context

### 1.1 Data Model Design

**File**: [`data-model.md`](./data-model.md) ✅

**Entities Defined**:
- `Event` (backend entity with StrictId)
- `EventViewModel` (frontend record with computed properties for UI binding)
- `EventListViewModel` (container with pagination metadata)

**Key Decisions**:
- Use C# record types for immutable view models
- Factory method pattern (`FromEvent`) for entity-to-ViewModel mapping
- Computed properties for formatted dates, optimized image URLs, status checks
- Validation enforced at backend; minimal client-side validation for UX

### 1.2 API Contracts

**File**: [`contracts/api-contracts.md`](./contracts/api-contracts.md) ✅

**Endpoints Documented**:
1. **GET /events/upcoming** - Returns upcoming events ordered by date (< 50ms)
2. **GET /events/past?page={page}&pageSize={pageSize}** - Paginated past events (< 100ms)
3. **GET /events/{id}** - Event details by ID (< 30ms)

**DTOs**: `Event`, `PaginatedResult<T>`  
**Service Discovery**: Aspire-based resolution via `https://eventing`  
**Error Handling**: Standardized JSON error responses with fallback strategies  
**Testing**: Integration test examples provided

### 1.3 Developer Quickstart Guide

**File**: [`quickstart.md`](./quickstart.md) ✅

**Sections**:
- Prerequisites and environment setup (15-20 min)
- DaisyUI/Tailwind CSS configuration (CDN for dev, build-time for prod)
- Database seeding with test data (10 upcoming + 50 past events)
- Integration and E2E test execution
- Troubleshooting guide (blazor.web.js 404, empty results, Aspire issues)
- Development workflow and useful commands

### 1.4 Agent Context Update

**Status**: ✅ **COMPLETE**

**Script Executed**: `.specify/scripts/powershell/update-agent-context.ps1 -AgentType copilot`

**Updates Applied**:
- Added C# 14 / .NET 10.0 to language context
- Added SQL Server database reference
- Added Blazor Hybrid project type
- Updated `.github/copilot-instructions.md` with new technologies

**Result**: GitHub Copilot now aware of DaisyUI 5, Tailwind CSS 4, Blazor render mode strategies

---

## Phase 2: Task Breakdown & Implementation

**Status**: ⏸️ **PENDING** (requires separate `/speckit.tasks` command)

**Next Steps**:
1. Run `.specify/scripts/powershell/create-tasks.ps1` to generate task breakdown
2. Implementation will create:
   - `EventViewModel.cs`, `EventListViewModel.cs` in Visage.FrontEnd.Shared/Models/
   - `EventService.cs` in Visage.FrontEnd.Shared/Services/
   - `EventCard.razor`, `EventList.razor`, `EventGrid.razor`, `EmptyState.razor` in Components/Events/
   - Enhanced `Home.razor` with event sections
   - `daisy-theme.css` with Hackerspace color palette
   - Integration tests in Visage.Test.Aspire/
   - Playwright E2E tests

**Complexity Estimate**: ~8-12 hours of development time across 4-6 components and 15-20 test cases

---

## Summary: Phase 0-1 Complete ✅

### Artifacts Created

| Artifact | Purpose | Status |
|----------|---------|--------|
| [`research.md`](./research.md) | Technical research findings and decisions | ✅ Complete |
| [`data-model.md`](./data-model.md) | Entity and view model specifications | ✅ Complete |
| [`contracts/api-contracts.md`](./contracts/api-contracts.md) | API endpoint documentation | ✅ Complete |
| [`quickstart.md`](./quickstart.md) | Developer setup and workflow guide | ✅ Complete |
| `.github/copilot-instructions.md` | Updated agent context | ✅ Complete |
| [`plan.md`](./plan.md) (this file) | Complete implementation plan | ✅ Complete |

### Key Technical Decisions

1. **Styling**: DaisyUI 5 + Tailwind CSS 4 with Hackerspace color palette (#FFC107, #4DB6AC, #7986CB, #1A1A1A)
2. **Render Modes**: InteractiveAuto (homepage), Static SSR (details), InteractiveServer (auth pages)
3. **Performance**: Virtualization (85% DOM reduction), lazy loading (90% bandwidth reduction), caching (5min/1hr TTL)
4. **Architecture**: Enhance existing Blazor Hybrid projects (follows Constitution Principle IV)
5. **API Integration**: Use existing Eventing service endpoints with no backend changes required

### Constitution Compliance

✅ All 10 constitution principles validated:
- ✅ Modular architecture (Principle I)
- ✅ Data-driven design (Principle II)
- ✅ No new projects created (Principle IV)
- ✅ Blazor render mode strategy applied (Principle VIII)
- ✅ IdP abstraction maintained (Principle IX)
- ✅ DaisyUI styling for consistency (Approved Patterns)
- ✅ Integration tests prioritized (Quality Gates)
- ✅ Performance targets defined (< 2s load, 90+ Lighthouse)

