param(
    [switch]$RunAuth
)

Write-Host "Running default tests (non-auth smoke & integration)"
dotnet test --filter "Category!=RequiresAuth&Category!=AspireHealth"
$code = $LastExitCode
if ($code -ne 0) { throw "Default tests failed with exit code $code" }

if ($RunAuth) {
    if (-not $env:AUTH0_DOMAIN) {
        Write-Host "AUTH0 environment not set; skipping RequiresAuth tests" -ForegroundColor Yellow
    }
    else {
        Write-Host "Running RequiresAuth tests"
        dotnet test --filter "Category=RequiresAuth"
        if ($LastExitCode -ne 0) { throw "RequiresAuth tests failed with exit code $LastExitCode" }
    }
}

Write-Host "All tests complete."
