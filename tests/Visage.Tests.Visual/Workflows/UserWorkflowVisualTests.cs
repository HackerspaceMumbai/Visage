using Visage.Tests.Visual.Base;
using Visage.Tests.Visual.Infrastructure;

namespace Visage.Tests.Visual.Workflows;

/// <summary>
/// End-to-end visual workflow tests that verify complete user journeys.
/// These tests capture visual state at each step of common user workflows.
/// </summary>
public class UserWorkflowVisualTests : VisualTestBase
{
    [Test]
    public async Task EventBrowsingWorkflow_ShouldMatchBaselines()
    {
        // Define the workflow steps
        var steps = new List<WorkflowStep>
        {
            new()
            {
                Name = "navigate_to_home",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "navigate", Target = "/" }
                },
                WaitForMs = 2000,
                ExpectedElements = new List<string> { "h2:has-text('Upcoming Events')" }
            },
            new()
            {
                Name = "view_event_cards",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "waitfor", Target = ".card" }
                },
                WaitForMs = 1000,
                ExpectedElements = new List<string> { ".card" }
            },
            new()
            {
                Name = "hover_first_event",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "hover", Target = ".card:first-child" }
                },
                WaitForMs = 500
            },
            new()
            {
                Name = "scroll_to_past_events",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "waitfor", Target = "h2:has-text('Past Events')" }
                },
                WaitForMs = 1000
            }
        };

        // Execute the workflow
        var result = await McpHelper.TestUserWorkflowAsync("event_browsing", steps);

        // Assert
        result.Success.Should().BeTrue($"Event browsing workflow failed: {result.StepResults.FirstOrDefault(s => !s.Success)?.ErrorMessage}");
        
        var visualResults = result.StepResults
            .Where(s => s.VisualResult != null)
            .Select(s => s.VisualResult!)
            .ToList();
        
        AssertAllVisualTestsPassed(visualResults);
    }

    [Test]
    public async Task NavigationWorkflow_ShouldMatchBaselines()
    {
        var steps = new List<WorkflowStep>
        {
            new()
            {
                Name = "start_at_home",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "navigate", Target = "/" }
                },
                WaitForMs = 2000
            },
            new()
            {
                Name = "navigate_to_counter",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "click", Target = "a[href='/counter']" }
                },
                WaitForMs = 1000,
                ExpectedElements = new List<string> { "h1:has-text('Counter')" }
            },
            new()
            {
                Name = "interact_with_counter",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "click", Target = "button:has-text('Click me')" }
                },
                WaitForMs = 500
            },
            new()
            {
                Name = "return_to_home",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "click", Target = "a[href='/']" }
                },
                WaitForMs = 1000,
                ExpectedElements = new List<string> { "h2:has-text('Upcoming Events')" }
            }
        };

        var result = await McpHelper.TestUserWorkflowAsync("navigation_workflow", steps);

        // Assert
        result.Success.Should().BeTrue($"Navigation workflow failed: {result.StepResults.FirstOrDefault(s => !s.Success)?.ErrorMessage}");
        
        var visualResults = result.StepResults
            .Where(s => s.VisualResult != null)
            .Select(s => s.VisualResult!)
            .ToList();
        
        AssertAllVisualTestsPassed(visualResults);
    }

    [Test]
    public async Task ResponsiveWorkflow_ShouldMatchBaselines()
    {
        // Test workflow across different screen sizes
        var breakpoints = new[]
        {
            new { Name = "mobile", Width = 375, Height = 667 },
            new { Name = "tablet", Width = 768, Height = 1024 },
            new { Name = "desktop", Width = 1280, Height = 720 }
        };

        var allResults = new List<VisualTestResult>();

        foreach (var breakpoint in breakpoints)
        {
            // Set viewport
            await Page.SetViewportSizeAsync(breakpoint.Width, breakpoint.Height);

            var steps = new List<WorkflowStep>
            {
                new()
                {
                    Name = "load_home_page",
                    Actions = new List<WorkflowAction>
                    {
                        new() { Type = "navigate", Target = "/" }
                    },
                    WaitForMs = 2000
                },
                new()
                {
                    Name = "test_navigation_responsive",
                    Actions = new List<WorkflowAction>
                    {
                        new() { Type = "wait", Target = "body", Value = "1000" }
                    },
                    WaitForMs = 500
                }
            };

            var result = await McpHelper.TestUserWorkflowAsync($"responsive_{breakpoint.Name}", steps);
            
            result.Success.Should().BeTrue($"Responsive workflow failed on {breakpoint.Name}");
            
            var visualResults = result.StepResults
                .Where(s => s.VisualResult != null)
                .Select(s => s.VisualResult!)
                .ToList();
            
            allResults.AddRange(visualResults);
        }

        AssertAllVisualTestsPassed(allResults);
    }

    [Test]
    public async Task FormInteractionWorkflow_ShouldMatchBaselines()
    {
        // Test form interactions if forms exist on the page
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        // Look for any forms or input fields
        var forms = Page.Locator("form");
        var inputs = Page.Locator("input, textarea, select");

        if (await forms.CountAsync() > 0 || await inputs.CountAsync() > 0)
        {
            var steps = new List<WorkflowStep>
            {
                new()
                {
                    Name = "focus_first_input",
                    Actions = new List<WorkflowAction>
                    {
                        new() { Type = "click", Target = "input:first" }
                    },
                    WaitForMs = 500
                },
                new()
                {
                    Name = "type_in_input",
                    Actions = new List<WorkflowAction>
                    {
                        new() { Type = "type", Target = "input:first", Value = "Test Input" }
                    },
                    WaitForMs = 500
                },
                new()
                {
                    Name = "blur_input",
                    Actions = new List<WorkflowAction>
                    {
                        new() { Type = "click", Target = "body" }
                    },
                    WaitForMs = 500
                }
            };

            var result = await McpHelper.TestUserWorkflowAsync("form_interaction", steps);

            result.Success.Should().BeTrue($"Form interaction workflow failed");
            
            var visualResults = result.StepResults
                .Where(s => s.VisualResult != null)
                .Select(s => s.VisualResult!)
                .ToList();
            
            AssertAllVisualTestsPassed(visualResults);
        }
    }

    [Test]
    public async Task ErrorStateWorkflow_ShouldMatchBaselines()
    {
        // Test various error states and how they're displayed
        var steps = new List<WorkflowStep>
        {
            new()
            {
                Name = "navigate_to_invalid_route",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "navigate", Target = "/non-existent-page" }
                },
                WaitForMs = 2000,
                StopOnFailure = false // Don't stop if this step fails
            },
            new()
            {
                Name = "capture_error_state",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "wait", Target = "body", Value = "1000" }
                },
                WaitForMs = 1000,
                StopOnFailure = false
            }
        };

        var result = await McpHelper.TestUserWorkflowAsync("error_state", steps);

        // Don't assert success since we expect some failures, just check visual results
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
    public async Task AccessibilityFocusWorkflow_ShouldMatchBaselines()
    {
        // Test keyboard navigation and focus management
        await NavigateToLocalAsync("/");
        await WaitForBlazorAsync();

        var steps = new List<WorkflowStep>
        {
            new()
            {
                Name = "initial_page_focus",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "wait", Target = "body", Value = "1000" }
                },
                WaitForMs = 500
            },
            new()
            {
                Name = "tab_to_first_focusable",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "waitfor", Target = "a, button, input, [tabindex]" }
                },
                WaitForMs = 500
            }
        };

        // Simulate tab navigation
        await Page.Keyboard.PressAsync("Tab");
        await Page.WaitForTimeoutAsync(500);

        var focusedElement = await Page.EvaluateAsync<string>("document.activeElement?.tagName || 'NONE'");
        
        if (focusedElement != "NONE")
        {
            var visualResult = await CapturePageScreenshotAsync("accessibility_focus_workflow");
            AssertVisualTestPassed(visualResult);
        }

        var result = await McpHelper.TestUserWorkflowAsync("accessibility_focus", steps);

        result.Success.Should().BeTrue($"Accessibility focus workflow failed");
        
        var visualResults = result.StepResults
            .Where(s => s.VisualResult != null)
            .Select(s => s.VisualResult!)
            .ToList();
        
        AssertAllVisualTestsPassed(visualResults);
    }
}