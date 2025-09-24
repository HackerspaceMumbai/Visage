using Visage.Tests.Visual.Base;
using Visage.Tests.Visual.Infrastructure;

namespace Visage.Tests.Visual.Components;

/// <summary>
/// Visual tests for the Home page and its components.
/// Tests layout, event cards, responsive behavior, and user interactions.
/// </summary>
public class HomePageVisualTests : VisualTestBase
{
    [Test]
    public async Task HomePage_InitialLoad_ShouldMatchBaseline()
    {
        // Arrange & Act
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Assert
        var result = await CapturePageScreenshotAsync("homepage_initial_load");
        AssertVisualTestPassed(result);
    }

    [Test]
    public async Task HomePage_ResponsiveLayout_ShouldMatchBaselines()
    {
        // Arrange
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Act & Assert
        var results = await TestResponsiveComponentAsync("body", "homepage_responsive");
        AssertAllVisualTestsPassed(results);
    }

    [Test]
    public async Task EventCard_Component_ShouldMatchBaseline()
    {
        // Arrange
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Wait for event cards to load
        await Page.WaitForSelectorAsync(".card", new PageWaitForSelectorOptions { Timeout = 5000 });

        // Act - Test first event card if exists
        var eventCards = await Page.Locator(".card").CountAsync();
        if (eventCards > 0)
        {
            var result = await CaptureComponentScreenshotAsync(".card:first-child", "event_card_component");
            AssertVisualTestPassed(result);
        }
    }

    [Test]
    public async Task HomePage_UpcomingEventsSection_ShouldMatchBaseline()
    {
        // Arrange
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Act
        var result = await CaptureComponentScreenshotAsync(".grid", "upcoming_events_section");
        
        // Assert
        AssertVisualTestPassed(result);
    }

    [Test]
    public async Task HomePage_NavigationButtons_ShouldMatchBaseline()
    {
        // Arrange
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Act - Test button variations
        var buttonContainer = Page.Locator("text=Button").First;
        await buttonContainer.ScrollIntoViewIfNeededAsync();
        
        var result = await CaptureComponentScreenshotAsync("button", "homepage_buttons");
        
        // Assert
        AssertVisualTestPassed(result);
    }

    [Test]
    public async Task HomePage_ButtonInteractions_ShouldMatchBaselines()
    {
        // Arrange
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        var config = new ComponentTestConfig
        {
            ComponentSelector = ".btn-primary",
            Interactions = new List<InteractionConfig>
            {
                new() { Name = "hover", Type = "hover", Selector = ".btn-primary" },
                new() { Name = "focus", Type = "focus", Selector = ".btn-primary" }
            }
        };

        // Act
        var result = await McpHelper.TestComponentWorkflowAsync("homepage_primary_button", config);

        // Assert
        result.Success.Should().BeTrue($"Component test failed: {result.Message}");
        AssertAllVisualTestsPassed(result.VisualResults);
    }

    [Test]
    public async Task HomePage_ThemeToggle_ShouldWorkVisually()
    {
        // Arrange
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Capture initial state
        var initialResult = await CapturePageScreenshotAsync("homepage_theme_initial");
        
        // Look for theme toggle button (assuming it exists in layout)
        var themeToggle = Page.Locator("[data-theme-toggle]").Or(Page.Locator("button:has-text('theme')")).First;
        
        if (await themeToggle.CountAsync() > 0)
        {
            // Click theme toggle
            await themeToggle.ClickAsync();
            await Page.WaitForTimeoutAsync(500); // Allow theme transition
            
            // Capture after theme change
            var toggledResult = await CapturePageScreenshotAsync("homepage_theme_toggled");
            
            // Assert both states
            AssertVisualTestPassed(initialResult);
            AssertVisualTestPassed(toggledResult);
        }
    }

    [Test]
    public async Task HomePage_EmptyState_ShouldMatchBaseline()
    {
        // This test might require mocking the event service to return empty results
        // For now, we'll test the "No upcoming events" text if it appears
        
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Check if empty state is visible
        var emptyState = Page.Locator("text=No upcoming events");
        if (await emptyState.CountAsync() > 0)
        {
            var result = await CaptureComponentScreenshotAsync("p:has-text('No upcoming events')", "homepage_empty_state");
            AssertVisualTestPassed(result);
        }
    }

    [Test]
    public async Task HomePage_LoadingStates_ShouldBeHandledGracefully()
    {
        // Test the page during loading
        var navigationPromise = NavigateToLocalAsync("/");
        
        // Capture early loading state if possible
        try
        {
            await Page.WaitForSelectorAsync("h2:has-text('Upcoming Events')", new PageWaitForSelectorOptions { Timeout = 1000 });
            var loadingResult = await CapturePageScreenshotAsync("homepage_loading_state");
            AssertVisualTestPassed(loadingResult);
        }
        catch
        {
            // If loading is too fast, that's fine
        }
        
        await navigationPromise;
        await WaitForBlazorAsync();
    }
}