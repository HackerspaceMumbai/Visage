using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
using TUnit;

namespace Visage.Tests.EndToEnd
{
    public class ConfigurationTests
    {
        private readonly HttpClient _httpClient;

        public ConfigurationTests()
        {
            _httpClient = new HttpClient();
        }

        [Test]
        public async Task TestLocalEndpointConfiguration()
        {
            var response = await _httpClient.GetAsync("http://localhost:5000/health");
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [Test]
        public async Task TestCI_CD_EndpointConfiguration()
        {
            var response = await _httpClient.GetAsync("http://ci-cd-endpoint/health");
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [Test]
        public async Task TestPlaywrightLocalEndpoint()
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await page.GotoAsync("http://localhost:5000");
            var title = await page.TitleAsync();
            title.Should().Be("Visage");
        }

        [Test]
        public async Task TestPlaywrightCI_CDEndpoint()
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await page.GotoAsync("http://ci-cd-endpoint");
            var title = await page.TitleAsync();
            title.Should().Be("Visage");
        }
    }
}
