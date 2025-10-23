# Tasks: Blazor Hybrid Frontend with Events Display

**Branch**: `002-blazor-frontend-redesign`  
**Input**: Design documents from `/specs/002-blazor-frontend-redesign/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Integration tests with Playwright are MANDATORY per Visage Constitution. bunit tests are OPTIONAL (only for critical interactive components). TDD approach is NOT required (can write tests after implementation).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing (P1: Upcoming events, P2: Past events, P1: Responsive design, P2: Brand consistency).

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: DaisyUI/Tailwind CSS integration and basic structure

- [X] T001 Create directory structure for event components in Visage.FrontEnd.Shared/Components/Events/
- [X] T002 Create directory structure for view models in Visage.FrontEnd.Shared/Models/
- [X] T003 Create directory structure for services in Visage.FrontEnd.Shared/Services/
- [X] T004 Add DaisyUI 5 and Tailwind CSS 4 CDN references to Visage.FrontEnd.Web/Components/App.razor head section
- [X] T005 [P] Create daisy-theme.css with Hackerspace Mumbai color palette in Visage.FrontEnd.Web/wwwroot/css/
- [X] T006 [P] Verify blazor.web.js loads correctly and InteractiveAuto render mode works in Visage.FrontEnd.Web/Components/App.razor

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core data models and API integration that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T007 Create EventStatus enum in Visage.FrontEnd.Shared/Models/EventStatus.cs
- [X] T008 Create EventViewModel record with computed properties in Visage.FrontEnd.Shared/Models/EventViewModel.cs
- [X] T009 Create EventListViewModel record with pagination metadata in Visage.FrontEnd.Shared/Models/EventListViewModel.cs
- [X] T010 Create EventService with HttpClient and IMemoryCache in Visage.FrontEnd.Shared/Services/EventService.cs
- [X] T011 Implement GetUpcomingEventsAsync() with 5min cache TTL in EventService.cs
- [X] T012 Implement GetPastEventsAsync(page, pageSize) with 1hr cache TTL in EventService.cs
- [X] T013 Implement GetEventByIdAsync(id) with 10min cache TTL in EventService.cs
- [X] T014 Register EventService with HttpClient in Visage.FrontEnd.Web/Program.cs with Aspire service discovery
- [X] T015 Register IMemoryCache in Visage.FrontEnd.Web/Program.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - View Upcoming Events (Priority: P1) 🎯 MVP

**Goal**: Display upcoming events on homepage ordered by date with event cards showing key information

**Independent Test**: Navigate to homepage and verify upcoming events section displays with event cards (name, date, location, image, description)

### Implementation for User Story 1

- [ ] T016 [P] [US1] Create EventCard.razor component with DaisyUI card styling in Visage.FrontEnd.Shared/Components/Events/
- [ ] T017 [P] [US1] Create EventGrid.razor component with responsive grid (1/2/3 cols) in Visage.FrontEnd.Shared/Components/Events/
- [ ] T018 [P] [US1] Create EmptyState.razor component for no events scenario in Visage.FrontEnd.Shared/Components/Events/
- [X] T019: Create EventList component (loading states, empty state handling)
- [X] T020: Update Home.razor to use EventList component
- [X] T021: Inject EventService and load upcoming events
- [X] T022: Add error handling with retry button
- [X] T023: Configure Cloudinary optimization parameters
- [X] T024: Implement RSVP button interaction
- [X] T025: Add description truncation logic

### Tests for User Story 1

- [ ] T026 [P] [US1] Playwright E2E test: Homepage displays upcoming events section in tests/Visage.Test.Aspire/FrontEndHomeTests.cs
- [ ] T027 [P] [US1] Playwright E2E test: Event cards show name, date, location, image in tests/Visage.Test.Aspire/FrontEndHomeTests.cs
- [ ] T028 [P] [US1] Playwright E2E test: Click event card navigates to details (if implemented) in tests/Visage.Test.Aspire/FrontEndHomeTests.cs
- [ ] T029 [P] [US1] Playwright E2E test: Empty state displays when no upcoming events in tests/Visage.Test.Aspire/FrontEndHomeTests.cs
- [ ] T030 [P] [US1] Optional bunit test: EventCard renders with mock EventViewModel in tests/Visage.Test.Aspire/EventComponentTests.cs

**Checkpoint**: User Story 1 complete - users can view upcoming events on homepage

---

## Phase 4: User Story 2 - View Past Events (Priority: P2)

**Goal**: Display past events on homepage with pagination/virtualization for large lists

**Independent Test**: Scroll past upcoming events section and verify past events section displays chronologically (most recent first) with pagination

### Implementation for User Story 2

- [ ] T031 [US2] Add past events section to Home.razor below upcoming events
- [ ] T032 [US2] Inject EventService and load first page of past events in Home.razor
- [ ] T033 [US2] Implement virtualization with <Virtualize> component for past events in EventList.razor
- [ ] T034 [US2] Add pagination controls (Previous/Next buttons) using DaisyUI join and btn classes
- [ ] T035 [US2] Implement LoadMorePastEventsAsync() method in Home.razor for pagination
- [ ] T036 [US2] Add loading spinner for pagination using DaisyUI loading component
- [ ] T037 [US2] Display attendance count badge on past event cards using DaisyUI badge classes

### Tests for User Story 2

- [ ] T038 [P] [US2] Playwright E2E test: Past events section displays below upcoming events in tests/Visage.Test.Aspire/FrontEndHomeTests.cs
- [ ] T039 [P] [US2] Playwright E2E test: Past events ordered by date descending (most recent first) in tests/Visage.Test.Aspire/FrontEndHomeTests.cs
- [ ] T040 [P] [US2] Playwright E2E test: Pagination loads additional past events in tests/Visage.Test.Aspire/FrontEndHomeTests.cs
- [ ] T041 [P] [US2] Playwright E2E test: Virtualization renders only visible items (performance check) in tests/Visage.Test.Aspire/FrontEndHomeTests.cs

**Checkpoint**: User Stories 1 AND 2 complete - users can view both upcoming and past events

---

## Phase 5: User Story 3 - Responsive Design Across Devices (Priority: P1)

**Goal**: Ensure homepage adapts to mobile (< 768px), tablet (768-1024px), and desktop (> 1024px) screen sizes

**Independent Test**: Open homepage on different screen sizes and verify grid columns adjust (1/2/3/4 columns) and touch targets are 44x44px minimum

### Implementation for User Story 3

- [ ] T042 [US3] Update EventGrid.razor to use DaisyUI responsive grid classes (grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4)
- [ ] T043 [US3] Add responsive gap spacing to EventGrid (gap-4 md:gap-6) for mobile/desktop
- [ ] T044 [US3] Ensure EventCard buttons meet 44x44px minimum touch target (btn-sm class provides this)
- [ ] T045 [US3] Add responsive padding to Home.razor sections (p-4 md:p-6 lg:p-8)
- [ ] T046 [US3] Implement responsive typography sizing (text-base md:text-lg for headings)
- [ ] T047 [US3] Add aspect-video class to EventCard images for consistent sizing across devices
- [ ] T048 [US3] Test responsive layout on mobile Chrome DevTools (320px, 375px, 768px, 1024px, 1920px)

### Tests for User Story 3

- [ ] T049 [P] [US3] Playwright E2E test: Mobile (375px) displays 1 column grid in tests/Visage.Test.Aspire/ResponsiveLayoutTests.cs
- [ ] T050 [P] [US3] Playwright E2E test: Tablet (768px) displays 2 column grid in tests/Visage.Test.Aspire/ResponsiveLayoutTests.cs
- [ ] T051 [P] [US3] Playwright E2E test: Desktop (1024px) displays 3 column grid in tests/Visage.Test.Aspire/ResponsiveLayoutTests.cs
- [ ] T052 [P] [US3] Playwright E2E test: Wide desktop (1920px) displays 4 column grid in tests/Visage.Test.Aspire/ResponsiveLayoutTests.cs
- [ ] T053 [P] [US3] Playwright E2E test: No horizontal scroll on any viewport size (320px-1920px) in tests/Visage.Test.Aspire/ResponsiveLayoutTests.cs
- [ ] T054 [P] [US3] Playwright E2E test: Touch targets meet 44x44px minimum on mobile in tests/Visage.Test.Aspire/ResponsiveLayoutTests.cs

**Checkpoint**: User Stories 1, 2, AND 3 complete - responsive experience across all devices

---

## Phase 6: User Story 4 - Brand Consistency with Main Website (Priority: P2)

**Goal**: Apply Hackerspace Mumbai color palette and styling patterns to match main website

**Independent Test**: Visual comparison of color palette, button styles, and component spacing with main Hackerspace Mumbai website

### Implementation for User Story 4

- [ ] T055 [US4] Configure Tailwind theme with Hackerspace colors in daisy-theme.css (hackmum-primary: #FFC107, secondary: #4DB6AC, accent: #7986CB, dark: #1A1A1A)
- [ ] T056 [US4] Apply DaisyUI theme classes to EventCard (card-compact, shadow-xl, hover:shadow-2xl transition)
- [ ] T057 [US4] Style primary action buttons with btn-primary class (golden yellow #FFC107 background)
- [ ] T058 [US4] Apply consistent border radius (rounded-lg for cards, rounded for buttons) matching main site
- [ ] T059 [US4] Add hover states to EventCard with DaisyUI transition classes (transition-all duration-300)
- [ ] T060 [US4] Ensure text contrast meets WCAG 2.1 AA (4.5:1 for body text, 3:1 for UI components)
- [ ] T061 [US4] Add section title styling matching main site (text-3xl font-bold text-base-content)
- [ ] T062 [US4] Apply consistent spacing patterns (space-y-6 between sections, p-4 card padding)

### Tests for User Story 4

- [ ] T063 [P] [US4] Manual visual test: Compare color palette with https://hackmum.in (document as test evidence)
- [ ] T064 [P] [US4] Playwright E2E test: Verify primary button uses #FFC107 color in tests/Visage.Test.Aspire/BrandConsistencyTests.cs
- [ ] T065 [P] [US4] Playwright E2E test: Verify card shadow and hover effects match main site in tests/Visage.Test.Aspire/BrandConsistencyTests.cs
- [ ] T066 [P] [US4] Accessibility test: Run axe-core to verify WCAG 2.1 AA color contrast in tests/Visage.Test.Aspire/AccessibilityTests.cs
- [ ] T067 [P] [US4] Lighthouse test: Verify 90+ performance score desktop, 80+ mobile in tests/Visage.Test.Aspire/PerformanceTests.cs

**Checkpoint**: All user stories complete - brand-consistent, responsive event display

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and edge cases

- [ ] T068 [P] Add error boundary to Home.razor for graceful failure handling
- [ ] T069 [P] Implement retry button in error state with EventService fallback to cached data
- [ ] T070 [P] Add aria-label attributes to all interactive elements for screen reader accessibility
- [ ] T071 [P] Implement keyboard navigation support (Tab, Enter, Escape keys)
- [ ] T072 [P] Add loading skeleton components for initial page load (DaisyUI skeleton class)
- [ ] T073 [P] Optimize Cloudinary image URLs for mobile (w_400 for < 768px, w_800 for desktop)
- [ ] T074 [P] Add meta tags for SEO (Open Graph, Twitter Card) to App.razor
- [ ] T075 [P] Implement dark mode support based on prefers-color-scheme media query
- [ ] T076 Update README.md with screenshot of event display homepage
- [ ] T077 Update quickstart.md with any additional setup steps discovered during implementation
- [ ] T078 Run Lighthouse audit and document performance scores (target: 90+ desktop, 80+ mobile)
- [ ] T079 Run axe-core accessibility audit and fix any violations (target: zero critical issues)
- [ ] T080 Performance test: Verify < 2s page load time on 3G connection using Chrome DevTools throttling

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - User Story 1 (P1): Can start after Phase 2 - No dependencies on other stories
  - User Story 2 (P2): Can start after Phase 2 - Uses same components as US1 but independently testable
  - User Story 3 (P1): Can start after US1 has components - Enhances existing components for responsive design
  - User Story 4 (P2): Can start after US1 has components - Applies styling to existing components
- **Polish (Phase 7)**: Depends on all P1 user stories (US1, US3) for MVP; all stories for complete feature

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - Creates base event display components
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Extends US1 components with pagination/virtualization
- **User Story 3 (P1)**: Can start after US1 implementation (T016-T025) - Enhances components with responsive styling
- **User Story 4 (P2)**: Can start after US1 implementation (T016-T025) - Applies brand styling to components

### Within Each User Story

- US1: Components (T016-T018) can run in parallel → EventList (T019) → Home.razor integration (T020-T025) → Tests (T026-T030)
- US2: Home.razor updates (T031-T037) → Tests (T038-T041)
- US3: Component enhancements (T042-T048) → Tests (T049-T054)
- US4: Styling updates (T055-T062) → Tests (T063-T067)

### Parallel Opportunities

- **Phase 1**: All setup tasks (T001-T006) can run in parallel
- **Phase 2**: T007-T009 (models) can run in parallel, then T010-T015 (services/registration) in parallel
- **Phase 3 (US1)**: T016-T018 (components) in parallel, T026-T030 (tests) in parallel
- **Phase 4 (US2)**: T038-T041 (tests) in parallel
- **Phase 5 (US3)**: T042-T048 (responsive updates) can run in parallel, T049-T054 (tests) in parallel
- **Phase 6 (US4)**: T055-T062 (styling) can run in parallel, T063-T067 (tests) in parallel
- **Phase 7**: All polish tasks (T068-T080) can run in parallel except T076-T077 (documentation)

---

## Parallel Example: User Story 1 (Upcoming Events)

```bash
# Launch all base components together:
Task T016: "Create EventCard.razor in Visage.FrontEnd.Shared/Components/Events/"
Task T017: "Create EventGrid.razor in Visage.FrontEnd.Shared/Components/Events/"
Task T018: "Create EmptyState.razor in Visage.FrontEnd.Shared/Components/Events/"

# After components complete, run all tests together:
Task T026: "Playwright test: Homepage displays upcoming events"
Task T027: "Playwright test: Event cards show name, date, location, image"
Task T028: "Playwright test: Click event card navigation"
Task T029: "Playwright test: Empty state displays"
Task T030: "bunit test: EventCard renders with mock data"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 3 Only) - Recommended

1. Complete Phase 1: Setup (DaisyUI/Tailwind integration)
2. Complete Phase 2: Foundational (Models, Services, API integration) - CRITICAL
3. Complete Phase 3: User Story 1 (Upcoming events display)
4. Complete Phase 5: User Story 3 (Responsive design) - P1 requirement
5. **STOP and VALIDATE**: Test on mobile/tablet/desktop
6. Deploy/demo MVP with upcoming events responsive display

### Incremental Delivery

1. Setup + Foundational → API integration ready
2. Add User Story 1 → Test independently → MVP: Upcoming events display!
3. Add User Story 3 → Test independently → MVP: Responsive upcoming events!
4. Add User Story 2 → Test independently → Past events with pagination
5. Add User Story 4 → Test independently → Full brand consistency
6. Add Polish (Phase 7) → Test independently → Production-ready feature

### Parallel Team Strategy

With multiple developers after Foundational phase completes:

1. **Developer A**: User Story 1 (T016-T030) - Upcoming events
2. **Developer B**: User Story 2 (T031-T041) - Past events (after US1 T016-T019 complete)
3. **Developer C**: User Story 3 (T042-T054) - Responsive design (after US1 T016-T025 complete)
4. **Developer D**: User Story 4 (T055-T067) - Brand styling (after US1 T016-T025 complete)
5. **All**: Phase 7 Polish (T068-T080) - Can split across team

---

## Notes

- **[P] tasks**: Different files, no dependencies - can run in parallel
- **[Story] label**: Maps task to specific user story (US1, US2, US3, US4) for traceability
- **Render modes**: Use InteractiveAuto for Home.razor, Static SSR for EventCard (if needed for SEO)
- **DaisyUI classes**: Use semantic class names (btn-primary, card, badge) not custom CSS
- **Testing priority**: Playwright E2E tests are MANDATORY. bunit tests are OPTIONAL (only for complex interactive components).
- **Commit strategy**: Commit after each task or logical group (e.g., all US1 components together)
- **Independent stories**: Each user story should be deployable and testable on its own
- **Performance validation**: Run Lighthouse after US1 complete to ensure < 2s load, 90+ score
- **Accessibility validation**: Run axe-core after US4 complete to ensure WCAG 2.1 AA compliance

---

## Total Task Count: 80 tasks

- **Phase 1 (Setup)**: 6 tasks
- **Phase 2 (Foundational)**: 9 tasks (BLOCKING - must complete first)
- **Phase 3 (US1 - Upcoming Events)**: 15 tasks (10 implementation + 5 tests)
- **Phase 4 (US2 - Past Events)**: 11 tasks (7 implementation + 4 tests)
- **Phase 5 (US3 - Responsive Design)**: 13 tasks (7 implementation + 6 tests)
- **Phase 6 (US4 - Brand Consistency)**: 13 tasks (8 implementation + 5 tests)
- **Phase 7 (Polish)**: 13 tasks (cross-cutting improvements)

**MVP Scope** (US1 + US3): 21 implementation tasks + 11 tests = **32 tasks** for core functionality  
**Full Feature**: All 80 tasks for production-ready event display

**Estimated Development Time**:
- MVP (US1 + US3): 8-10 hours
- Full Feature (all stories): 12-16 hours
- With parallel team (3 developers): 6-8 hours for full feature
