# Test Baseline - Pre-Existing Test Failures

**Last Updated**: 2025-11-11  
**Total Tests**: 46  
**Passing**: 25  
**Failing**: 19 (documented below)  
**Skipped**: 2

## Purpose
This document tracks pre-existing test failures to distinguish new issues from technical debt. Update this file when:
- Resolving pre-existing failures
- Adding new test suites
- Identifying new failure patterns

## Pre-Existing Failures by Category

### 1. Auth0 Configuration Issues (2 failures)

**Root Cause**: `AUTH0_DOMAIN` environment variable not set for test environment

**Affected Tests**:
1. `WhenUserFillsProfileThenDraftSavesAndRestores`
   - **Error**: Configuration value 'Auth0:Domain' is null or empty
   - **Expected**: Test should save draft profile data
   - **Actual**: NullReferenceException on Auth0 domain lookup

2. `WhenUserDeletesDraftThenFieldsAreCleared`
   - **Error**: Configuration value 'Auth0:Domain' is null or empty
   - **Expected**: Test should delete draft and clear fields
   - **Actual**: NullReferenceException on Auth0 domain lookup

**Resolution Plan**: 
- Configure Auth0 test tenant OR mock authentication for test environment
- Add `AUTH0_DOMAIN` to test configuration or use `IAuthenticationService` mock

---

### 2. Draft API Authorization Failures (11 failures)

**Root Cause**: Draft API endpoints returning 401 Unauthorized instead of expected 200/204/404

**Affected Tests**:
1. `Draft_Save_Endpoint_Should_Save_Valid_Data`
   - **Expected**: 200 OK with saved draft data
   - **Actual**: 401 Unauthorized

2. `Draft_Save_Endpoint_Should_Return_BadRequest_For_Invalid_Data`
   - **Expected**: 400 Bad Request
   - **Actual**: 401 Unauthorized

3. `Draft_Retrieval_Endpoint_Should_Return_Saved_Draft`
   - **Expected**: 200 OK with draft data
   - **Actual**: 401 Unauthorized

4. `Draft_Retrieval_Endpoint_Should_Return_NotFound_When_No_Draft`
   - **Expected**: 404 Not Found
   - **Actual**: 401 Unauthorized

5. `Draft_Deletion_Endpoint_Should_Delete_Existing_Draft`
   - **Expected**: 204 No Content
   - **Actual**: 401 Unauthorized

6. `Draft_Deletion_Endpoint_Should_Return_NotFound_When_No_Draft`
   - **Expected**: 404 Not Found
   - **Actual**: 401 Unauthorized

7. Additional 5 Draft API tests with same 401 pattern

**Resolution Plan**:
- Configure test JWT tokens with `profile:read-write` scope
- Add Auth0 test user configuration
- OR mock `IAuthenticationService` for test environment
- Verify Draft API endpoints have correct `[Authorize]` attributes

---

### 3. Playwright Selector Strict Mode Violations (6 failures)

**Root Cause**: Playwright selectors matching multiple elements when expecting single element

**Affected Tests**:
1. `EventCardShowsNameLocationAndRsvp`
   - **Error**: Strict mode violation: getByText("Mumbai Hackerspace Tech Meetup") resolved to 2 elements
   - **Expected**: Single event card with unique text
   - **Actual**: Multiple matching elements on page

2. `ShouldShowUpcomingAndPastEventsAfterSeeding` (multiple violations)
   - **Error**: Multiple elements matching "Mumbai Hackerspace Tech Meetup", "Tech Conference 2025"
   - **Expected**: Distinct event cards in Upcoming/Past sections
   - **Actual**: Duplicate elements or non-unique selectors

3. Additional 4 Playwright tests with similar strict mode violations

**Resolution Plan**:
- Use more specific selectors: `page.locator('[data-testid="upcoming-events"] >> text="Event Name"')`
- Add `data-testid` attributes to Blazor components for unique identification
- Use `.first()` or `.nth(0)` only when multiple elements are intentional
- Review event seeding to ensure test data isolation

---

## Health Endpoint Tests Status

**Added**: 2025-11-11  
**Status**: ✅ All Passing (7 tests)

These tests are NEW and should NOT have any failures:
- `RegistrationApi_Health_Endpoint_Should_Return_200` ✅
- `RegistrationApi_Alive_Endpoint_Should_Return_200` ✅
- `EventingApi_Health_Endpoint_Should_Return_200` ✅
- `EventingApi_Alive_Endpoint_Should_Return_200` ✅
- `FrontendWeb_Health_Endpoint_Should_Return_200` ✅
- `FrontendWeb_Alive_Endpoint_Should_Return_200` ✅
- `All_Http_Resources_Should_Have_Health_Endpoints` ✅

---

## Resolution Priority

**P1 - Critical (Blocks PR Merges)**:
- Health endpoint tests must pass (currently passing ✅)

**P2 - High (Reduces Test Confidence)**:
- Auth0 configuration issues (2 tests)
- Draft API authorization failures (11 tests)

**P3 - Medium (UI Test Reliability)**:
- Playwright selector violations (6 tests)

---

## Notes
- When resolving failures, update this document with resolution date and approach
- Add new failure categories as discovered
- Keep this file in sync with actual test suite status
- Run `dotnet test tests/Visage.Test.Aspire/Visage.Test.Aspire.csproj --logger "console;verbosity=detailed"` to verify current status
