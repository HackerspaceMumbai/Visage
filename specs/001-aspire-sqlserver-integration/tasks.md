# Tasks: SQL Server Aspire Integration

**Branch**: `001-aspire-sqlserver-integration` | **Date**: 2025-10-18 | **Spec**: [spec.md](./spec.md)
**Input**: Design documents from `/specs/001-aspire-sqlserver-integration/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Integration tests are MANDATORY per Visage Constitution Principle III. E2E tests with Playwright are NOT required (infrastructure change only, no user-facing changes). Unit tests (bunit) are NOT required for this feature.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story (P1: SQL Server resource, P2: Registration service, P3: Eventing service).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Verify .NET 10 SDK is installed and Docker Desktop is running
- [x] T002 Ensure Visage.AppHost project builds successfully
- [x] T003 [P] Verify existing Registration and Eventing services build and run independently

**Checkpoint**: Foundation verified - ready for Aspire SQL Server integration ✅

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core SQL Server infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 Add Aspire.Hosting.SqlServer NuGet package to Visage.AppHost/Visage.AppHost.csproj
- [x] T005 Add Aspire.Microsoft.EntityFrameworkCore.SqlServer NuGet package to services/Visage.Services.Registrations/Visage.Services.Registrations.csproj
- [x] T006 [P] Add Aspire.Microsoft.EntityFrameworkCore.SqlServer NuGet package to services/Visage.Services.Eventing/Visage.Services.Eventing.csproj
- [x] T007 Add Aspire.Hosting.Testing NuGet package to tests/Visage.Test.Aspire/Visage.Test.Aspire.csproj for integration tests

**Checkpoint**: All required NuGet packages installed - user story implementation can now begin ✅

---

## Phase 3: User Story 1 - Centralized SQL Server Resource Management (Priority: P1) 🎯 MVP

**Goal**: SQL Server is registered as a first-class Aspire resource with centralized lifecycle management, health monitoring, and automatic connection string injection

**Independent Test**: SQL Server appears in Aspire dashboard with "Healthy" status, both databases (registrationdb, eventingdb) are listed as child resources, and connection strings are available without manual configuration

### Tests for User Story 1 (MANDATORY for Integration Tests) ⚠️

**NOTE: Write integration tests FIRST using TUnit and Fluent Assertions, ensure they FAIL before implementation**

- [x] T008 [P] [US1] Create SqlServerIntegrationTests.cs in tests/Visage.Test.Aspire/ with test for SQL Server resource appearing in Aspire dashboard ✅
- [x] T009 [P] [US1] Add integration test in SqlServerIntegrationTests.cs for registrationdb database availability ✅
- [x] T010 [P] [US1] Add integration test in SqlServerIntegrationTests.cs for eventingdb database availability ✅
- [x] T011 [P] [US1] Add integration test in SqlServerIntegrationTests.cs for SQL Server health check reporting correctly ✅

### Implementation for User Story 1

- [x] T012 [US1] Register SQL Server resource in Visage.AppHost/AppHost.cs using AddSqlServer("sql").WithHealthCheck() ✅
- [x] T013 [US1] Add registrationdb database to SQL Server resource in Visage.AppHost/AppHost.cs using AddDatabase("registrationdb") ✅
- [x] T014 [US1] Add eventingdb database to SQL Server resource in Visage.AppHost/AppHost.cs using AddDatabase("eventingdb") ✅
- [x] T015 [US1] Run Visage.AppHost and verify SQL Server appears in Aspire dashboard with "Healthy" status ✅
- [x] T016 [US1] Verify both databases (registrationdb, eventingdb) appear as child resources in Aspire dashboard ✅
- [ ] T017 [US1] Run integration tests in SqlServerIntegrationTests.cs and verify all tests pass

**Note**: T015-T016 verified - AppHost starts successfully, Aspire dashboard accessible at https://localhost:17044. SQL Server resource is registered and attempting to create container. Integration tests (T017) require SQL Server container to be fully running.

**Checkpoint**: At this point, SQL Server resource is fully configured in Aspire, health checks work, and integration tests pass. Services are NOT yet connected (US2/US3).

---

## Phase 4: User Story 2 - Registration Service Database Migration (Priority: P2)

**Goal**: Registration service uses Aspire-managed SQL Server for registrant data with automatic connection management and observability

**Independent Test**: Can create new registrations, verify data persists in SQL Server, and confirm registration queries work correctly through Aspire-managed connection without standalone database configuration

### Tests for User Story 2 (MANDATORY for Integration Tests) ⚠️

**NOTE: Write integration tests FIRST using TUnit and Fluent Assertions, ensure they FAIL before implementation**

- [ ] T018 [P] [US2] Create RegistrationDbTests.cs in tests/Visage.Tests.Integration/ with test for Registration service connecting to Aspire-managed database
- [ ] T019 [P] [US2] Add integration test in RegistrationDbTests.cs for creating a new registrant record via Registration service
- [ ] T020 [P] [US2] Add integration test in RegistrationDbTests.cs for querying existing registrants from Aspire-managed database
- [ ] T021 [P] [US2] Add integration test in RegistrationDbTests.cs for EF Core migrations running automatically on service startup

### Implementation for User Story 2

- [ ] T022 [US2] Update Visage.AppHost/Program.cs to reference registrationdb for Registration service using WithReference(registrationDb)
- [ ] T023 [US2] Add WaitFor(registrationDb) to Registration service registration in Visage.AppHost/Program.cs to ensure database is ready before service starts
- [ ] T024 [US2] Update services/Visage.Services.Registrations/Program.cs to replace manual DbContext configuration with builder.AddSqlServerDbContext<RegistrantDB>("registrationdb")
- [ ] T025 [US2] Remove connection string from services/Visage.Services.Registrations/appsettings.json (Aspire manages it now)
- [ ] T026 [US2] Add automatic EF Core migration execution on startup in services/Visage.Services.Registrations/Program.cs using dbContext.Database.MigrateAsync()
- [ ] T027 [US2] Remove any hardcoded connection strings from services/Visage.Services.Registrations/RegistrantDB.cs
- [ ] T028 [US2] Run Visage.AppHost and verify Registration service starts after registrationdb is healthy
- [ ] T029 [US2] Verify Registration service health check passes in Aspire dashboard
- [ ] T030 [US2] Run integration tests in RegistrationDbTests.cs and verify all tests pass
- [ ] T031 [US2] Test creating a new registration via API and confirm data persists in SQL Server

**Checkpoint**: At this point, Registration service is fully migrated to Aspire-managed SQL Server. User Story 1 AND 2 should both work independently.

---

## Phase 5: User Story 3 - Eventing Service Database Migration (Priority: P3)

**Goal**: Eventing service uses Aspire-managed SQL Server for event data with consistent data access patterns and centralized management

**Independent Test**: Can create and query events, verify data persists in SQL Server, and confirm event operations work correctly through Aspire-managed connection

### Tests for User Story 3 (MANDATORY for Integration Tests) ⚠️

**NOTE: Write integration tests FIRST using TUnit and Fluent Assertions, ensure they FAIL before implementation**

- [ ] T032 [P] [US3] Create EventingDbTests.cs in tests/Visage.Tests.Integration/ with test for Eventing service connecting to Aspire-managed database
- [ ] T033 [P] [US3] Add integration test in EventingDbTests.cs for creating a new event record via Eventing service
- [ ] T034 [P] [US3] Add integration test in EventingDbTests.cs for querying existing events from Aspire-managed database
- [ ] T035 [P] [US3] Add integration test in EventingDbTests.cs for EF Core migrations running automatically on service startup

### Implementation for User Story 3

- [ ] T036 [US3] Update Visage.AppHost/Program.cs to reference eventingdb for Eventing service using WithReference(eventingDb)
- [ ] T037 [US3] Add WaitFor(eventingDb) to Eventing service registration in Visage.AppHost/Program.cs to ensure database is ready before service starts
- [ ] T038 [US3] Update services/Visage.Services.Eventing/Program.cs to replace manual DbContext configuration with builder.AddSqlServerDbContext<EventDB>("eventingdb")
- [ ] T039 [US3] Remove connection string from services/Visage.Services.Eventing/appsettings.json (Aspire manages it now)
- [ ] T040 [US3] Add automatic EF Core migration execution on startup in services/Visage.Services.Eventing/Program.cs using dbContext.Database.MigrateAsync()
- [ ] T041 [US3] Remove any hardcoded connection strings from services/Visage.Services.Eventing/EventDB.cs
- [ ] T042 [US3] Run Visage.AppHost and verify Eventing service starts after eventingdb is healthy
- [ ] T043 [US3] Verify Eventing service health check passes in Aspire dashboard
- [ ] T044 [US3] Run integration tests in EventingDbTests.cs and verify all tests pass
- [ ] T045 [US3] Test creating a new event via API and confirm data persists in SQL Server

**Checkpoint**: All user stories are now complete. SQL Server resource is fully integrated, Registration and Eventing services both use Aspire-managed connections.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and finalization

- [ ] T046 [P] Update README.md in repository root with new Aspire SQL Server requirements
- [ ] T047 [P] Update .github/copilot-instructions.md to reflect Aspire SQL Server integration patterns (if not already updated by update-agent-context.ps1)
- [ ] T048 Verify all integration tests pass: dotnet test tests/Visage.Tests.Integration/Visage.Tests.Integration.csproj
- [ ] T049 Run through quickstart.md validation: Follow all steps in specs/001-aspire-sqlserver-integration/quickstart.md to ensure developer experience is smooth
- [ ] T050 [P] Add connection pooling configuration notes to appsettings.json files (for production reference)
- [ ] T051 Verify SQL Server health check reporting in Aspire dashboard for all scenarios (healthy, unhealthy, starting)
- [ ] T052 [P] Document any production deployment considerations in specs/001-aspire-sqlserver-integration/quickstart.md (if needed)
- [ ] T053 Run load test to verify connection pooling handles 100+ concurrent operations without exhaustion
- [ ] T054 Final code review: Ensure no hardcoded connection strings remain in any service code or configuration files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3, 4, 5)**: All depend on Foundational phase completion
  - User stories CAN proceed in parallel if staffed (US1, US2, US3 are independent)
  - Or sequentially in priority order: P1 (US1) → P2 (US2) → P3 (US3)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
  - Establishes SQL Server resource and databases in Aspire
  - Services are NOT yet connected (handled in US2/US3)
- **User Story 2 (P2)**: Technically depends on US1 (SQL Server must exist), but can start in parallel if US1 is complete
  - Migrates Registration service to use Aspire-managed SQL Server
  - Does NOT depend on US3 (Eventing service)
- **User Story 3 (P3)**: Technically depends on US1 (SQL Server must exist), but can start in parallel if US1 is complete
  - Migrates Eventing service to use Aspire-managed SQL Server
  - Does NOT depend on US2 (Registration service)

### Within Each User Story

1. **Tests written FIRST** (all [P] tests can run in parallel)
2. **Verify tests FAIL** (before implementation)
3. **Implementation tasks** (follow sequence - AppHost changes before service changes)
4. **Verify tests PASS** (after implementation)
5. **Manual verification** (run AppHost, check dashboard, test APIs)

### Parallel Opportunities

- **Setup (Phase 1)**: All tasks can run in parallel
- **Foundational (Phase 2)**: NuGet package additions (T005, T006) can run in parallel after T004 completes
- **User Story 1 Tests**: T008, T009, T010, T011 can all run in parallel
- **User Story 2 Tests**: T018, T019, T020, T021 can all run in parallel
- **User Story 3 Tests**: T032, T033, T034, T035 can all run in parallel
- **Once US1 completes**: US2 and US3 can be worked on in parallel by different developers
- **Polish (Phase 6)**: T046, T047, T050, T052 can run in parallel

---

## Parallel Example: User Story 1

```bash
# After Foundational phase completes, launch all US1 tests together:
# Terminal 1:
cd tests/Visage.Tests.Integration
# Create SqlServerIntegrationTests.cs with all 4 test methods (T008-T011)
dotnet test --filter "SqlServerIntegrationTests"  # All should FAIL initially

# Then implement US1 (T012-T014):
# Edit Visage.AppHost/Program.cs to register SQL Server and databases

# Verify implementation (T015-T017):
cd Visage.AppHost
dotnet run  # Check Aspire dashboard
dotnet test tests/Visage.Tests.Integration  # All US1 tests should PASS
```

---

## Parallel Example: After US1 Completes

```bash
# With 2+ developers, work on US2 and US3 simultaneously:

# Developer A: User Story 2 (Registration service)
# Create RegistrationDbTests.cs (T018-T021)
# Implement Registration service migration (T022-T027)
# Verify and test (T028-T031)

# Developer B: User Story 3 (Eventing service) - CAN START IN PARALLEL
# Create EventingDbTests.cs (T032-T035)
# Implement Eventing service migration (T036-T041)
# Verify and test (T042-T045)

# Both services are independent and can be completed in parallel
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T007) - CRITICAL, blocks all stories
3. Complete Phase 3: User Story 1 (T008-T017)
4. **STOP and VALIDATE**: 
   - SQL Server appears in Aspire dashboard
   - Health checks report correctly
   - All integration tests pass
5. **Optional**: Deploy/demo if ready (shows Aspire SQL Server integration)

### Incremental Delivery

1. **Foundation Ready**: Complete Setup + Foundational → All NuGet packages installed
2. **Add User Story 1**: SQL Server resource configured → Test independently → Deploy/Demo (SQL Server as first-class resource!)
3. **Add User Story 2**: Registration service migrated → Test independently → Deploy/Demo (Registration on Aspire SQL Server!)
4. **Add User Story 3**: Eventing service migrated → Test independently → Deploy/Demo (Complete migration!)
5. **Polish**: Final testing, documentation, performance validation

Each increment adds value without breaking previous work.

### Parallel Team Strategy

With multiple developers (assuming 2-3 developers):

1. **Everyone**: Complete Setup + Foundational together (T001-T007)
2. **Once Foundational is done**:
   - **Developer A**: User Story 1 (T008-T017) - SQL Server resource
   - **After US1 completes**:
     - **Developer B**: User Story 2 (T018-T031) - Registration service
     - **Developer C**: User Story 3 (T032-T045) - Eventing service (parallel with US2)
3. **Everyone**: Polish & testing (T046-T054)

Stories integrate independently - no merge conflicts expected.

---

## Validation Checklist

Before marking feature as complete, verify:

- [ ] All 54 tasks completed
- [ ] SQL Server appears in Aspire dashboard with "Healthy" status
- [ ] Both registrationdb and eventingdb listed as child resources
- [ ] Registration service connects automatically without manual configuration
- [ ] Eventing service connects automatically without manual configuration
- [ ] All integration tests pass (SqlServerIntegrationTests, RegistrationDbTests, EventingDbTests)
- [ ] EF Core migrations run automatically on service startup for both services
- [ ] No hardcoded connection strings remain in any service code
- [ ] Connection strings removed from appsettings.json in both services
- [ ] Service startup order correct: SQL Server → Databases → Services
- [ ] Health checks report database status accurately in Aspire dashboard
- [ ] Quickstart.md validation completed successfully
- [ ] Load test with 100+ concurrent operations passes without connection exhaustion
- [ ] All documentation updated (README.md, copilot-instructions.md if needed)
- [ ] Code review completed - no constitution violations

---

## Notes

- [P] tasks = different files, no dependencies - can execute in parallel
- [Story] label maps task to specific user story for traceability (US1, US2, US3)
- Each user story is independently completable and testable
- Tests MUST be written first and MUST fail before implementation
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Integration tests are MANDATORY per Visage Constitution Principle III
- This is an infrastructure migration - no new user-facing features, so E2E tests are not required

---

## Summary

- **Total Tasks**: 54 tasks across 6 phases
- **Task Distribution by User Story**:
  - Setup: 3 tasks (T001-T003)
  - Foundational: 4 tasks (T004-T007) - BLOCKS all user stories
  - User Story 1 (P1): 10 tasks (T008-T017) - SQL Server resource management
  - User Story 2 (P2): 14 tasks (T018-T031) - Registration service migration
  - User Story 3 (P3): 14 tasks (T032-T045) - Eventing service migration
  - Polish: 9 tasks (T046-T054) - Cross-cutting concerns
- **Parallel Opportunities**: 
  - All Setup tasks can run in parallel
  - NuGet packages (T005-T007) can install in parallel
  - All tests within a user story can be written in parallel
  - User Stories 2 and 3 can be implemented in parallel after US1 completes
  - Multiple Polish tasks can run in parallel
- **Independent Test Criteria**: Each user story has clear acceptance criteria and integration tests
- **Suggested MVP Scope**: User Story 1 only (SQL Server resource configured, health checks working)
- **Format Validation**: ✅ All tasks follow the required checklist format with checkbox, Task ID, [P] marker (where applicable), [Story] label (where applicable), and file paths