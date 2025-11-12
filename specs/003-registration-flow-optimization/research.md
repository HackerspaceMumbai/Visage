# Phase 0 Research: Registration Flow Optimization

**Branch**: `003-registration-flow-optimization` | **Date**: October 28, 2025
**Purpose**: Research best practices, patterns, and technical approaches before detailed design

## Research Questions

### 1. Client-Side Caching Strategy for Profile Completion Status

**Question**: What is the best approach for implementing a 5-minute client-side cache for profile completion status that balances performance with data freshness?

**Research Areas**:

- Browser storage options (localStorage, sessionStorage, IndexedDB, in-memory)
- Cache invalidation strategies (TTL-based, event-driven, manual)
- Cache coherence across browser tabs
- Security considerations for cached user data
- .NET Blazor-specific caching patterns (ProtectedSessionStorage, ProtectedLocalStorage)

**Success Criteria**:

- Cache reduces API calls by 80%+ during normal browsing
- Cache invalidates immediately on profile updates
- Cache works across browser tabs/windows
- Cached data is secure and not exposed to client-side JS (if using Blazor Server)

**Findings**: [To be filled during research]

---

### 2. PostgreSQL Schema Design for Profile Completion Tracking

**Question**: How should we model profile completion status, draft registrations, and user preferences in PostgreSQL for optimal query performance and data integrity?

**Research Areas**:

- Schema design for profile completion flags (denormalized vs. computed column)
- Draft registration storage with 24-hour auto-expiration (PostgreSQL TTL patterns)
- User preferences table design (key-value vs. structured columns)
- Indexing strategy for profile completion queries (by user ID, by completion status)
- EF Core migration patterns for adding new entities to existing database
- PostgreSQL JSONB vs. relational columns for flexible preference storage

**Success Criteria**:

- Profile completion check query executes in <50ms (leaving headroom for <200ms API latency target)
- Draft registrations auto-delete after 24 hours without manual cleanup jobs
- User preferences scale to 100k+ users without performance degradation
- Schema supports future extensibility (additional mandatory fields, new preference types)

**Findings**: [To be filled during research]

---

### 3. Blazor Multi-Section Form Validation Patterns

**Question**: What is the recommended pattern for implementing form validation in Blazor that works across multiple sections (mandatory vs. optional) with clear error messaging?

**Research Areas**:

- EditForm component with DataAnnotationsValidator vs. FluentValidation
- ValidationMessageStore for custom business rule validation
- Inline ValidationMessage components for field-specific errors
- Progressive validation (validate on blur vs. on submit)
- Section-level validation summaries
- Visual indicators for required fields (DaisyUI badge patterns)
- Accessibility considerations (ARIA labels, screen reader support)

**Success Criteria**:

- Validation errors are clear and actionable
- Required fields are visually distinct from optional fields
- Validation does not block partial saves (for draft functionality)
- Validation works seamlessly with DaisyUI form components
- Form validation is accessible (keyboard navigation, screen reader friendly)

**Findings**: [To be filled during research]

---

### 4. Auth0 Custom Claims Refresh for Profile Completion Status

**Question**: Should profile completion status be stored as a custom claim in Auth0 JWT, and if so, how do we ensure claims refresh after profile updates?

**Research Areas**:

- Auth0 custom claims in ID tokens vs. access tokens
- JWT refresh strategies (forced logout/login, silent authentication, token refresh)
- Trade-offs: JWT claims vs. API-based status checks
- Auth0 Actions/Rules for injecting profile completion status
- IdP abstraction considerations (how to make this work with Keycloak, Entra ID)
- Security implications of caching profile status in JWTs

**Success Criteria**:

- Approach works with IdP abstraction (not Auth0-specific)
- Profile status updates reflected within 5 minutes (cache TTL)
- No forced logout/login required for status updates
- Minimal Auth0-specific code (keeps IdP abstraction clean)

**Findings**: [To be filled during research]

---

### 5. Draft Registration Auto-Save Patterns

**Question**: What is the best pattern for implementing auto-save draft functionality in Blazor forms that balances user experience with server load?

**Research Areas**:

- Auto-save triggers (time-based, field blur, debounced)
- Optimistic UI updates vs. server confirmation
- Conflict resolution (user edits in multiple tabs)
- Draft expiration and cleanup strategies
- User feedback for draft saves (toast notifications, inline indicators)
- Server-side rate limiting for draft saves

**Success Criteria**:

- Draft saves are unobtrusive (no modal dialogs)
- User sees clear confirmation of save status
- Auto-save frequency is tuned to minimize server load (e.g., 30-second debounce)
- Draft data is secure and encrypted at rest
- Expired drafts auto-delete without manual jobs

**Findings**: [To be filled during research]

---

## Recommended Best Practices (from Constitution)

### Form Validation (Constitution: Technology Stack → Approved Patterns)

- Use EditForm with DataAnnotationsValidator
- Add custom ValidationMessageStore for business rules
- Include inline `<ValidationMessage For="@(() => model.Property)" />` components
- Use `OnValidSubmit` for success, `OnInvalidSubmit` for feedback
- Wrap handlers in try/catch with `isSubmitting` state for visual feedback

### DaisyUI Styling (Constitution: Principle IX)

- Use `input` class for text fields
- Use `select` class for dropdowns
- Use `textarea` class for multi-line fields
- Use `badge` with `badge-primary` for required field indicators
- Use `collapse` with `collapse-arrow` for collapsible sections
- Use `alert` with `alert-info` for AIDE completion banner
- Use `btn` with `btn-primary` for submit buttons

### Blazor Render Mode (Constitution: Principle VIII)

- Use InteractiveServer (app-wide default) for authenticated forms
- Do not override render mode on individual pages (avoids SPA navigation issues)
- Ensure all navigation stays within InteractiveServer context

## Next Steps

After completing research above:

1. Document findings for each research question
2. Update Technical Context in plan.md if clarifications emerge
3. Proceed to Phase 1: Create data-model.md, contracts/, quickstart.md
4. Gate: Re-check Constitution compliance after design decisions
