using Microsoft.Playwright;
using TUnit.Playwright;
using Visage.Tests.Visual.Infrastructure;

namespace Visage.Tests.Visual.Base;

/// <summary>
/// Base class for visual testing that provides common setup and utilities.
/// Integrates Playwright MCP with TUnit testing framework.
/// </summary>
public abstract class VisualTestBase : PageTest
{
    protected VisualTestingEngine VisualEngine { get; private set; } = null!;
    protected PlaywrightMcpHelper McpHelper { get; private set; } = null!;

    [Before(Test)]
    public async Task SetupVisualTesting()
    {
        // Initialize visual testing engine
        VisualEngine = new VisualTestingEngine();
        
        // Initialize MCP helper
        McpHelper = new PlaywrightMcpHelper(Page, VisualEngine);

        // Configure page for consistent testing
        await ConfigurePageForTesting();
    }

    /// <summary>
    /// Configures the page with consistent settings for visual testing
    /// </summary>
    protected virtual async Task ConfigurePageForTesting()
    {
        // Set consistent viewport for testing
        await Page.SetViewportSizeAsync(1280, 720);
        
        // Disable animations for consistent screenshots
        await Page.AddInitScriptAsync(@"
            CSS.supports('animation-duration', '0s') && 
            document.addEventListener('DOMContentLoaded', () => {
                const style = document.createElement('style');
                style.innerHTML = `
                    *, *::before, *::after {
                        animation-duration: 0s !important;
                        animation-delay: 0s !important;
                        transition-duration: 0s !important;
                        transition-delay: 0s !important;
                    }
                `;
                document.head.appendChild(style);
            });
        ");

        // Set consistent font rendering
        await Page.EmulateMediaAsync(new PageEmulateMediaOptions
        {
            ReducedMotion = ReducedMotion.Reduce
        });
    }

    /// <summary>
    /// Navigates to a local development URL and waits for the page to be ready
    /// </summary>
    protected async Task NavigateToLocalAsync(string path = "/")
    {
        var baseUrl = GetBaseUrl();
        var fullUrl = new Uri(new Uri(baseUrl), path).ToString();
        
        await Page.GotoAsync(fullUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for any initial renders or hydration
        await Page.WaitForTimeoutAsync(1000);
    }

    /// <summary>
    /// Gets the base URL for testing. Can be overridden for different environments.
    /// </summary>
    protected virtual string GetBaseUrl()
    {
        // Try to get from environment variable first (for CI/CD)
        var envUrl = Environment.GetEnvironmentVariable("VISAGE_TEST_URL");
        if (!string.IsNullOrEmpty(envUrl))
        {
            return envUrl;
        }

        // Default to local development
        return "https://localhost:7150";
    }

    /// <summary>
    /// Waits for Blazor to be ready and hydrated
    /// </summary>
    protected async Task WaitForBlazorAsync()
    {
        // Wait for Blazor to be loaded and any initial rendering to complete
        await Page.WaitForFunctionAsync(@"
            () => window.Blazor && window.Blazor._internal && 
                  document.querySelector('[data-enhanced-load]') === null
        ");
    }

    /// <summary>
    /// Captures a full page screenshot with a descriptive name
    /// </summary>
    protected async Task<VisualTestResult> CapturePageScreenshotAsync(string testName)
    {
        return await VisualEngine.CompareScreenshotAsync(Page, testName, new PageScreenshotOptions
        {
            FullPage = true,
            Type = ScreenshotType.Png
        });
    }

    /// <summary>
    /// Captures a screenshot of a specific component
    /// </summary>
    protected async Task<VisualTestResult> CaptureComponentScreenshotAsync(string selector, string testName)
    {
        var element = Page.Locator(selector);
        await element.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        
        return await VisualEngine.CompareElementScreenshotAsync(element, testName);
    }

    /// <summary>
    /// Tests a component across multiple responsive breakpoints
    /// </summary>
    protected async Task<List<VisualTestResult>> TestResponsiveComponentAsync(string selector, string componentName)
    {
        var breakpoints = new[]
        {
            new { Name = "mobile", Width = 375, Height = 667 },
            new { Name = "tablet", Width = 768, Height = 1024 },
            new { Name = "desktop", Width = 1280, Height = 720 },
            new { Name = "large", Width = 1920, Height = 1080 }
        };

        var results = new List<VisualTestResult>();

        foreach (var breakpoint in breakpoints)
        {
            await Page.SetViewportSizeAsync(breakpoint.Width, breakpoint.Height);
            await Page.WaitForTimeoutAsync(500); // Allow for responsive transitions

            var result = await CaptureComponentScreenshotAsync(selector, $"{componentName}_{breakpoint.Name}");
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Helper to assert that visual test results passed
    /// </summary>
    protected static void AssertVisualTestPassed(VisualTestResult result)
    {
        if (result.Status == VisualTestStatus.Failed)
        {
            throw new AssertionException(
                $"Visual test '{result.TestName}' failed: {result.Message}. " +
                $"Difference: {result.DifferencePercentage:F2}%. " +
                $"Check diff image: {result.DiffPath}");
        }
    }

    /// <summary>
    /// Helper to assert that all visual test results passed
    /// </summary>
    protected static void AssertAllVisualTestsPassed(IEnumerable<VisualTestResult> results)
    {
        var failedTests = results.Where(r => r.Status == VisualTestStatus.Failed).ToList();
        
        if (failedTests.Any())
        {
            var failures = string.Join(", ", failedTests.Select(t => $"{t.TestName} ({t.DifferencePercentage:F2}%)"));
            throw new AssertionException($"Visual tests failed: {failures}");
        }
    }
}

public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
}