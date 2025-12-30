# Phase 1 Data Model: Verified social profile linking

**Branch**: `004-verify-social-profiles` | **Date**: 2025-12-13

This document describes the data model needed to support verified LinkedIn/GitHub linking, including uniqueness enforcement and an audit trail.

## Existing data (current schema)

The Registrations domain currently stores social verification on the registrant record (see `Visage.Services.Registrations`):

- `LinkedInProfile` (string/URL)
- `IsLinkedInVerified` (bool)
- `LinkedInVerifiedAt` (DateTime?)
- `GitHubProfile` (string/URL)
- `IsGitHubVerified` (bool)
- `GitHubVerifiedAt` (DateTime?)

This is sufficient to support the “verified link + timestamp” portion of the spec.

## Gap: uniqueness enforcement (FR-006)

Requirement: a single LinkedIn/GitHub account **must not** be verified for multiple registrants.

### Application-level check (for friendly errors)

In `POST /api/profile/social/link-callback`:

- When linking provider `linkedin`, query for any other registrant with `IsLinkedInVerified = true` and `LinkedInProfile == linkDto.ProfileUrl`.
- If found, return `409 Conflict` with a problem payload that indicates the account is already in use.

Do the same for `github`.

This provides the correct UX even without relying on database constraint error messages.

### Database-level enforcement (to prevent races)

Add a **filtered unique index** in SQL Server to prevent concurrent requests from violating uniqueness.

Suggested indexes:

- Unique on `LinkedInProfile` where `IsLinkedInVerified = 1` and `LinkedInProfile IS NOT NULL`
- Unique on `GitHubProfile` where `IsGitHubVerified = 1` and `GitHubProfile IS NOT NULL`

EF Core supports filtered indexes for SQL Server (`HasFilter(...)`). The migration should be part of `Visage.Services.Registrations/Migrations/`.

Notes:

- This index strategy allows multiple unverified rows to contain null/empty profiles.
- Application code should still normalize profile URLs (trim, canonicalize) to avoid false negatives.

## Gap: durable audit trail (FR-007)

Logs are helpful, but we need a durable, queryable history for troubleshooting and compliance.

### Proposed entity: SocialVerificationEvent

Create a new table to record key actions.

Fields (recommended):

- `Id` (Guid)
- `RegistrantId` (StrictId / FK to Registrant)
- `Provider` (string: `linkedin` | `github`)
- `Action` (string/enum: `link_attempt` | `link_succeeded` | `link_failed` | `disconnect`)
- `ProfileUrl` (string, nullable for failures)
- `OccurredAtUtc` (DateTime)
- `Outcome` (string/enum: `succeeded` | `failed`)
- `FailureReason` (string, nullable, sanitized)

Indexes:

- `(RegistrantId, OccurredAtUtc DESC)` for timeline queries
- `(Provider, ProfileUrl)` for investigations (optional)

Privacy notes:

- Do not store access tokens.
- Keep `FailureReason` high-level (e.g., “provider_error”, “state_invalid”, “conflict”) to avoid leaking sensitive details.

## Disconnect semantics

A disconnect should clear:

- `<Provider>Profile`
- `Is<Provider>Verified`
- `<Provider>VerifiedAt`

…and record a `SocialVerificationEvent`.

This should be done via a server-side endpoint (see contracts), not only in UI state.
