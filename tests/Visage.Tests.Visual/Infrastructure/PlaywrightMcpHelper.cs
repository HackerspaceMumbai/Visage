using Microsoft.Playwright;
using System.Text.Json;

namespace Visage.Tests.Visual.Infrastructure;

/// <summary>
/// Provides MCP (Model Context Protocol) integration capabilities for Playwright testing.
/// This enables advanced interaction patterns and workflow automation.
/// </summary>
public class PlaywrightMcpHelper
{
    private readonly IPage _page;
    private readonly VisualTestingEngine _visualEngine;

    public PlaywrightMcpHelper(IPage page, VisualTestingEngine visualEngine)
    {
        _page = page;
        _visualEngine = visualEngine;
    }

    /// <summary>
    /// Performs a comprehensive UI component test with visual verification.
    /// This includes interaction testing, state validation, and visual regression detection.
    /// </summary>
    public async Task<ComponentTestResult> TestComponentWorkflowAsync(string componentName, ComponentTestConfig config)
    {
        var results = new List<VisualTestResult>();
        var interactions = new List<InteractionResult>();

        try
        {
            // Navigate to the component
            if (!string.IsNullOrEmpty(config.NavigationUrl))
            {
                await _page.GotoAsync(config.NavigationUrl);
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }

            // Wait for component to be ready
            if (!string.IsNullOrEmpty(config.ComponentSelector))
            {
                await _page.WaitForSelectorAsync(config.ComponentSelector);
            }

            // Initial state screenshot
            var initialResult = await _visualEngine.CompareScreenshotAsync(_page, $"{componentName}_initial_state");
            results.Add(initialResult);

            // Perform configured interactions
            foreach (var interaction in config.Interactions)
            {
                var interactionResult = await PerformInteractionAsync(interaction);
                interactions.Add(interactionResult);

                // Wait for any animations or state changes
                if (interaction.WaitAfterMs > 0)
                {
                    await _page.WaitForTimeoutAsync(interaction.WaitAfterMs);
                }

                // Capture state after interaction
                var stateResult = await _visualEngine.CompareScreenshotAsync(_page, 
                    $"{componentName}_{interaction.Name}_state");
                results.Add(stateResult);
            }

            // Test responsive behavior if configured
            if (config.ResponsiveBreakpoints?.Any() == true)
            {
                foreach (var breakpoint in config.ResponsiveBreakpoints)
                {
                    await _page.SetViewportSizeAsync(breakpoint.Width, breakpoint.Height);
                    await _page.WaitForTimeoutAsync(500); // Allow for responsive transitions

                    var responsiveResult = await _visualEngine.CompareScreenshotAsync(_page, 
                        $"{componentName}_responsive_{breakpoint.Name}");
                    results.Add(responsiveResult);
                }
            }

            return new ComponentTestResult
            {
                ComponentName = componentName,
                Success = results.All(r => r.Status != VisualTestStatus.Failed) && 
                         interactions.All(i => i.Success),
                VisualResults = results,
                InteractionResults = interactions,
                Message = "Component test completed successfully"
            };
        }
        catch (Exception ex)
        {
            return new ComponentTestResult
            {
                ComponentName = componentName,
                Success = false,
                VisualResults = results,
                InteractionResults = interactions,
                Message = $"Component test failed: {ex.Message}",
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Tests a complete user workflow with visual verification at each step.
    /// </summary>
    public async Task<WorkflowTestResult> TestUserWorkflowAsync(string workflowName, List<WorkflowStep> steps)
    {
        var stepResults = new List<WorkflowStepResult>();

        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var stepResult = new WorkflowStepResult
            {
                StepNumber = i + 1,
                StepName = step.Name,
                Success = true
            };

            try
            {
                // Execute step actions
                foreach (var action in step.Actions)
                {
                    await ExecuteWorkflowActionAsync(action);
                }

                // Wait for step completion
                if (step.WaitForSelector != null)
                {
                    await _page.WaitForSelectorAsync(step.WaitForSelector);
                }

                if (step.WaitForMs > 0)
                {
                    await _page.WaitForTimeoutAsync(step.WaitForMs);
                }

                // Capture visual state
                if (step.CaptureVisual)
                {
                    var visualResult = await _visualEngine.CompareScreenshotAsync(_page, 
                        $"{workflowName}_step_{i + 1}_{step.Name}");
                    stepResult.VisualResult = visualResult;
                    
                    if (visualResult.Status == VisualTestStatus.Failed)
                    {
                        stepResult.Success = false;
                        stepResult.ErrorMessage = visualResult.Message;
                    }
                }

                // Validate step expectations
                if (step.ExpectedElements?.Any() == true)
                {
                    foreach (var expectedElement in step.ExpectedElements)
                    {
                        var element = _page.Locator(expectedElement);
                        var isVisible = await element.IsVisibleAsync();
                        if (!isVisible)
                        {
                            stepResult.Success = false;
                            stepResult.ErrorMessage = $"Expected element not visible: {expectedElement}";
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                stepResult.Success = false;
                stepResult.ErrorMessage = ex.Message;
                stepResult.Exception = ex;
            }

            stepResults.Add(stepResult);

            // If step failed and workflow is configured to stop on failure, break
            if (!stepResult.Success && step.StopOnFailure)
            {
                break;
            }
        }

        return new WorkflowTestResult
        {
            WorkflowName = workflowName,
            Success = stepResults.All(s => s.Success),
            StepResults = stepResults,
            TotalSteps = steps.Count,
            CompletedSteps = stepResults.Count
        };
    }

    private async Task<InteractionResult> PerformInteractionAsync(InteractionConfig interaction)
    {
        try
        {
            var element = _page.Locator(interaction.Selector);

            switch (interaction.Type.ToLowerInvariant())
            {
                case "click":
                    await element.ClickAsync();
                    break;
                case "type":
                    await element.FillAsync(interaction.Value ?? "");
                    break;
                case "hover":
                    await element.HoverAsync();
                    break;
                case "focus":
                    await element.FocusAsync();
                    break;
                case "select":
                    await element.SelectOptionAsync(interaction.Value ?? "");
                    break;
                default:
                    throw new ArgumentException($"Unknown interaction type: {interaction.Type}");
            }

            return new InteractionResult
            {
                Name = interaction.Name,
                Success = true,
                Message = $"Interaction '{interaction.Name}' completed successfully"
            };
        }
        catch (Exception ex)
        {
            return new InteractionResult
            {
                Name = interaction.Name,
                Success = false,
                Message = $"Interaction '{interaction.Name}' failed: {ex.Message}",
                Exception = ex
            };
        }
    }

    private async Task ExecuteWorkflowActionAsync(WorkflowAction action)
    {
        switch (action.Type.ToLowerInvariant())
        {
            case "navigate":
                await _page.GotoAsync(action.Target);
                break;
            case "click":
                await _page.ClickAsync(action.Target);
                break;
            case "type":
                await _page.FillAsync(action.Target, action.Value ?? "");
                break;
            case "waitfor":
                await _page.WaitForSelectorAsync(action.Target);
                break;
            case "wait":
                await _page.WaitForTimeoutAsync(int.Parse(action.Value ?? "1000"));
                break;
            default:
                throw new ArgumentException($"Unknown workflow action type: {action.Type}");
        }
    }
}

// Configuration and result classes
public class ComponentTestConfig
{
    public string? NavigationUrl { get; set; }
    public string? ComponentSelector { get; set; }
    public List<InteractionConfig> Interactions { get; set; } = new();
    public List<ResponsiveBreakpoint>? ResponsiveBreakpoints { get; set; }
}

public class InteractionConfig
{
    public required string Name { get; set; }
    public required string Type { get; set; } // click, type, hover, focus, select
    public required string Selector { get; set; }
    public string? Value { get; set; }
    public int WaitAfterMs { get; set; } = 500;
}

public class ResponsiveBreakpoint
{
    public required string Name { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class WorkflowStep
{
    public required string Name { get; set; }
    public List<WorkflowAction> Actions { get; set; } = new();
    public bool CaptureVisual { get; set; } = true;
    public string? WaitForSelector { get; set; }
    public int WaitForMs { get; set; } = 1000;
    public List<string>? ExpectedElements { get; set; }
    public bool StopOnFailure { get; set; } = true;
}

public class WorkflowAction
{
    public required string Type { get; set; } // navigate, click, type, waitfor, wait
    public required string Target { get; set; }
    public string? Value { get; set; }
}

// Result classes
public class ComponentTestResult
{
    public required string ComponentName { get; set; }
    public bool Success { get; set; }
    public List<VisualTestResult> VisualResults { get; set; } = new();
    public List<InteractionResult> InteractionResults { get; set; } = new();
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
}

public class InteractionResult
{
    public required string Name { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
}

public class WorkflowTestResult
{
    public required string WorkflowName { get; set; }
    public bool Success { get; set; }
    public List<WorkflowStepResult> StepResults { get; set; } = new();
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
}

public class WorkflowStepResult
{
    public int StepNumber { get; set; }
    public required string StepName { get; set; }
    public bool Success { get; set; }
    public VisualTestResult? VisualResult { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
}