using System.Text.Json;

namespace Visage.Tests.Visual.Infrastructure;

/// <summary>
/// Configuration manager for visual testing environment settings.
/// Handles different test environments, thresholds, and browser configurations.
/// </summary>
public class VisualTestConfig
{
    public static VisualTestConfig Instance { get; } = new();

    public string BaseUrl { get; set; } = GetEnvironmentVariable("VISAGE_TEST_URL", "https://localhost:7150");
    public double DifferenceThreshold { get; set; } = GetEnvironmentVariable("VISUAL_THRESHOLD", "0.2", double.Parse);
    public int DefaultTimeout { get; set; } = GetEnvironmentVariable("TEST_TIMEOUT", "30000", int.Parse);
    public bool HeadlessMode { get; set; } = GetEnvironmentVariable("HEADLESS", "true", bool.Parse);
    public string TestEnvironment { get; set; } = GetEnvironmentVariable("TEST_ENVIRONMENT", "Development");
    public ViewportSize DefaultViewport { get; set; } = new() { Width = 1280, Height = 720 };
    public List<ViewportSize> ResponsiveBreakpoints { get; set; } = new()
    {
        new() { Name = "mobile", Width = 375, Height = 667 },
        new() { Name = "tablet", Width = 768, Height = 1024 },
        new() { Name = "desktop", Width = 1280, Height = 720 },
        new() { Name = "large", Width = 1920, Height = 1080 }
    };

    /// <summary>
    /// Browser configuration settings
    /// </summary>
    public BrowserConfig Browser { get; set; } = new();

    /// <summary>
    /// Test result directories configuration
    /// </summary>
    public TestDirectories Directories { get; set; } = new();

    private static string GetEnvironmentVariable(string name, string defaultValue)
    {
        return Environment.GetEnvironmentVariable(name) ?? defaultValue;
    }

    private static T GetEnvironmentVariable<T>(string name, string defaultValue, Func<string, T> parser)
    {
        var value = Environment.GetEnvironmentVariable(name) ?? defaultValue;
        try
        {
            return parser(value);
        }
        catch
        {
            return parser(defaultValue);
        }
    }

    /// <summary>
    /// Loads configuration from a JSON file if it exists
    /// </summary>
    public static VisualTestConfig LoadFromFile(string configPath = "visual-test-config.json")
    {
        if (!File.Exists(configPath))
        {
            return Instance;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<VisualTestConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return config ?? Instance;
        }
        catch
        {
            return Instance;
        }
    }

    /// <summary>
    /// Saves current configuration to a JSON file
    /// </summary>
    public void SaveToFile(string configPath = "visual-test-config.json")
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        File.WriteAllText(configPath, json);
    }
}

public class ViewportSize
{
    public string Name { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
}

public class BrowserConfig
{
    public string Type { get; set; } = "chromium"; // chromium, firefox, webkit
    public bool SlowMo { get; set; } = false;
    public int SlowMoMs { get; set; } = 0;
    public bool DevTools { get; set; } = false;
    public List<string> Args { get; set; } = new();
    public bool IgnoreHttpsErrors { get; set; } = true;
}

public class TestDirectories
{
    public string Baselines { get; set; } = "test-results/visual-baselines";
    public string Actual { get; set; } = "test-results/visual-actual";
    public string Diffs { get; set; } = "test-results/visual-diffs";
    public string Reports { get; set; } = "test-results/reports";
    public string Screenshots { get; set; } = "test-results/screenshots";
}

/// <summary>
/// Test data manager for creating consistent test scenarios
/// </summary>
public static class TestDataManager
{
    /// <summary>
    /// Common test selectors used across visual tests
    /// </summary>
    public static class Selectors
    {
        public const string EventCard = ".card";
        public const string Navigation = "nav";
        public const string NavigationLink = "nav a";
        public const string PrimaryButton = ".btn-primary";
        public const string SecondaryButton = ".btn-secondary";
        public const string ThemeToggle = "[data-theme-toggle]";
        public const string MobileMenuToggle = ".navbar-toggler, [aria-label*='menu']";
        public const string EventGrid = ".grid";
        public const string Header = "header";
        public const string Footer = "footer";
        public const string MainContent = "main";
    }

    /// <summary>
    /// Common test scenarios for component testing
    /// </summary>
    public static class Scenarios
    {
        public static ComponentTestConfig ButtonInteraction(string selector) => new()
        {
            ComponentSelector = selector,
            Interactions = new List<InteractionConfig>
            {
                new() { Name = "hover", Type = "hover", Selector = selector, WaitAfterMs = 300 },
                new() { Name = "focus", Type = "focus", Selector = selector, WaitAfterMs = 300 },
                new() { Name = "click", Type = "click", Selector = selector, WaitAfterMs = 500 }
            }
        };

        public static ComponentTestConfig NavigationFlow() => new()
        {
            ComponentSelector = Selectors.Navigation,
            Interactions = new List<InteractionConfig>
            {
                new() { Name = "hover_first_link", Type = "hover", Selector = $"{Selectors.NavigationLink}:first-child", WaitAfterMs = 300 },
                new() { Name = "hover_second_link", Type = "hover", Selector = $"{Selectors.NavigationLink}:nth-child(2)", WaitAfterMs = 300 }
            }
        };

        public static List<WorkflowStep> BasicPageNavigation(string fromUrl, string toUrl, string expectedElement) => new()
        {
            new()
            {
                Name = "navigate_from",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "navigate", Target = fromUrl }
                },
                WaitForMs = 2000
            },
            new()
            {
                Name = "navigate_to",
                Actions = new List<WorkflowAction>
                {
                    new() { Type = "click", Target = $"a[href='{toUrl}']" }
                },
                WaitForMs = 1500,
                ExpectedElements = new List<string> { expectedElement }
            }
        };
    }

    /// <summary>
    /// Test page URLs for different scenarios
    /// </summary>
    public static class TestPages
    {
        public const string Home = "/";
        public const string Counter = "/counter";
        public const string Weather = "/weather";
        public const string Profile = "/profile";
        public const string Events = "/events";
        public const string ScheduleEvent = "/schedule-event";
        public const string UserRegistration = "/user-registration";
    }
}