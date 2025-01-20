using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUnit.Core;
using TUnit.Playwright;
using FluentAssertions;

namespace Visage.Tests.Frontend.Web
{
    internal class FrontEndHomeTests: PageTest
    {

        private static IDistributedApplicationTestingBuilder? _aspireHost;



        [Before(Class)]
        public static async Task CreateTestAppHostAsync()
        {
            _aspireHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Visage_AppHost>();
        }

        [Test]
        public async Task ShouldDisplayCorrectPageTitle()
        {

            if (_aspireHost == null)
            {
                throw new InvalidOperationException("Aspire host is not initialized.");
            }

            _aspireHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            await using var app = await _aspireHost.BuildAsync();
            var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
            await app.StartAsync();

            // Act
            //var httpClient = app.CreateHttpClient("frontendweb");
            //await resourceNotificationService.WaitForResourceAsync("frontendweb", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            //var response = await httpClient.GetAsync("/");

            var frontendweb = _aspireHost.Resources.Single(project => project.Name == "frontendweb");
            var endpoint = frontendweb.Annotations.OfType<EndpointAnnotation>().Single(p => p.Name == "http");

            // Assert
            await Page.GotoAsync(endpoint!.AllocatedEndpoint!.UriString);
            var title = await Page.TitleAsync();
            //Assert the Title of the page
            title.Should().Be("Visage");
        }
    }


}
