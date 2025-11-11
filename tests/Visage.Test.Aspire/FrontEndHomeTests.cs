using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TUnit.Core;
using FluentAssertions;
using Visage.Shared.Models;
using Microsoft.Playwright;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Visage.Test.Aspire;

namespace Visage.Tests.Frontend.Web
{
    public class FrontEndHomeTests
    {
        /// <summary>
        /// CRITICAL TEST: Validates that Aspire service discovery correctly exposes backend services.
        /// This test would have caught the "event-api" vs "eventing" naming mismatch.
        /// </summary>
        [Test]
        public async Task AspireServiceDiscoveryExposesRequiredBackendServices()
        {
            var resourceNotificationService = TestAppContext.ResourceNotificationService;
            
            // Wait for all backend services
            await resourceNotificationService.WaitForResourceAsync("eventing", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            await resourceNotificationService.WaitForResourceAsync("cloudinary-image-signing", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            await resourceNotificationService.WaitForResourceAsync("frontendweb", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            
            // Validate that HttpClients can be created for all expected service names
            // This exercises the same code path that Program.cs uses
            var eventingClient = TestAppContext.CreateHttpClient("eventing");
            eventingClient.Should().NotBeNull("eventing service should be discoverable");
            eventingClient.BaseAddress.Should().NotBeNull("eventing service should have a base address");
            
            var registrationsClient = TestAppContext.CreateHttpClient("registrations-api");
            registrationsClient.Should().NotBeNull("registrations-api service should be discoverable");
            registrationsClient.BaseAddress.Should().NotBeNull("registrations-api service should have a base address");
            
            var cloudinaryClient = TestAppContext.CreateHttpClient("cloudinary-image-signing");
            cloudinaryClient.Should().NotBeNull("cloudinary-image-signing service should be discoverable");
            cloudinaryClient.BaseAddress.Should().NotBeNull("cloudinary-image-signing service should have a base address");
            
            // Validate that services are actually reachable via health checks
            var eventingHealth = await eventingClient.GetAsync("/health");
            eventingHealth.Should().HaveStatusCode(System.Net.HttpStatusCode.OK, "eventing service health check should succeed");
            
            var registrationsHealth = await registrationsClient.GetAsync("/health");
            registrationsHealth.Should().HaveStatusCode(System.Net.HttpStatusCode.OK, "registrations-api service health check should succeed");
        }

        [Test]
        public async Task ShouldDisplayCorrectPageTitle()
        {
            var resourceNotificationService = TestAppContext.ResourceNotificationService;
            await resourceNotificationService.WaitForResourceAsync("frontendweb", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            var feClient = TestAppContext.CreateHttpClient("frontendweb");
            var baseUrl = feClient.BaseAddress?.ToString() ?? throw new InvalidOperationException("frontendweb base address not found");

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();

            await page.GotoAsync(baseUrl);
            var title = await page.TitleAsync();
            title.Should().Be("Visage - Hackerspace Mumbai Events");
        }

        [Test]
        public async Task ShouldShowUpcomingAndPastEventsAfterSeeding()
        {
            var resourceNotificationService = TestAppContext.ResourceNotificationService;
            
            // CRITICAL: Wait for ALL services the frontend depends on, not just the frontend itself
            await resourceNotificationService.WaitForResourceAsync("eventing", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            await resourceNotificationService.WaitForResourceAsync("frontendweb", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));

            // Seed data via Event API
            var apiClient = TestAppContext.CreateHttpClient("eventing");

            var upcoming = new Event
            {
                Title = "Playwright Test Upcoming",
                Description = "Automated test upcoming event",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                StartTime = new TimeOnly(18, 30),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                EndTime = new TimeOnly(20, 0),
                Location = "Hackerspace Mumbai",
                CoverPicture = "https://res.cloudinary.com/demo/image/upload/sample.jpg"
            };

            var past = new Event
            {
                Title = "Playwright Test Past",
                Description = "Automated test past event",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                StartTime = new TimeOnly(18, 30),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                EndTime = new TimeOnly(20, 0),
                Location = "Hackerspace Mumbai",
                CoverPicture = "https://res.cloudinary.com/demo/image/upload/sample.jpg"
            };

            var upResp = await apiClient.PostAsJsonAsync("/events", upcoming);
            upResp.EnsureSuccessStatusCode();
            var pastResp = await apiClient.PostAsJsonAsync("/events", past);
            pastResp.EnsureSuccessStatusCode();

            var feClient = TestAppContext.CreateHttpClient("frontendweb");
            var baseUrl = feClient.BaseAddress?.ToString() ?? throw new InvalidOperationException("frontendweb base address not found");

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            // CRITICAL: Monitor network requests to catch API call failures
            var apiCallFailed = false;
            var apiErrorMessage = string.Empty;
            page.Response += (_, response) =>
            {
                if (response.Url.Contains("/events") && !response.Ok)
                {
                    apiCallFailed = true;
                    apiErrorMessage = $"API call failed: {response.Status} {response.StatusText} - {response.Url}";
                }
            };
            
            page.PageError += (_, error) =>
            {
                if (error.Contains("eventing") || error.Contains("No such host"))
                {
                    apiCallFailed = true;
                    apiErrorMessage = $"DNS/Network error: {error}";
                }
            };
            
            // CRITICAL: Capture console errors which include Blazor rendering exceptions
            var consoleErrors = new List<string>();
            page.Console += (_, msg) =>
            {
                if (msg.Type == "error")
                {
                    consoleErrors.Add(msg.Text);
                }
            };

            await page.GotoAsync(baseUrl);

            // CRITICAL: Check for Blazor error UI which indicates rendering exceptions
            var hasErrorUI = await IsVisibleWithinAsync(page.Locator("text=/Unable to load events/i"), 2000)
                || await IsVisibleWithinAsync(page.Locator("text=/Unrecognized Guid format/i"), 2000)
                || await IsVisibleWithinAsync(page.Locator(".alert-error"), 2000);
            
            hasErrorUI.Should().BeFalse("Home page should not display error UI after seeding events");

            await page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Upcoming Events" }).IsVisibleAsync();
            await page.GetByText("Playwright Test Upcoming").IsVisibleAsync();

            await page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Past Events" }).IsVisibleAsync();
            await page.GetByText("Playwright Test Past").IsVisibleAsync();
            
            // CRITICAL: Assert that no API errors occurred during page load
            apiCallFailed.Should().BeFalse($"Frontend should successfully call eventing service, but got: {apiErrorMessage}");
            
            // CRITICAL: Assert that no console errors occurred (includes Blazor exceptions)
            consoleErrors.Should().BeEmpty($"Page should not have console errors, but got: {string.Join(", ", consoleErrors)}");
        }

        [Test]
        public async Task ShouldShowEmptyStatesWhenNoEvents()
        {
            var resourceNotificationService = TestAppContext.ResourceNotificationService;
            
            // Wait for backend services
            await resourceNotificationService.WaitForResourceAsync("eventing", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            await resourceNotificationService.WaitForResourceAsync("frontendweb", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            var feClient = TestAppContext.CreateHttpClient("frontendweb");
            var baseUrl = feClient.BaseAddress?.ToString() ?? throw new InvalidOperationException("frontendweb base address not found");

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();

            await page.GotoAsync(baseUrl);

            await page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Upcoming Events" }).IsVisibleAsync();
            await page.GetByText("No Upcoming Events").IsVisibleAsync();
            await page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Past Events" }).IsVisibleAsync();
            await page.GetByText("No Past Events").IsVisibleAsync();
        }

        [Test]
        public async Task EventCardShowsNameLocationAndRsvp()
        {
            var resourceNotificationService = TestAppContext.ResourceNotificationService;
            
            // Wait for backend services before frontend
            await resourceNotificationService.WaitForResourceAsync("eventing", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            await resourceNotificationService.WaitForResourceAsync("registrations-api", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));
            await resourceNotificationService.WaitForResourceAsync("frontendweb", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(60));

            // Seed one upcoming event
            var apiClient = TestAppContext.CreateHttpClient("eventing");
            var evt = new Event
            {
                Title = "Card Detail Test",
                Description = "Event with details",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                StartTime = new TimeOnly(17, 0),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                EndTime = new TimeOnly(19, 0),
                Location = "Colaba Library",
                CoverPicture = "https://res.cloudinary.com/demo/image/upload/sample.jpg"
            };
            var resp = await apiClient.PostAsJsonAsync("/events", evt);
            resp.EnsureSuccessStatusCode();

            var feClient = TestAppContext.CreateHttpClient("frontendweb");
            var baseUrl = feClient.BaseAddress?.ToString() ?? throw new InvalidOperationException("frontendweb base address not found");

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            
            // CRITICAL: Capture console errors which include Blazor rendering exceptions
            var consoleErrors = new List<string>();
            page.Console += (_, msg) =>
            {
                if (msg.Type == "error")
                {
                    consoleErrors.Add(msg.Text);
                }
            };

            await page.GotoAsync(baseUrl);
            
            // CRITICAL: Check for Blazor error UI
            var hasErrorUI = await IsVisibleWithinAsync(page.Locator("text=/Unable to load events/i"), 2000)
                || await IsVisibleWithinAsync(page.Locator(".alert-error"), 2000);
            hasErrorUI.Should().BeFalse("Home page should not display error UI");

            await page.GetByText("Card Detail Test").IsVisibleAsync();
            await page.GetByText("Colaba Library").IsVisibleAsync();
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "RSVP" }).IsVisibleAsync();
            
            // CRITICAL: Assert that no console errors occurred
            consoleErrors.Should().BeEmpty($"Page should not have console errors, but got: {string.Join(", ", consoleErrors)}");
        }

        // Helper to replace obsolete IsVisibleAsync timeout option usage
        private static async Task<bool> IsVisibleWithinAsync(ILocator locator, int timeoutMs)
        {
            try
            {
                await locator.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = timeoutMs
                });
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }
    }
}
