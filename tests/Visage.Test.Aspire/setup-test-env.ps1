# Visage E2E Test Environment Setup Script
# This script sets environment variables required for E2E Playwright tests

Write-Host "🔧 Setting up Visage E2E Test Environment Variables" -ForegroundColor Cyan
Write-Host ""

# Prompt for Auth0 configuration
Write-Host "📋 Auth0 Configuration (from Auth0 Dashboard → Applications → Your App)" -ForegroundColor Yellow
Write-Host ""

$auth0Domain = Read-Host "Enter AUTH0_DOMAIN (e.g., your-tenant.auth0.com)"
$auth0ClientId = Read-Host "Enter AUTH0_CLIENT_ID (from Application settings)"
$auth0ClientSecret = Read-Host "Enter AUTH0_CLIENT_SECRET (from Application settings)" -AsSecureString
$auth0Audience = Read-Host "Enter AUTH0_AUDIENCE (your API identifier)"

Write-Host ""
Write-Host "👤 Test User Credentials (whitelisted test account)" -ForegroundColor Yellow
Write-Host ""

$testUserEmail = Read-Host "Enter TEST_USER_EMAIL (e.g., test.playwright@hackmum.in)"
$testUserPassword = Read-Host "Enter TEST_USER_PASSWORD" -AsSecureString

Write-Host ""
Write-Host "🌐 Application Configuration" -ForegroundColor Yellow
Write-Host ""

$baseUrl = Read-Host "Enter TEST_BASE_URL (default: https://localhost:5001)"
if ([string]::IsNullOrWhiteSpace($baseUrl)) {
    $baseUrl = "https://localhost:5001"
}

# Convert SecureString to plain text for environment variables
$auth0ClientSecretPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($auth0ClientSecret)
)
$testUserPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($testUserPassword)
)

# Set environment variables for current session
Write-Host ""
Write-Host "✅ Setting environment variables for current PowerShell session..." -ForegroundColor Green

$env:AUTH0_DOMAIN = $auth0Domain
$env:AUTH0_CLIENT_ID = $auth0ClientId
$env:AUTH0_CLIENT_SECRET = $auth0ClientSecretPlain
$env:AUTH0_AUDIENCE = $auth0Audience
$env:TEST_USER_EMAIL = $testUserEmail
$env:TEST_USER_PASSWORD = $testUserPasswordPlain
$env:TEST_BASE_URL = $baseUrl

Write-Host "✅ Environment variables set successfully!" -ForegroundColor Green
Write-Host ""

# Display verification
Write-Host "📊 Current Configuration:" -ForegroundColor Cyan
Write-Host "  AUTH0_DOMAIN:       $env:AUTH0_DOMAIN"
Write-Host "  AUTH0_CLIENT_ID:    $env:AUTH0_CLIENT_ID"
Write-Host "  AUTH0_CLIENT_SECRET: ******** (hidden)"
Write-Host "  AUTH0_AUDIENCE:     $env:AUTH0_AUDIENCE"
Write-Host "  TEST_USER_EMAIL:    $env:TEST_USER_EMAIL"
Write-Host "  TEST_USER_PASSWORD: ******** (hidden)"
Write-Host "  TEST_BASE_URL:      $env:TEST_BASE_URL"
Write-Host ""

# Offer to save to user secrets (optional)
Write-Host "💾 Save to .NET User Secrets?" -ForegroundColor Yellow
Write-Host "   This stores credentials securely on your machine (recommended for local dev)" -ForegroundColor Gray
$saveToSecrets = Read-Host "Save to user secrets? (y/N)"

if ($saveToSecrets -eq 'y' -or $saveToSecrets -eq 'Y') {
    Write-Host ""
    Write-Host "Saving to user secrets..." -ForegroundColor Green
    
    dotnet user-secrets set "Auth0:Domain" $auth0Domain --project tests/Visage.Test.Aspire
    dotnet user-secrets set "Auth0:ClientId" $auth0ClientId --project tests/Visage.Test.Aspire
    dotnet user-secrets set "Auth0:ClientSecret" $auth0ClientSecretPlain --project tests/Visage.Test.Aspire
    dotnet user-secrets set "Auth0:Audience" $auth0Audience --project tests/Visage.Test.Aspire
    dotnet user-secrets set "TestUser:Email" $testUserEmail --project tests/Visage.Test.Aspire
    dotnet user-secrets set "TestUser:Password" $testUserPasswordPlain --project tests/Visage.Test.Aspire
    dotnet user-secrets set "TestUser:BaseUrl" $baseUrl --project tests/Visage.Test.Aspire
    
    Write-Host "✅ Saved to user secrets!" -ForegroundColor Green
}

Write-Host ""
Write-Host "🚀 Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Ensure Auth0 Action whitelist is configured (see README.md)"
Write-Host "  2. Start Aspire app:  dotnet run --project Visage.AppHost/Visage.AppHost.csproj"
Write-Host "  3. Run E2E tests:     dotnet test --filter `"Category=E2E`""
Write-Host ""
Write-Host "⚠️  Note: Environment variables are set for this PowerShell session only." -ForegroundColor Yellow
Write-Host "    Run this script again in new terminal sessions, or use user secrets." -ForegroundColor Gray
Write-Host ""

# Clear sensitive data from memory
$auth0ClientSecretPlain = $null
$testUserPasswordPlain = $null
