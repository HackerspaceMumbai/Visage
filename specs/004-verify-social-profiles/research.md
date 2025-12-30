# Phase 0 Research: Verified social profile linking

**Branch**: `004-verify-social-profiles` | **Date**: 2025-12-13

This document captures the key implementation decision(s) needed before locking the data model and API contracts.

## Current state (what already exists in repo)

- UI: `Visage.FrontEnd/Visage.FrontEnd.Shared/Components/MandatoryRegistration.razor*` already shows **Connect LinkedIn/GitHub** and **Verified** badges.
- Frontend service: `Visage.FrontEnd/Visage.FrontEnd.Shared/Services/SocialAuthService.cs` currently initiates **Auth0 social connections** via `/Account/LinkSocial`.
- Callback page: `Visage.FrontEnd/Visage.FrontEnd.Shared/Pages/OAuthCallback.razor` parses **Auth0-issued claims** to infer provider + profile URL.
- Backend persistence:
  - `POST /api/profile/social/link-callback` stores provider + profile URL and marks verified.
  - `GET /api/profile/social/status` returns current status.
- Direct provider OAuth building blocks already exist:
  - `Visage.FrontEnd/Visage.FrontEnd.Web/Services/DirectOAuthService.cs`
  - `Visage.FrontEnd.Web/Configuration/OAuthOptions.cs`
  - Repo guidance in `OAUTH_FIX_INSTRUCTIONS.md` / `QUICK_FIX_OAUTH.md` / `docs/Direct-OAuth-Profile-Verification.md`

Gaps vs spec:

- No uniqueness enforcement (one LinkedIn/GitHub account can be verified for multiple registrants).
- No server-side disconnect endpoint (current UI “disconnect” updates local state only).
- No durable audit trail of link/unlink/verification outcomes (logs exist, but not queryable history).

## Decision: Auth0 social connection vs direct provider OAuth

### Option A — Keep Auth0 social connections as the verification mechanism

**How it works**: Redirect to Auth0 using a specific social connection (LinkedIn/GitHub), then extract provider/profile URL from Auth0 claims.

**Pros**

- Fewer moving parts (Auth0 handles provider auth and token exchange).
- Less risk of provider API changes.

**Cons**

- Requires Auth0 claim wiring to reliably obtain the profile URL (fragile and environment-specific).
- Harder to guarantee a canonical profile URL without direct provider API confirmation.
- Repo already contains docs stating the goal is to **bypass Auth0** for social verification.

### Option B — Use direct OAuth with LinkedIn/GitHub (recommended)

**How it works**: Frontend web host exposes `/oauth/{provider}/authorize` + `/oauth/{provider}/callback` endpoints. It exchanges the code for a provider access token, fetches the profile, derives a canonical profile URL, and redirects back to the registration UI. The UI then calls `POST /api/profile/social/link-callback` to persist the verified URL.

**Pros**

- Verification is based on the **provider’s API**, not derived/assumed claims.
- Removes dependency on Auth0 social-connection customization.
- Matches repository guidance (`OAUTH_FIX_INSTRUCTIONS.md`, `docs/Direct-OAuth-Profile-Verification.md`).

**Cons / Risks**

- Requires careful CSRF/state handling (mitigated with server-side session + state).
- LinkedIn’s modern APIs may not return a stable public profile slug with minimal scopes; the current implementation constructs a URL from `id`, which may not map 1:1 to the public URL. We may need to adjust the LinkedIn fetch/URL derivation strategy.

### Decision

Proceed with **Option B (direct provider OAuth)** for LinkedIn/GitHub verification.

Auth0 remains the primary OIDC provider for Visage authentication; direct OAuth is only used to prove ownership of a social profile and to derive the verified profile URL.

## Security and privacy notes

- **CSRF/state**: Use a cryptographically-random `state` value per OAuth initiation, stored server-side (ASP.NET Session) and validated on callback.
- **Token handling**: Do not persist provider access tokens; use them only to fetch profile data.
- **Data minimization**: Persist only:
  - provider name (`linkedin`/`github`)
  - verified profile URL
  - verification timestamp
  - (optional) minimal display name/handle if needed for UI
- **Authorization**:
  - `/oauth/*/authorize` should require the user to be authenticated.
  - Registrations service social endpoints must require an access token with `profile:read-write`.

## UX notes (error handling)

- If the user cancels/denies on the provider side, redirect back to `/registration/mandatory` with a safe error message and a retry button.
- Avoid dumping raw exception details in the UI.

## Testing implications

- Add integration tests for:
  - successful `link-callback` updates
  - `status` returns persisted values
  - uniqueness conflict returns a clear `409 Conflict`
  - disconnect clears values and returns updated status
- Keep E2E tests minimal and smoke-level; provider OAuth flows are difficult to automate in CI.
