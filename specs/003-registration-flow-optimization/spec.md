# Feature Specification: Registration Flow Optimization with Profile Completion & Smart Redirect

**Feature Branch**: `003-registration-flow-optimization`  
**Created**: October 28, 2025  
**Status**: Draft  
**Input**: User description: "After logging in, the authenticated user is directed to UserRegistration.razor which is quite a huge form and may result in user fatigue while filling out both the Personal Information from which consists of mostly mandatory field operations, while the Accessibility, Inclusiveness form has many fields but are All optional. Also we need to ensure that on future logins, we need to make a determination that the user needs to be redirected to home rather than user registration page"

## Clarifications

### Session 2025-10-28

- Q: What mechanism should be used to check profile completion status on each authentication/navigation event? → A: Server-side API endpoint that checks database and returns profile status, with client-side cache (5-min TTL)
- Q: When and how should users be prompted to complete the optional AIDE (Accessibility, Inclusiveness, Diversity, and Equity) form after completing mandatory registration? → A: Show in post-submission success message plus display a dismissible banner on home page for users with incomplete AIDE profiles
- Q: How should AIDE profile data influence event acceptance decisions, and who makes these decisions? → A: Deferred to future specification - will implement hybrid approach with automated scoring plus human organizer oversight
- Q: How should the banner dismissal state be persisted across sessions and devices? → A: Server-side user preference stored in profile database
- Q: Should users with complete mandatory profiles but incomplete AIDE profiles be able to access a unified profile edit page, or should AIDE fields remain separate? → A: Single unified profile edit page showing both mandatory and optional (AIDE) sections, with visual separation and completion indicators

## User Scenarios & Testing *(mandatory)*

### User Story 1 - First-Time User Completes Mandatory Profile (Priority: P1)

A new authenticated user needs to complete essential personal information before accessing the application. The user should be able to complete mandatory fields quickly without feeling overwhelmed by optional fields.

**Why this priority**: This is the critical path for new user onboarding. Without mandatory profile completion, the user cannot fully participate in events or use core features. This is the minimum viable functionality.

**Independent Test**: Can be fully tested by authenticating a new user, completing only the mandatory Personal Information fields (name, contact, government ID), and verifying they can submit and access the home page. Delivers immediate value by allowing basic event participation.

**Acceptance Scenarios**:

1. **Given** a user has just authenticated for the first time, **When** they are redirected after login, **Then** they see only the mandatory Personal Information section with clear indicators of required fields
2. **Given** a user is filling out mandatory Personal Information, **When** they complete all required fields, **Then** the submit button becomes enabled and they can save their basic profile
3. **Given** a user has submitted mandatory Personal Information, **When** the submission succeeds, **Then** they are redirected to the home page and see a success message encouraging them to complete the optional AIDE profile for better event matching
4. **Given** a user is filling out mandatory Personal Information, **When** they attempt to submit with incomplete required fields, **Then** they see clear validation messages indicating which fields need attention

---

### User Story 2 - User Completes Optional Inclusiveness Profile Later (Priority: P2)

A user with a completed mandatory profile wants to enhance their profile by providing optional inclusiveness and accessibility information at their convenience, enabling better event personalization and community building.

**Why this priority**: This enhances user experience and enables better event matching but is not critical for basic participation. Users can opt-in when ready, reducing initial friction.

**Independent Test**: Can be tested independently by creating a user with completed mandatory profile, navigating to profile settings, and completing accessibility/inclusiveness fields. Delivers value by improving event recommendations and community insights.

**Acceptance Scenarios**:

1. **Given** a user has completed mandatory profile information but not AIDE fields, **When** they view the home page, **Then** they see a dismissible banner encouraging them to complete the optional AIDE profile with a clear explanation of how it improves event acceptance chances
2. **Given** a user has completed mandatory profile information, **When** they access their profile or settings, **Then** they see a unified profile page showing both mandatory and optional sections with visual separation and completion indicators
3. **Given** a user is viewing the unified profile edit page, **When** they see the Accessibility and Inclusiveness section, **Then** all fields are clearly marked as optional with explanations of how the data will be used
4. **Given** a user completes some but not all optional fields, **When** they save, **Then** the partial information is saved successfully without validation errors
5. **Given** a user has completed optional profile information, **When** they return to their profile, **Then** they can view and edit this information at any time
6. **Given** a user dismisses the AIDE completion banner on the home page, **When** they return to the home page in future sessions, **Then** the banner does not reappear (until browser cache/cookies are cleared or after a reasonable period like 30 days)

---

### User Story 3 - Returning User Bypasses Registration (Priority: P1)

An authenticated user who has already completed their mandatory profile should be directed to the home page on login, not the registration flow, ensuring a seamless return experience.

**Why this priority**: This is critical for user retention and experience. Forcing returning users through registration creates frustration and appears broken. This is essential for production readiness.

**Independent Test**: Can be fully tested by authenticating with an existing user account that has a completed profile and verifying immediate redirect to home page. Delivers value by providing efficient access for returning users.

**Acceptance Scenarios**:

1. **Given** a user with a completed mandatory profile authenticates, **When** login completes, **Then** they are redirected directly to the home page
2. **Given** a user with an incomplete mandatory profile authenticates, **When** login completes, **Then** they are redirected to complete their mandatory profile information
3. **Given** the system checks profile completion status, **When** evaluating a user profile, **Then** it accurately determines if mandatory fields (name, government ID, contact information) are present
4. **Given** a returning user lands on the home page, **When** they want to update their profile, **Then** they can access a unified profile edit page through the navigation menu that shows both mandatory and optional sections

---

### User Story 4 - User Saves Partial Progress (Priority: P3)

A user filling out their registration can save partial progress and return later to complete the mandatory fields, preventing data loss and reducing pressure to complete in one session.

**Why this priority**: This improves user experience but is not critical for MVP. Users can complete registration in one sitting initially, and this feature can be added to enhance convenience.

**Independent Test**: Can be tested by filling out half the mandatory fields, clicking "Save Progress", closing the browser, then returning to verify saved data persists. Delivers value by reducing abandonment due to time constraints.

**Acceptance Scenarios**:

1. **Given** a user is partially through mandatory registration, **When** they click "Save Progress", **Then** their entered data is saved and they receive confirmation
2. **Given** a user has saved partial progress, **When** they return to complete registration, **Then** all previously entered data is pre-filled
3. **Given** a user saves progress multiple times, **When** they return, **Then** the most recent saved data is displayed

---

### Edge Cases

- What happens when a user with Auth0 profile data (name, email) authenticates for the first time? (Pre-populate mandatory fields from Auth0 claims where available to reduce data entry)
- How does the system handle users who close the browser mid-registration without saving? (Session-level draft save for 24 hours, after which user must restart)
- What if a user manually navigates to `/user-registration` URL when profile is complete? (Redirect to profile edit page instead)
- How does the system handle profile completion check during concurrent sessions? (Check on every page navigation, use cached profile status with 5-minute TTL)
- What happens if mandatory profile fields change after a user has completed registration? (Existing users are not required to fill new mandatory fields retroactively unless explicitly prompted)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST determine profile completion status for each authenticated user by checking presence of all mandatory fields (FirstName, LastName, Email, MobileNumber, GovtId, GovtIdLast4Digits, AddressLine1, City, State, PostalCode, OccupationStatus, and occupation-specific fields) via a server-side API endpoint that queries the database
- **FR-002**: System MUST redirect authenticated users with incomplete mandatory profiles to a streamlined registration page containing only mandatory Personal Information fields
- **FR-003**: System MUST redirect authenticated users with complete mandatory profiles directly to the home page, bypassing the registration flow
- **FR-004**: System MUST provide a separate entry point for users to complete or update optional Accessibility and Inclusiveness information after mandatory profile completion
- **FR-005**: System MUST pre-populate mandatory registration fields with data available from authentication provider (Auth0) claims, including name and email
- **FR-006**: System MUST clearly indicate which fields are required and which are optional throughout the registration flow using visual indicators (e.g., asterisks, badges)
- **FR-007**: System MUST validate mandatory fields before allowing profile submission and provide specific, actionable error messages for validation failures
- **FR-008**: System MUST allow users to save partial progress on mandatory registration and restore this data when they return within a reasonable timeframe (24 hours)
- **FR-009**: System MUST cache profile completion status client-side with a 5-minute time-to-live (TTL) to minimize repeated API calls while ensuring reasonably fresh data
- **FR-010**: System MUST provide a unified profile edit page accessible through settings that displays both mandatory and optional (AIDE) fields with visual separation, allowing users to view and edit all profile information in one location with clear completion indicators for each section
- **FR-011**: System MUST treat all Accessibility and Inclusiveness fields as optional, never blocking user progress based on these fields
- **FR-012**: System MUST group related mandatory fields logically (basic information, contact information, identification, occupation) to reduce cognitive load
- **FR-013**: System MUST provide a progress indicator showing completion percentage for mandatory fields during registration
- **FR-014**: System MUST display a prompt encouraging AIDE profile completion in the mandatory registration success message, explaining that completing these fields improves chances of event acceptance
- **FR-015**: System MUST display a dismissible banner on the home page for users with incomplete AIDE profiles, encouraging completion with clear benefits messaging
- **FR-016**: System MUST persist user dismissal of the AIDE completion banner server-side in the user profile database and suppress it for a reasonable period (30 days) or until AIDE profile is completed, ensuring consistent behavior across all user devices

### Key Entities *(include if feature involves data)*

- **User Profile**: Represents the complete user information including mandatory fields (FirstName, LastName, Email, MobileNumber, GovtId, GovtIdLast4Digits, Address, OccupationStatus) and optional fields (all Accessibility and Inclusiveness attributes); includes profile completion status flags and user preferences such as AIDE banner dismissal timestamp
- **Profile Completion Status**: Represents whether a user has completed mandatory profile requirements; includes timestamp of last update and list of incomplete mandatory fields
- **Draft Registration**: Represents partially completed registration data that can be restored; includes user identifier, form field values, and expiration timestamp

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: New users can complete mandatory profile registration in under 3 minutes (down from current average of 8+ minutes with full form)
- **SC-002**: Registration abandonment rate decreases by at least 40% compared to current single-form approach
- **SC-003**: Returning users reach the home page within 2 seconds of authentication, with zero instances of being incorrectly routed to registration
- **SC-004**: 90% of users successfully complete mandatory profile on first attempt without validation errors or confusion
- **SC-005**: At least 25% of users with completed mandatory profiles voluntarily complete optional Accessibility and Inclusiveness information within 30 days
- **SC-006**: Zero data loss incidents for partial registrations saved within the 24-hour retention window
- **SC-007**: Profile completion status check adds less than 200ms latency to post-authentication navigation

## Assumptions

- Authentication is handled by Auth0 and provides user claims including sub (user ID), email, and potentially name fields
- Current user model already includes all mandatory and optional fields mentioned in UserRegistration.razor
- Mandatory fields are defined as: FirstName, LastName, Email, MobileNumber, GovtId, GovtIdLast4Digits, AddressLine1, City, State, PostalCode, OccupationStatus, and occupation-specific fields (CompanyName/LinkedInProfile for Working Professionals, EducationalInstituteName/GitHubProfile for Students)
- All fields in the "Accessible, Inclusiveness, Diversity and Equity" section are considered optional
- Profile data is stored server-side with appropriate privacy and security controls
- Users cannot access core event features (registration, check-in) until mandatory profile is complete
- Industry-standard session management with 24-hour timeout for draft registrations is acceptable
- Standard web application performance expectations (page loads under 2 seconds) apply

## Dependencies

- Auth0 authentication must provide stable user claims (sub, email, name)
- Registration service API must support profile completion status queries
- User database schema must support profile completion timestamp and draft registration storage
- Frontend routing mechanism must support conditional redirects based on profile status
- Browser local storage or session storage available for client-side draft persistence fallback

## Out of Scope

- Changing which fields are considered mandatory vs. optional (this specification assumes current field requirements)
- Adding new fields to the registration form
- Integration with third-party data enrichment services to auto-populate profile fields
- Gamification or incentives for completing optional profile information
- Email reminders for incomplete registrations
- Admin interface for viewing profile completion statistics
- Multi-language support for registration forms
- Accessibility improvements beyond the registration flow restructuring
- **Event curation logic**: How AIDE profile data influences event acceptance/selection decisions (deferred to future specification on event curation and attendee selection system)
- **Organizer dashboard**: Interface for event organizers to view and utilize AIDE data during curation (part of future event management specification)

