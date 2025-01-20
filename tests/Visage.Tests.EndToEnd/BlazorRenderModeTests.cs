using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

public class BlazorRenderModeTests
{
    [Fact]
    public async Task TestStaticRenderMode()
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();

        await page.GotoAsync("http://localhost:5000");

        // Check if the static content is rendered correctly
        var staticContent = await page.TextContentAsync("#static-content");
        Assert.Equal("Expected Static Content", staticContent);
    }

    [Fact]
    public async Task TestInteractiveRenderMode()
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();

        await page.GotoAsync("http://localhost:5000");

        // Check if the interactive content is rendered correctly
        var interactiveContent = await page.TextContentAsync("#interactive-content");
        Assert.Equal("Expected Interactive Content", interactiveContent);

        // Perform some interaction
        await page.ClickAsync("#interactive-button");
        var updatedContent = await page.TextContentAsync("#interactive-content");
        Assert.Equal("Updated Interactive Content", updatedContent);
    }
}
