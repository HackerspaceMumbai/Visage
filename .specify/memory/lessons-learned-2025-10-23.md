# Lessons Learned: Blazor Navigation, Form Validation & Event Status (2025-10-23)

## Session Summary

**Branch**: `002-blazor-frontend-redesign`  
**Date**: October 23, 2025  
**Issues Addressed**:

1. Navigation failure (URL changes but page doesn't render)
2. Build warnings (Playwright obsolete API usage)
3. Validation UX regression (missing inline error messages)
4. Event status misclassification (same-day future events marked as Completed)
5. UI inconsistency (badge vs button alignment)

---

## Issue 1: Blazor Navigation Failure

### Problem

Clicking "Schedule Event" button changed the URL but didn't navigate to ScheduleEvent.razor. Browser console showed `insertBefore` errors.

### Root Cause

**Render mode mismatch**: Home.razor used `@rendermode InteractiveAuto` while ScheduleEvent.razor used `@rendermode InteractiveServer`. When InteractiveAuto runs on WebAssembly, Aspire service discovery URIs (`https+http://eventing`) don't resolve, causing silent failures.

### Solution

1. **Centralized render mode** in `App.razor`: Set `@rendermode="InteractiveServer"` on the `<Routes>` component
2. **Removed per-page overrides**: Deleted `@rendermode` directives from Home.razor and ScheduleEvent.razor
3. **Simplified navigation**: Removed conditional `NavLink` with `forceLoad` hack

### Files Changed

- `Visage.FrontEnd/Visage.FrontEnd.Web/Components/App.razor` - Added app-wide InteractiveServer
- `Visage.FrontEnd/Visage.FrontEnd.Shared/Pages/Home.razor` - Removed `@rendermode InteractiveAuto`
- `Visage.FrontEnd/Visage.FrontEnd.Shared/Pages/ScheduleEvent.razor` - Removed `@rendermode InteractiveServer`

### Key Takeaways

- **For Auth0 + Aspire apps**: Always use InteractiveServer as app-wide default
- **Mixing render modes breaks navigation**: Avoid per-page overrides unless explicitly justified
- **Service discovery requires server context**: Aspire URIs only resolve server-side

---

## Issue 2: Playwright Build Warnings

### Problem

5 CS0612 warnings: `LocatorIsVisibleOptions.Timeout` is obsolete.

### Solution
Created `IsVisibleWithinAsync` helper method using `WaitForAsync` with `WaitForSelectorState.Visible`.

### Key Takeaway
Use `WaitForAsync` with explicit `State` instead of `IsVisibleAsync` with `Timeout` for Playwright visibility checks.

---

## Issue 3: Validation UX Regression

### Problem

After refactoring, field-specific validation errors disappeared. Only generic `ValidationSummary` displayed errors.

### Root Cause

`<ValidationMessage>` components were removed during cleanup, leaving only `<ValidationSummary>` for form-level errors.

### Solution
Added `<ValidationMessage For="@(() => newEvent.Property)" />` for each validated field.

### Key Takeaways

- **ValidationSummary ≠ ValidationMessage**: Both are needed for good UX
- **ValidationSummary**: Form-level errors at top
- **ValidationMessage**: Field-specific errors inline with inputs

---

## Issue 4: Event Status Misclassification

### Problem

Newly scheduled event (Oct 23, 2025 at 2:54 PM) showed as "Completed" in Past Events section.

### Root Cause

**Date-only comparison**: Code used `evt.StartDate > DateOnly.FromDateTime(DateTime.Now)`, ignoring time.

### Solution

Changed to **full DateTime comparison**: `eventDateTime > DateTime.Now`

### Key Takeaway

**Use full DateTime for time-sensitive logic**. `DateOnly` comparisons lose time precision.

---

## Issue 5: UI Inconsistency

### Problem

"Completed" events displayed as badge while Upcoming events had RSVP button, breaking alignment.

### Solution

Changed "Completed" to disabled button for consistency.

### Key Takeaway

**Maintain consistent component styling** across states.

---

## Architecture Updates

### Constitution.md (v1.2.0)

**Modified Sections**:

- **Principle VIII**: Blazor Render Mode Strategy - Added navigation rules and architectural guidance
- **Approved Patterns**: Added form validation, DateTime, and UI consistency patterns
- **Quality Gates**: Added render mode, form validation, and DateTime verification steps

### Copilot-Instructions.md

**Added Sections**:

- Blazor render modes: Default strategy, override guidance, navigation warnings
- Form validation: EditForm + DataAnnotationsValidator + ValidationMessageStore pattern
- DateTime comparisons: Best practices for event status classification
- UI consistency: Maintain visual styling across states

---

## Testing Recommendations

### Regression Tests

1. **Navigation**: Test all page-to-page flows for SPA behavior
2. **Form validation**: Verify inline ValidationMessage on all fields
3. **Event status**: Test same-day event creation at various times
4. **UI consistency**: Verify all component states use consistent styling

### E2E Scenarios

1. Create same-day future event → verify Upcoming section with RSVP
2. Navigate Home → Schedule Event → Submit → verify return with new event
3. Trigger validation → verify inline messages
4. Test mobile viewport → verify button/badge alignment

---

**Author**: GitHub Copilot (Claude Sonnet 4.5)  
**Status**: Implemented and validated
