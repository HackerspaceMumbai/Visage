# Quickstart: Verified social profile linking

**Branch**: `004-verify-social-profiles`

This is a developer-focused guide for running and manually testing verified LinkedIn/GitHub linking locally.

## Prerequisites

- Docker Desktop running (Aspire-managed SQL Server)
- .NET SDK pinned by repo (`global.json`)
- Auth0 development credentials configured (required to sign in)

## Configure OAuth provider apps (LinkedIn + GitHub)

The repo’s intended approach is **direct OAuth** to LinkedIn/GitHub (not Auth0 social connections).

You will need OAuth apps configured with these callback URLs:

- LinkedIn callback: `https://localhost:7400/oauth/linkedin/callback`
- GitHub callback: `https://localhost:7400/oauth/github/callback`

Tip: If you run the frontend behind a proxy, or your provider requires a fixed callback host/port, set the optional `OAuth:BaseUrl` configuration (e.g., `OAuth__BaseUrl=https://localhost:7400`) so the server generates a deterministic `redirect_uri`. The Direct OAuth service logs the `redirect_uri` it uses (INFO level) with keys `redirect_uri` and `usingConfiguredBase` to help debug mismatches.
See:

- `docs/Direct-OAuth-Profile-Verification.md`
- `README_OAUTH_FIX.md`

## Set local secrets

This repo includes helper scripts:

- `setup-oauth-secrets.bat` (Windows)
- `setup-oauth-secrets.sh` (Linux/macOS)

The OAuth configuration keys are:

- `OAuth__LinkedIn__ClientId`
- `OAuth__LinkedIn__ClientSecret`
- `OAuth__GitHub__ClientId`
- `OAuth__GitHub__ClientSecret`

These are wired into the Aspire app model in `Visage.AppHost/AppHost.cs`.

## Run locally

Start the Aspire AppHost and use the dashboard to confirm all resources are healthy:

- SQL Server resource is healthy
- `registrationdb` exists
- `registrations-api` is healthy
- `frontendweb` is healthy

## Manual test flow (UI)

1. Open the frontend web app.
2. Navigate to `/registration/mandatory`.
3. Pick an occupation status that requires a social profile:
   - Employed/Self-employed → LinkedIn required
   - Student → GitHub required
4. Click **Connect LinkedIn** or **Connect GitHub**.
5. Complete provider authentication.
6. Verify you are redirected back and the UI shows a **Verified** state.

## Manual test flow (API)

The Registrations service exposes these endpoints:

- `GET /api/profile/social/status`
- `POST /api/profile/social/link-callback`
- `POST /api/profile/social/disconnect` (planned)

To call them manually you’ll need a Bearer access token with `profile:read-write` scope.

Use the existing `.http` file in `Visage.Services.Registrations/app.http` as the starting point for requests.

## Accessibility check

Build UI changes with accessibility in mind, but still validate manually using tools such as Accessibility Insights.
