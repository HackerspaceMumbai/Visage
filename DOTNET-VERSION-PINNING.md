# .NET Version Pinning for Visage

This document describes the .NET version pinning implementation for the Visage solution.

## Overview

The Visage solution has been configured to pin the .NET SDK version to ensure consistent builds across all development environments. This addresses issue #187 "Pin the .NET version to 10 for the solution".

## Current Configuration

-- **Pinned SDK Version**: 10.0.100-rc.1 (full SDK identifier in `global.json` may be `10.0.100-rc.1.25451.107`)
- **Target Framework**: net10.0 (across all projects)
- **Rollforward Policy**: latestPatch

## Files Modified

### global.json
Created to pin the .NET SDK version. Note that `global.json` may include a full SDK identifier with patch/build metadata. Example:
```json
{
  "sdk": {
    "version": "10.0.100-rc.1.25451.107",
    "rollForward": "latestPatch"
  }
}
```

### Project Files
All `.csproj` files updated to target `net10.0`:
- Visage.AppHost/Visage.AppHost.csproj
- Visage.ServiceDefaults/Visage.ServiceDefaults.csproj
- All service and frontend projects
- All test projects

### Package Versions
Updated `Directory.Packages.props` with .NET 10 RC compatible package versions:
- Microsoft.AspNetCore.* packages: 10.0.0-rc.1
- Microsoft.EntityFrameworkCore.* packages: 10.0.0-rc.1
- Microsoft.Extensions.* packages: 10.0.0-rc.1
- StrictId packages: 1.3.0
- Aspire packages: 10.0.0-rc.1

### CI/CD
Updated `.github/workflows/dotnet.yml` to use .NET 10.0.x

## Installation Requirements

⚠️ **Important**: This solution now requires .NET 10 RC SDK to be installed.

### Installing .NET 10 RC
1. Visit the official .NET download page: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
2. Download and install the .NET 10 RC SDK for your platform
3. Verify installation: `dotnet --version` will show the full installed SDK version. When `global.json` pins a preview SDK and `rollForward` is enabled you may see a string like:

- `10.0.100-rc.1.25451.107` (full SDK identifier), or
- a short form with additional patch/build segments, e.g. `10.0.100-rc.1.xxxxx.xxx`.

If you only see `10.0.100-rc.1` without the patch segments, the SDK installed matches the short identifier exactly. Otherwise expect the full patch/build identifier when roll-forward is in effect.

## Current Status

✅ **Ready for .NET 10 RC**: All project configurations updated
✅ **Package Versions**: Updated to .NET 10 RC compatible versions
✅ **Service Discovery**: Restored to use full API (compatible with .NET 10)
⚠️ **Requires Installation**: .NET 10 RC SDK must be installed to build

## Validation

Use the provided PowerShell script to validate the setup:
```bash
pwsh ./validate-dotnet-version.ps1
```

This script:
1. Checks that the actual SDK version matches `global.json` (compare full version strings when available)
2. Provides guidance if .NET 10 RC is not installed
3. Tests building core projects (when SDK is available)

## Migration History

### From .NET 8 to .NET 10 RC
The following changes were made:
1. Updated global.json SDK version to 10.0.100-rc.1
2. Updated all project target frameworks from net8.0 to net10.0
3. Upgraded all package versions to 10.0.0-rc.1
4. Restored Service Discovery API usage (was commented out for .NET 8)
5. Updated CI/CD pipeline to use .NET 10.0.x

### Aspire Integration
- Updated to use .NET 10 RC versions of Aspire packages
- Restored full Aspire functionality
- Service Discovery API fully enabled

## Troubleshooting

### Build Failures
If builds fail after version changes:
1. Ensure .NET 10 RC SDK is installed: `dotnet --version`
2. Check that version matches global.json (10.0.100-rc.1)
3. Run `dotnet clean` and `dotnet restore`
4. If SDK not installed, download from: https://dotnet.microsoft.com/en-us/download/dotnet/10.0

### Package Compatibility
All packages have been updated to RC versions. If specific packages are not available:
1. Check NuGet.org for latest RC versions
2. Update Directory.Packages.props accordingly
3. Some packages may still be in preview - this is expected for RC releases

## Future Considerations

### Upgrading to .NET 10 RTM
When .NET 10 RTM is released:
1. Update global.json SDK version to 10.0.100
2. Update package versions from RC to RTM versions
3. Test all functionality with RTM release

## References

- Issue #187: Pin the .NET version to 10 for the solution
- [.NET 10 RC-1 Blog Post](https://devblogs.microsoft.com/dotnet/dotnet-10-rc-1/)
- [.NET 10 Download](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [.NET global.json documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/global-json)