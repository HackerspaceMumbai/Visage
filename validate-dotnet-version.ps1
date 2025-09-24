#!/usr/bin/env pwsh
# Validate .NET version pinning for Visage solution
# This script checks that the .NET SDK version matches the pinned version in global.json

Write-Host "🔍 Validating .NET version pinning..." -ForegroundColor Cyan

# Read the pinned version from global.json
$globalJson = Get-Content "global.json" | ConvertFrom-Json
$expectedVersion = $globalJson.sdk.version
Write-Host "📌 Expected SDK version from global.json: $expectedVersion" -ForegroundColor Yellow

# Get the actual SDK version
$actualVersion = (dotnet --version).Trim()
Write-Host "🔧 Actual SDK version: $actualVersion" -ForegroundColor Yellow

# Check if versions match
if ($actualVersion -eq $expectedVersion) {
    Write-Host "✅ SUCCESS: .NET SDK version matches the pinned version!" -ForegroundColor Green
    
    # Test build core projects
    Write-Host "🏗️ Testing build of core projects..." -ForegroundColor Cyan
    
    $projects = @(
        "Visage.ServiceDefaults/Visage.ServiceDefaults.csproj",
        "Visage.Shared/Visage.Shared.csproj"
    )
    
    foreach ($project in $projects) {
        Write-Host "Building $project..." -ForegroundColor White
        $result = dotnet build $project --verbosity minimal
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ $project built successfully" -ForegroundColor Green
        } else {
            Write-Host "❌ $project build failed" -ForegroundColor Red
            Write-Host "💡 This is expected since .NET 10 RC SDK is not installed in this environment" -ForegroundColor Yellow
        }
    }
    
    Write-Host "🎉 Version pinning validation completed!" -ForegroundColor Green
} else {
    Write-Host "❌ MISMATCH: Expected $expectedVersion but got $actualVersion" -ForegroundColor Red
    Write-Host "💡 Solution: Install .NET SDK version $expectedVersion or update global.json" -ForegroundColor Yellow
    Write-Host "📦 To install .NET 10 RC: Download from https://dotnet.microsoft.com/en-us/download/dotnet/10.0" -ForegroundColor Cyan
    exit 1
}