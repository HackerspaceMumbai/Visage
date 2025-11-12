---
name: Update Windows Target Framework Moniker to Windows 11 equivalent
about: Bump the Windows TFM in MAUI projects from `windows10.0.19041.0` to a Windows 11 SDK equivalent and update related platform properties.
title: "Upgrade Windows TFM to Windows 11 (net10.0-windows10.0.22621.0)"
labels: [area/maui, tech-debt, target/net10]
assignees: []
---

## Summary

Current MAUI project files (for example `Visage.FrontEnd/Visage.FrontEnd.csproj`) target a Windows TFM of `net9.0-windows10.0.19041.0`. As part of the planned migration to .NET 10 we should also offer an option to update the Windows Target Framework Moniker to the Windows 11 equivalent (for example `net10.0-windows10.0.22621.0`).

## Rationale

- `19041` corresponds to Windows 10 20H1 (May 2020). Bumping to `22621` targets Windows 11 and unlocks newer platform APIs and improved tooling support.
- Moving to `net10.0-windows10.0.22621.0` aligns with upgrading the solution to .NET 10 and modernizing platform targets for MAUI desktop builds.
- This is optional for backwards compatibility; keeping `19041` is still supported if we want to preserve older Windows support.

## Acceptance criteria

- Add a decision or PR that updates the Windows TFM in MAUI projects to `net10.0-windows10.0.22621.0` (or documents the choice to keep `19041`).
- Update `SupportedOSPlatformVersion` and `TargetPlatformMinVersion` for Windows in the same project files to `10.0.22621.0`.
- CI/build images and developer setup documentation updated if newer SDKs/workloads are required.
- A PR is created that updates the `Visage.FrontEnd.csproj` (and any other affected projects) and successfully builds on the CI.

## Implementation steps

1. Search repository for occurrences of `net9.0-windows10.0.19041.0`, `net9.0-`, and Windows min versions like `10.0.17763.0`.
2. Update `TargetFrameworks` in `Visage.FrontEnd.csproj`:
   - `net9.0-android;net9.0-ios;net9.0-maccatalyst` -> `net10.0-android;net10.0-ios;net10.0-maccatalyst`
   - `net9.0-windows10.0.19041.0` -> `net10.0-windows10.0.22621.0`
3. Update the Windows `SupportedOSPlatformVersion` and `TargetPlatformMinVersion` to `10.0.22621.0`.
4. Update `global.json` if pinning SDK versions to use a .NET 10 SDK.
5. Update MAUI/other package references if necessary to versions compatible with .NET 10.
6. Run `dotnet build` on CI (or locally) and fix any API/package compatibility issues.

## Notes

- This issue is primarily tech-debt/upgrade work and may be split into smaller PRs: one to update TFMs, another to update CI and docs, and a follow-up to address compatibility issues.
- If we want to keep broader Windows 10 compatibility, we can instead change the TFM to `net10.0-windows10.0.19041.0` and only bump the `SupportedOSPlatformVersion` when platform APIs are required.

If you want, I can prepare the PR that updates `Visage.FrontEnd.csproj` and scan for other occurrences to include in the same PR.
