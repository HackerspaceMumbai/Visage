using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

public class PerformanceTests
{
    [Fact]
    public async Task TestHeavyDOMManipulationPerformance()
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();

        await page.GotoAsync("http://localhost:5000");

        // Start measuring performance
        var startTime = await page.EvaluateAsync<long>("() => performance.now()");

        // Perform heavy DOM manipulations
        await page.EvaluateAsync("() => { for (let i = 0; i < 10000; i++) { let div = document.createElement('div'); div.textContent = 'Test'; document.body.appendChild(div); } }");

        // End measuring performance
        var endTime = await page.EvaluateAsync<long>("() => performance.now()");

        var duration = endTime - startTime;

        // Assert that the duration is within acceptable limits
        Assert.True(duration < 5000, $"Heavy DOM manipulation took too long: {duration}ms");
    }
}
