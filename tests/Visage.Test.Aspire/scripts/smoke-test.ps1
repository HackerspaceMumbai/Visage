param()

Write-Host "Running smoke tests (home page)..."
dotnet test --filter "Category=Smoke"

if ($LastExitCode -ne 0) {
    Write-Host "Smoke tests failed with exit code $LastExitCode" -ForegroundColor Red
    exit $LastExitCode
}
Write-Host "Smoke tests passed." -ForegroundColor Green
