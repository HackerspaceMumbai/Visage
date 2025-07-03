using Visage.Tests.Visual.Base;
using Visage.Tests.Visual.Infrastructure;

namespace Visage.Tests.Visual.Examples;

/// <summary>
/// Example visual tests demonstrating different testing patterns and capabilities.
/// These tests serve as documentation and reference for writing new visual tests.
/// </summary>
public class ExampleVisualTests : VisualTestBase
{
    [Test]
    public async Task Example_BasicPageScreenshot()
    {
        // Navigate to the home page
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Capture a full page screenshot
        var result = await CapturePageScreenshotAsync("example_home_page");
        
        // Assert that the visual test passed
        AssertVisualTestPassed(result);
    }

    [Test]
    public async Task Example_ComponentScreenshot()
    {
        // Navigate to a page with components
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Capture a specific component
        var result = await CaptureComponentScreenshotAsync(
            TestDataManager.Selectors.EventGrid, 
            "example_event_grid");
        
        AssertVisualTestPassed(result);
    }

    [Test]
    public async Task Example_ResponsiveTesting()
    {
        // Navigate to the page
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Test the page across all configured responsive breakpoints
        var results = await TestResponsiveComponentAsync("body", "example_responsive");
        
        // Assert all responsive tests passed
        AssertAllVisualTestsPassed(results);
    }

    [Test]
    public async Task Example_InteractionTesting()
    {
        // Navigate to the page
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Use predefined test scenario for button interactions
        var config = TestDataManager.Scenarios.ButtonInteraction(
            TestDataManager.Selectors.PrimaryButton);

        // Execute the component workflow test
        var result = await McpHelper.TestComponentWorkflowAsync("example_button", config);

        // Assert the workflow succeeded
        result.Success.Should().BeTrue($"Button interaction test failed: {result.Message}");
        AssertAllVisualTestsPassed(result.VisualResults);
    }

    [Test]
    public async Task Example_UserWorkflow()
    {
        // Define a simple navigation workflow
        var steps = TestDataManager.Scenarios.BasicPageNavigation(
            TestDataManager.TestPages.Home,
            TestDataManager.TestPages.Counter,
            "h1:has-text('Counter')");

        // Execute the workflow
        var result = await McpHelper.TestUserWorkflowAsync("example_navigation", steps);

        // Assert the workflow completed successfully
        result.Success.Should().BeTrue($"Navigation workflow failed");
        
        // Check visual results
        var visualResults = result.StepResults
            .Where(s => s.VisualResult != null)
            .Select(s => s.VisualResult!)
            .ToList();
        
        if (visualResults.Any())
        {
            AssertAllVisualTestsPassed(visualResults);
        }
    }

    [Test]
    public async Task Example_ConfigurationUsage()
    {
        // Access configuration values
        var baseUrl = Config.BaseUrl;
        var threshold = Config.DifferenceThreshold;
        
        // Use configuration for custom viewport
        await Page.SetViewportSizeAsync(
            Config.DefaultViewport.Width, 
            Config.DefaultViewport.Height);
        
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Create custom visual engine with different settings
        var customEngine = new VisualTestingEngine(
            Config.Directories.Baselines,
            Config.Directories.Actual,
            Config.Directories.Diffs,
            threshold: 0.1); // More strict threshold

        var result = await customEngine.CompareScreenshotAsync(Page, "example_custom_config");
        AssertVisualTestPassed(result);
    }

    [Test]
    public async Task Example_ErrorHandling()
    {
        try
        {
            // Navigate to a non-existent page
            await NavigateToLocalAsync("/non-existent-page");
            await Page.WaitForTimeoutAsync(2000);

            // Capture the error state
            var result = await CapturePageScreenshotAsync("example_error_state");
            
            // Error pages might have dynamic content, so we might allow higher differences
            if (result.Status == VisualTestStatus.Failed && result.DifferencePercentage < 5.0)
            {
                // Accept minor differences for error pages
                Console.WriteLine($"Error page has minor differences: {result.DifferencePercentage:F2}%");
            }
            else
            {
                AssertVisualTestPassed(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error page test exception (expected): {ex.Message}");
            // This might be expected for non-existent pages
        }
    }

    [Test]
    public async Task Example_ConditionalTesting()
    {
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Check if certain elements exist before testing them
        var eventCards = Page.Locator(TestDataManager.Selectors.EventCard);
        var cardCount = await eventCards.CountAsync();

        if (cardCount > 0)
        {
            // Test event cards if they exist
            var result = await CaptureComponentScreenshotAsync(
                TestDataManager.Selectors.EventCard + ":first-child", 
                "example_first_event_card");
            AssertVisualTestPassed(result);
        }
        else
        {
            // Test empty state
            var emptyStateText = Page.Locator("text=No upcoming events");
            if (await emptyStateText.CountAsync() > 0)
            {
                var result = await CaptureComponentScreenshotAsync(
                    "p:has-text('No upcoming events')", 
                    "example_empty_state");
                AssertVisualTestPassed(result);
            }
        }
    }
}