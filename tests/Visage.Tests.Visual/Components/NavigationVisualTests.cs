using Visage.Tests.Visual.Base;
using Visage.Tests.Visual.Infrastructure;

namespace Visage.Tests.Visual.Components;

/// <summary>
/// Visual tests for navigation components including NavMenu, NavHeader, and navigation interactions.
/// </summary>
public class NavigationVisualTests : VisualTestBase
{
    [Test]
    public async Task NavMenu_DefaultState_ShouldMatchBaseline()
    {
        // Arrange
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Act
        var result = await CaptureComponentScreenshotAsync("nav", "navigation_menu_default");

        // Assert
        AssertVisualTestPassed(result);
    }

    [Test]
    public async Task NavMenu_ResponsiveLayout_ShouldMatchBaselines()
    {
        // Arrange
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Act & Assert
        var results = await TestResponsiveComponentAsync("nav", "navigation_responsive");
        AssertAllVisualTestsPassed(results);
    }

    [Test]
    public async Task NavMenu_HoverStates_ShouldMatchBaselines()
    {
        // Arrange
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Find navigation links
        var navLinks = Page.Locator("nav a");
        var linkCount = await navLinks.CountAsync();

        var results = new List<VisualTestResult>();

        // Test hover state for each navigation link
        for (int i = 0; i < Math.Min(linkCount, 3); i++) // Test first 3 links
        {
            var link = navLinks.Nth(i);
            var linkText = await link.TextContentAsync();
            var safeLinkText = linkText?.Replace(" ", "_").ToLower() ?? $"link_{i}";

            // Hover over the link
            await link.HoverAsync();
            await Page.WaitForTimeoutAsync(200); // Allow for hover effects

            var result = await CaptureComponentScreenshotAsync("nav", $"navigation_hover_{safeLinkText}");
            results.Add(result);

            // Move away to clear hover state
            await Page.Mouse.MoveAsync(0, 0);
            await Page.WaitForTimeoutAsync(200);
        }

        // Assert
        AssertAllVisualTestsPassed(results);
    }

    [Test]
    public async Task NavMenu_ActiveStates_ShouldMatchBaselines()
    {
        // Test active states by navigating to different pages
        var testPages = new[]
        {
            new { Url = "/", Name = "home" },
            new { Url = "/counter", Name = "counter" }
        };

        var results = new List<VisualTestResult>();

        foreach (var testPage in testPages)
        {
            // Navigate to page
            await NavigateToLocalAsync(testPage.Url);
            await WaitForBlazorAsync();

            // Capture navigation state
            var result = await CaptureComponentScreenshotAsync("nav", $"navigation_active_{testPage.Name}");
            results.Add(result);
        }

        // Assert
        AssertAllVisualTestsPassed(results);
    }

    [Test]
    public async Task NavHeader_UserAuthentication_ShouldHandleStates()
    {
        // Test navigation header in different authentication states
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Capture unauthenticated state
        var unauthResult = await CaptureComponentScreenshotAsync("header", "nav_header_unauthenticated");
        AssertVisualTestPassed(unauthResult);

        // Note: Testing authenticated state would require actual authentication
        // This could be extended with mock authentication or test user setup
    }

    [Test]
    public async Task MobileNavigation_ToggleBehavior_ShouldMatchBaselines()
    {
        // Set mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        var results = new List<VisualTestResult>();

        // Capture closed mobile menu
        var closedResult = await CapturePageScreenshotAsync("mobile_navigation_closed");
        results.Add(closedResult);

        // Look for mobile menu toggle (hamburger button)
        var mobileToggle = Page.Locator("[aria-label*='menu']")
            .Or(Page.Locator("button:has([role='img'])")
            .Or(Page.Locator(".navbar-toggler")))
            .First;

        if (await mobileToggle.CountAsync() > 0)
        {
            // Click to open mobile menu
            await mobileToggle.ClickAsync();
            await Page.WaitForTimeoutAsync(500); // Allow for animation

            // Capture opened mobile menu
            var openResult = await CapturePageScreenshotAsync("mobile_navigation_opened");
            results.Add(openResult);

            // Click to close mobile menu
            await mobileToggle.ClickAsync();
            await Page.WaitForTimeoutAsync(500);

            // Capture closed again
            var closedAgainResult = await CapturePageScreenshotAsync("mobile_navigation_closed_again");
            results.Add(closedAgainResult);
        }

        // Assert
        AssertAllVisualTestsPassed(results);
    }

    [Test]
    public async Task ThemeToggler_Component_ShouldMatchBaselines()
    {
        // Arrange
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Look for theme toggler component
        var themeToggler = Page.Locator("[data-theme-toggle]")
            .Or(Page.Locator("button:has-text('theme')")
            .Or(Page.Locator(".theme-toggler")))
            .First;

        if (await themeToggler.CountAsync() > 0)
        {
            // Test different theme states
            var config = new ComponentTestConfig
            {
                ComponentSelector = "[data-theme-toggle]",
                Interactions = new List<InteractionConfig>
                {
                    new() { Name = "toggle_dark", Type = "click", Selector = "[data-theme-toggle]", WaitAfterMs = 1000 },
                    new() { Name = "toggle_light", Type = "click", Selector = "[data-theme-toggle]", WaitAfterMs = 1000 }
                }
            };

            var result = await McpHelper.TestComponentWorkflowAsync("theme_toggler", config);

            // Assert
            result.Success.Should().BeTrue($"Theme toggler test failed: {result.Message}");
            AssertAllVisualTestsPassed(result.VisualResults);
        }
    }

    [Test]
    public async Task Navigation_KeyboardAccessibility_ShouldBeVisible()
    {
        // Test keyboard navigation visual indicators
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        var results = new List<VisualTestResult>();

        // Find focusable navigation elements
        var focusableElements = Page.Locator("nav a, nav button");
        var elementCount = await focusableElements.CountAsync();

        // Test focus states
        for (int i = 0; i < Math.Min(elementCount, 3); i++)
        {
            var element = focusableElements.Nth(i);
            
            // Focus the element
            await element.FocusAsync();
            await Page.WaitForTimeoutAsync(200);

            // Capture focus state
            var elementText = await element.TextContentAsync();
            var safeElementText = elementText?.Replace(" ", "_").ToLower() ?? $"element_{i}";
            var result = await CaptureComponentScreenshotAsync("nav", $"navigation_focus_{safeElementText}");
            results.Add(result);
        }

        // Assert
        AssertAllVisualTestsPassed(results);
    }

    [Test]
    public async Task Navigation_BreadcrumbTrail_ShouldMatchBaseline()
    {
        // Test breadcrumb navigation if it exists
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        var breadcrumbs = Page.Locator(".breadcrumb, [aria-label*='breadcrumb'], nav[aria-label*='breadcrumb']");
        
        if (await breadcrumbs.CountAsync() > 0)
        {
            var result = await CaptureComponentScreenshotAsync(".breadcrumb", "navigation_breadcrumbs");
            AssertVisualTestPassed(result);
        }
    }
}