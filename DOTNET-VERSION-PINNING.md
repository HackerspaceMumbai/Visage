# .NET Version Pinning for Visage

This document describes the .NET version pinning implementation for the Visage solution.

## Overview

The Visage solution has been configured to pin the .NET SDK version to ensure consistent builds across all development environments. This addresses issue #187 "Pin the .NET version to 10 for the solution".

## Current Configuration

- **Pinned SDK Version**: 8.0.119
- **Target Framework**: net8.0 (across all projects)
- **Rollforward Policy**: latestPatch

## Files Modified

### global.json
Created to pin the .NET SDK version:
```json
{
  "sdk": {
    "version": "8.0.119",
    "rollForward": "latestPatch"
  }
}
```

### Project Files
All `.csproj` files updated to target `net8.0`:
- Visage.AppHost/Visage.AppHost.csproj
- Visage.ServiceDefaults/Visage.ServiceDefaults.csproj
- All service and frontend projects
- All test projects

### Package Versions
Updated `Directory.Packages.props` with .NET 8 compatible package versions:
- Microsoft.AspNetCore.* packages: 8.0.11
- Microsoft.EntityFrameworkCore.* packages: 8.0.11
- Microsoft.Extensions.* packages: 8.0.x
- StrictId packages: 1.1.0
- Aspire packages: 8.0.0-preview.1.23557.2

### CI/CD
Updated `.github/workflows/dotnet.yml` to use .NET 8.0.x

## Current Status

✅ **Working**: Core infrastructure projects build successfully
- Visage.ServiceDefaults
- Visage.Shared
- Services (with package compatibility fixes)

⚠️ **Needs Work**: Some frontend projects use .NET 9-specific APIs
- AssignedRenderMode
- RendererInfo
- Other Blazor .NET 9 features

## Validation

Use the provided PowerShell script to validate the setup:
```bash
pwsh ./validate-dotnet-version.ps1
```

This script:
1. Checks that the actual SDK version matches global.json
2. Tests building core projects
3. Reports success/failure status

## Migration Notes

### From .NET 9 to .NET 8
The following changes were necessary:
1. Downgraded all package versions to .NET 8 compatible versions
2. Commented out .NET 9-specific Service Discovery API usage
3. Updated StrictId package to earlier version (1.1.0)

### Aspire Compatibility
- Used preview versions of Aspire packages for .NET 8
- Disabled IsAspireHost flag to avoid workload issues
- Some Aspire features may have limited functionality in .NET 8

## Future Considerations

### Upgrading to .NET 9
When ready to upgrade:
1. Update global.json SDK version to 9.0.x
2. Update all target frameworks to net9.0
3. Upgrade package versions in Directory.Packages.props
4. Restore .NET 9-specific API usage
5. Test all projects build and run correctly

### .NET 10 (Future)
The original issue mentioned ".NET version to 10", but .NET 10 is projected for November 2025. When available:
1. Follow similar upgrade process as .NET 9
2. Update global.json and target frameworks
3. Upgrade packages to .NET 10 compatible versions

## Troubleshooting

### Build Failures
If builds fail after version changes:
1. Check `dotnet --version` matches global.json
2. Run `dotnet clean` and `dotnet restore`
3. Verify package versions are compatible with target framework
4. Check for framework-specific API usage

### Workload Issues
Some features (MAUI, Aspire) may require additional workloads:
```bash
dotnet workload install aspire
dotnet workload install wasi-experimental
```

## References

- Issue #187: Pin the .NET version to 10 for the solution
- [.NET global.json documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/global-json)
- [.NET SDK rollForward policies](https://docs.microsoft.com/en-us/dotnet/core/tools/global-json#rollforward)