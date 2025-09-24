# Visage Visual Testing Suite

This project provides comprehensive visual regression testing for the Visage application using Playwright MCP (Model Context Protocol) integration.

## Overview

The visual testing suite includes:

- **Visual Regression Testing**: Automated screenshot comparison to detect UI changes
- **Component Testing**: Individual UI component visual validation
- **Responsive Testing**: Cross-device visual consistency verification
- **Workflow Testing**: End-to-end user journey visual validation
- **Playwright MCP Integration**: Enhanced automation and workflow capabilities

## Project Structure

```
Visage.Tests.Visual/
├── Base/
│   └── VisualTestBase.cs           # Base class for all visual tests
├── Components/
│   ├── HomePageVisualTests.cs     # Home page component tests
│   └── NavigationVisualTests.cs   # Navigation component tests
├── Infrastructure/
│   ├── VisualTestingEngine.cs     # Core visual comparison engine
│   └── PlaywrightMcpHelper.cs     # MCP integration utilities
├── Workflows/
│   └── UserWorkflowVisualTests.cs # End-to-end workflow tests
└── README.md
```

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Playwright browsers installed
- Visage application running locally

### Installation

1. Install Playwright browsers:
   ```bash
   dotnet build
   pwsh bin/Debug/net9.0/playwright.ps1 install
   ```

2. Set up the application:
   ```bash
   # Start the Visage application (typically on https://localhost:7150)
   dotnet run --project ../../Visage.FrontEnd/Visage.FrontEnd.Web/
   ```

### Running Tests

Run all visual tests:
```bash
dotnet test
```

Run specific test categories:
```bash
# Component tests only
dotnet test --filter "Category=Component"

# Workflow tests only  
dotnet test --filter "Category=Workflow"
```

## How Visual Testing Works

### 1. Baseline Creation
When you run a visual test for the first time, it creates a baseline screenshot in the `test-results/visual-baselines/` directory.

### 2. Visual Comparison
Subsequent test runs compare the current screenshot against the baseline using pixel-by-pixel analysis with configurable thresholds.

### 3. Difference Detection
If differences exceed the threshold, the test fails and generates:
- **Actual Screenshot**: Current state
- **Diff Image**: Highlighted differences in red
- **Detailed Report**: Percentage difference and affected areas

### 4. Test Results
Results are stored in:
- `test-results/visual-baselines/` - Reference images
- `test-results/visual-actual/` - Current screenshots  
- `test-results/visual-diffs/` - Difference images (on failure)

## Configuration

### Environment Variables

- `VISAGE_TEST_URL`: Base URL for testing (default: https://localhost:7150)
- `VISUAL_THRESHOLD`: Difference threshold percentage (default: 0.2%)

### Test Configuration

The `VisualTestingEngine` can be configured with:
- **Threshold**: Acceptable difference percentage (default: 0.2%)
- **Directories**: Custom paths for baselines, actual, and diff images
- **Comparison Options**: Pixel sensitivity and comparison algorithms

## Test Categories

### Component Tests
Test individual UI components in isolation:
- Event cards
- Navigation menus
- Buttons and form elements
- Responsive layouts

### Workflow Tests  
Test complete user journeys:
- Event browsing workflow
- Navigation between pages
- Form interactions
- Error state handling

### Responsive Tests
Test across multiple viewport sizes:
- Mobile (375x667)
- Tablet (768x1024)
- Desktop (1280x720)
- Large desktop (1920x1080)

## Playwright MCP Integration

The `PlaywrightMcpHelper` provides enhanced capabilities:

### Component Testing
```csharp
var config = new ComponentTestConfig
{
    ComponentSelector = ".btn-primary",
    Interactions = new List<InteractionConfig>
    {
        new() { Name = "hover", Type = "hover", Selector = ".btn-primary" },
        new() { Name = "focus", Type = "focus", Selector = ".btn-primary" }
    }
};

var result = await McpHelper.TestComponentWorkflowAsync("button_component", config);
```

### Workflow Testing
```csharp
var steps = new List<WorkflowStep>
{
    new()
    {
        Name = "navigate_home",
        Actions = new List<WorkflowAction>
        {
            new() { Type = "navigate", Target = "/" }
        },
        WaitForMs = 2000,
        ExpectedElements = new List<string> { "h1" }
    }
};

var result = await McpHelper.TestUserWorkflowAsync("navigation", steps);
```

## Best Practices

### Writing Visual Tests
1. **Consistent Environment**: Use the same browser, viewport, and settings
2. **Stable Elements**: Wait for loading states and animations to complete
3. **Descriptive Names**: Use clear, descriptive test and screenshot names
4. **Focused Tests**: Test specific components or workflows, not entire pages when possible

### Maintaining Baselines
1. **Regular Updates**: Update baselines when intentional UI changes are made
2. **Review Changes**: Always review visual diffs before accepting new baselines
3. **Version Control**: Consider storing baselines in version control for team consistency

### Debugging Failed Tests
1. **Check Diff Images**: Review the generated diff images to understand changes
2. **Compare Screenshots**: Manually compare baseline vs actual screenshots
3. **Verify Environment**: Ensure consistent test environment setup
4. **Check Timing**: Verify proper wait conditions for dynamic content

## CI/CD Integration

For automated testing in CI/CD pipelines:

1. Set the `VISAGE_TEST_URL` environment variable
2. Ensure Playwright browsers are installed
3. Store baseline images in version control or artifact storage
4. Configure test reporting to capture visual test results

Example GitHub Actions step:
```yaml
- name: Run Visual Tests
  run: |
    dotnet test tests/Visage.Tests.Visual/ --logger trx --results-directory TestResults
  env:
    VISAGE_TEST_URL: ${{ secrets.TEST_APP_URL }}
```

## Troubleshooting

### Common Issues

**Playwright browsers not installed**
```bash
pwsh bin/Debug/net9.0/playwright.ps1 install
```

**Application not running**
```bash
# Ensure the Visage app is running on the expected URL
curl https://localhost:7150
```

**Flaky visual tests**
- Increase wait times for dynamic content
- Disable animations and transitions
- Use consistent viewport sizes
- Ensure stable test data

**High difference percentages**
- Review actual changes in the UI
- Verify test environment consistency
- Check for font rendering differences
- Consider browser or OS differences

## Contributing

When adding new visual tests:

1. Follow the existing naming conventions
2. Inherit from `VisualTestBase`
3. Use descriptive test names and screenshot names
4. Add appropriate wait conditions
5. Test across multiple viewports when relevant
6. Document any special setup requirements

## Support

For questions or issues with visual testing:
1. Check the test output and generated diff images
2. Review this documentation
3. Check existing test examples
4. Open an issue with detailed information about the failure