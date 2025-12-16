# Feature Specification: Verified social profile linking

**Feature Branch**: `004-verify-social-profiles`  
**Created**: 2025-12-13  
**Status**: Draft  
**Input**: User description: "Instead of relying on free form text for the LinkedIn and GitHub fields in MandatoryRegistration, add connect buttons to link these accounts and verify the registrant truly owns the LinkedIn/GitHub profile so we can use it for registrant curation and avoid typos or false info."

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Link and verify required social profile(s) during mandatory registration (Priority: P1)

As a registrant completing mandatory registration, I can connect my LinkedIn and/or GitHub using a guided “Connect” action so the system can verify I control the account and record it as verified.

**Why this priority**: This prevents typos and intentional misrepresentation at the point we first collect the data, improving trust for curation.

**Independent Test**: Can be fully tested by completing mandatory registration for each applicable occupation status and verifying that registration succeeds only when the required social profile is verified.

**Acceptance Scenarios**:

1. **Given** a registrant is required to provide LinkedIn (e.g., employed/self-employed), **When** they connect a LinkedIn account and the system verifies control, **Then** the registration form shows the verified LinkedIn identity and allows registration submission.
2. **Given** a registrant is required to provide GitHub (e.g., student), **When** they do not connect a GitHub account, **Then** the system clearly explains what’s missing and prevents registration completion.
3. **Given** a registrant starts a connect flow but cancels or denies permission, **When** they return to the registration form, **Then** the form remains incomplete and provides a safe retry option.

---

### User Story 2 - Review, disconnect, and relink a social profile (Priority: P2)

As a registrant, I can see which social account is linked, disconnect it, and reconnect a different one if I linked the wrong account.

**Why this priority**: Mistakes happen (wrong browser account, shared machine). Users need a self-service correction path without support.

**Independent Test**: Can be tested by linking one account, disconnecting, linking a different account, and confirming the verified identity updates and is used for registration.

**Acceptance Scenarios**:

1. **Given** a registrant has a verified LinkedIn or GitHub connected, **When** they choose to disconnect it, **Then** the system removes the verified status and (if required) prevents registration completion until another account is verified.
2. **Given** a registrant previously verified an account, **When** they reconnect and verify a different account, **Then** the system records the new verified identity and uses it going forward.

---

### User Story 3 - Use verified social profiles during registrant curation (Priority: P3)

As an event organizer/curator, I can view the registrant’s verified LinkedIn and/or GitHub links and trust that the registrant controls them.

**Why this priority**: The core business value is better curation decisions based on credible profile signals.

**Independent Test**: Can be tested by completing registration with a verified social profile and confirming the curation view (or exported registrant data) includes the verified link and a “verified” indicator.

**Acceptance Scenarios**:

1. **Given** a registrant has a verified LinkedIn or GitHub profile, **When** a curator reviews the registrant’s information, **Then** the curator sees the verified profile link and a clear verification status.

---

[Add more user stories as needed, each with an assigned priority]

### Edge Cases

- Provider is unavailable or slow during connect/verification.
- Registrant attempts to connect an account that is already verified for a different registrant.
- Registrant changes occupation status (e.g., Student → Employed) after verifying only GitHub.
- Registrant is using a device/browser that blocks pop-ups or third-party cookies, causing connect flow interruptions.
- Registrant completes registration, then later the connected account is removed or renamed on the provider side.
- Registrant has no LinkedIn/GitHub account (required-case handling should clearly explain next steps).

## Requirements *(mandatory)*

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

### Functional Requirements

- **FR-001**: System MUST present “Connect LinkedIn” and “Connect GitHub” actions (instead of free-form text fields) when those profiles are requested from the registrant.
- **FR-002**: System MUST verify that the registrant controls the connected LinkedIn/GitHub account before marking it as verified.
- **FR-003**: System MUST display the connected identity in a human-readable way (e.g., name/handle + a link) along with a clear “verified” indicator.
- **FR-004**: System MUST enforce mandatory social profile rules based on the registrant’s occupation status, preventing registration completion when required profiles are not verified.
- **FR-005**: System MUST allow a registrant to disconnect a previously verified social profile and connect a different account.
- **FR-006**: System MUST prevent a single LinkedIn/GitHub account from being verified for multiple registrants (to reduce impersonation and duplicate identity reuse), and provide a clear support path when this occurs.
- **FR-007**: System MUST keep an audit trail of social profile linking actions (connected, disconnected, verification succeeded/failed) sufficient for troubleshooting and compliance review.
- **FR-008**: System MUST minimize personal data collection by storing only what is required for curation use (verified link + minimal identity attributes) and by clearly communicating what will be stored.
- **FR-009**: System MUST provide accessible, actionable error messaging and allow retries when verification fails due to user cancellation, provider errors, or transient failures.

### Key Entities *(include if feature involves data)*

- **SocialProfileLink**: Represents a registrant’s linked external profile (provider name, verified profile URL, display handle/name, verification status, linked timestamp).
- **SocialVerificationEvent**: Represents an auditable record of link/unlink/verification attempts (timestamp, outcome, high-level failure reason).

### Assumptions & Dependencies

- The mandatory registration flow already determines which social profile(s) are required based on occupation status.
- LinkedIn and GitHub provide a user-facing sign-in/consent flow that can be used to prove control of an account.
- Verified social profile information will be visible to authorized event staff for the purpose of registrant curation.

### Scope Boundaries

- In scope: replacing manual entry with verified linking for LinkedIn/GitHub in mandatory registration, storing verification status, and making the verified link available for curation.
- Out of scope: assessing the quality of a profile (e.g., judging experience), automated scoring of registrants, or collecting more profile data than needed to link and verify.

## Success Criteria *(mandatory)*

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### Measurable Outcomes

- **SC-001**: At least 95% of mandatory registrations that require a social profile end with that profile verified (LinkedIn for employed/self-employed; GitHub for students).
- **SC-002**: Reduce “invalid/typo/false social profile” corrections during curation by at least 80% compared to a baseline period using manual entry.
- **SC-003**: At least 90% of registrants who attempt to connect a required social profile successfully complete verification on the first attempt.
- **SC-004**: Reduce average time spent by curators validating social profile authenticity by at least 50% (measured across comparable events).

