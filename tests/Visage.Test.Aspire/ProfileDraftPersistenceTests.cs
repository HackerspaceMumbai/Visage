using FluentAssertions;
using Microsoft.Playwright;

namespace Visage.Test.Aspire;

/// <summary>
/// E2E tests for Profile draft persistence workflow.
/// Covers: save draft, restore draft, delete draft, UI indicators.
/// 
/// Run all E2E tests:
///   dotnet test --filter "Category=E2E"
/// 
/// Run only draft persistence tests:
///   dotnet test --filter "Category=DraftPersistence"
/// 
/// Skip E2E tests (run only unit/integration):
///   dotnet test --filter "Category!=E2E"
/// 
/// Prerequisites:
/// 1. Install Playwright browsers: pwsh tests/Visage.Test.Aspire/bin/Debug/net10.0/playwright.ps1 install
/// 2. Configure Auth0 (see tests/Visage.Test.Aspire/README.md for security setup)
/// 3. Set environment variables: TEST_USER_EMAIL, TEST_USER_PASSWORD, TEST_BASE_URL, AUTH0_DOMAIN, AUTH0_CLIENT_ID, AUTH0_CLIENT_SECRET, AUTH0_AUDIENCE
/// 4. Start Aspire app: dotnet run --project Visage.AppHost/Visage.AppHost.csproj
/// 5. Run tests: dotnet test --filter "Category=E2E"
/// </summary>
// Requires Auth0 - these E2E Playwright tests use Auth0 resource owner grant and should
// be executed explicitly when Auth0 is available for CI or local run.
[Category("E2E")]
[Category("DraftPersistence")]
[Category("RequiresAuth")]
[AuthRequired]
[NotInParallel] // E2E tests should run sequentially to avoid state conflicts
public class ProfileDraftPersistenceTests : IAsyncDisposable
{
    // Configuration: Set these via environment variables or user secrets
    private static readonly string BaseUrl = Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "https://localhost:5001";
    
    // Playwright instances - managed manually for full control
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    
    /// <summary>
    /// Initialize Playwright browser before each test.
    /// </summary>
    [Before(HookType.Test)]
    public async Task SetupAsync()
    {
        // Create Playwright instance
        _playwright = await Playwright.CreateAsync();
        
        // Launch browser (use Chromium by default, can switch to Firefox/WebKit)
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true // Set to false for debugging
        });
        
        // Create browser context with settings
        _context = await _browser.NewContextAsync(new()
        {
            IgnoreHTTPSErrors = true, // Accept self-signed certificates for local development
            ViewportSize = new() { Width = 1920, Height = 1080 }
        });
        
        // Create new page
        _page = await _context.NewPageAsync();
    }
    
    /// <summary>
    /// Cleanup Playwright resources after each test.
    /// </summary>
    [After(HookType.Test)]
    public async Task TeardownAsync()
    {
        if (_page != null)
        {
            await _page.CloseAsync();
            _page = null;
        }
        
        if (_context != null)
        {
            await _context.CloseAsync();
            _context = null;
        }
        
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }
        
        _playwright?.Dispose();
        _playwright = null;
    }
    
    public async ValueTask DisposeAsync()
    {
        await TeardownAsync();
    }

    [Test]
    [Category("Smoke")] // Add smoke test category for quick sanity checks
    public async Task WhenUserFillsProfileThenDraftSavesAndRestores()
    {
        AuthTestGuard.RequireAuthConfigured();
        // Ensure page is initialized by setup
        if (_page == null || _context == null)
            throw new InvalidOperationException("Page not initialized. Setup did not run.");
        
        // 1. Authenticate via Auth0 token (bypasses UI login for reliability)
        var accessToken = await TestAppContext.GetAuthTokenAsync();

        // 2. Navigate to profile page first (this ensures Context is initialized)
        await _page.GotoAsync($"{BaseUrl}/profile/edit");
        
        // 3. Set authentication header via context
        await _context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {accessToken}"
        });
        
        // 4. Reload page to apply authentication headers
        await _page.ReloadAsync();

        // 5. Wait for page to be ready
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 6. Fill in profile fields to trigger auto-save
        await _page.FillAsync("input[name='firstName']", "DraftTest");
        await _page.FillAsync("input[name='lastName']", "User");

        // 7. Wait for auto-save timer (default is 3000ms + processing time)
        await _page.WaitForTimeoutAsync(4000);

        // 8. Verify "Draft saved" indicator appears
        var draftSavedIndicator = _page.GetByText("Draft saved");
        await draftSavedIndicator.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        (await draftSavedIndicator.IsVisibleAsync()).Should().BeTrue("Draft saved indicator should be visible after auto-save");

        // 9. Reload page to confirm draft restoration
        await _page.ReloadAsync();

        // 10. Verify "Draft restored" indicator appears
        var draftRestoredIndicator = _page.GetByText("Draft restored");
        await draftRestoredIndicator.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        (await draftRestoredIndicator.IsVisibleAsync()).Should().BeTrue("Draft restored indicator should be visible after page reload");

        // 11. Verify fields are populated with draft data
        var firstNameValue = await _page.InputValueAsync("input[name='firstName']");
        var lastNameValue = await _page.InputValueAsync("input[name='lastName']");
        firstNameValue.Should().Be("DraftTest", "First name should be restored from draft");
        lastNameValue.Should().Be("User", "Last name should be restored from draft");
    }

    [Test]
    [Category("Full")] // Full workflow test
    public async Task WhenUserDeletesDraftThenFieldsAreCleared()
    {
        AuthTestGuard.RequireAuthConfigured();
        // Ensure page is initialized by setup
        if (_page == null || _context == null)
            throw new InvalidOperationException("Page not initialized. Setup did not run.");
        
        // 1. Authenticate via Auth0 token
        var accessToken = await TestAppContext.GetAuthTokenAsync();
        
        // 2. Navigate to profile page first
        await _page.GotoAsync($"{BaseUrl}/profile/edit");
        
        // 3. Set authentication header
        await _context.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {accessToken}"
        });
        
        // 4. Reload page to apply authentication
        await _page.ReloadAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 5. Fill in profile fields and wait for auto-save
        await _page.FillAsync("input[name='firstName']", "ToDelete");
        await _page.FillAsync("input[name='lastName']", "Draft");
        await _page.WaitForTimeoutAsync(4000);

        // 6. Verify draft saved
        await _page.GetByText("Draft saved").WaitForAsync(new() { State = WaitForSelectorState.Visible });

        // 7. Delete the draft
        var deleteDraftButton = _page.GetByRole(AriaRole.Button, new() { Name = "Delete draft" });
        await deleteDraftButton.ClickAsync();

        // 8. Verify draft restored indicator disappears (or is not present)
        var draftIndicators = _page.GetByText("Draft restored");
        (await draftIndicators.IsVisibleAsync()).Should().BeFalse("Draft restored indicator should not be visible after deletion");

        // 9. Verify fields are cleared
        var firstNameValue = await _page.InputValueAsync("input[name='firstName']");
        var lastNameValue = await _page.InputValueAsync("input[name='lastName']");
        firstNameValue.Should().BeEmpty("First name should be cleared after draft deletion");
        lastNameValue.Should().BeEmpty("Last name should be cleared after draft deletion");
    }
}
