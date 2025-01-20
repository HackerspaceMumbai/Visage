using Aspire.Hosting;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using TUnit.Assertions;

namespace Visage.Test.Aspire.Tests
{
    public class AspireHostTest
    {
        private static IDistributedApplicationTestingBuilder? sutAspire;

        [Before(Class)]
        public static async Task CreateTestAppHostAsync()
        {
            sutAspire = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Visage_AppHost>();
        }

        [Test]
        public async Task GetWebResourceRoot_ShouldReturnOkStatusCode()
        {
            // Arrange
            if (sutAspire == null)
            {
                throw new InvalidOperationException("Aspire host is not initialized.");
            }

            sutAspire.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });
            // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging

            await using var app = await sutAspire.BuildAsync();
            var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
            await app.StartAsync();

            // Act
            var httpClient = app.CreateHttpClient("frontendweb");
            await resourceNotificationService.WaitForResourceAsync("frontendweb", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            var response = await httpClient.GetAsync("/");

            // Assert
            //Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task FrontendWeb_ShouldHaveExpectedEnvironmentVariables()
        {
            // Arrange
            if (sutAspire == null)
            {
                throw new InvalidOperationException("Aspire host is not initialized.");
            }

            var frontend = (IResourceWithEnvironment)sutAspire.Resources
                .Single(static r => r.Name == "frontendweb");

            // Act
            var envVars = await frontend.GetEnvironmentVariableValuesAsync(
                DistributedApplicationOperation.Publish);

            // Assert

            envVars.Should().NotBeNull();

            //FrontEndWeb has the settings for external connections
            envVars.Should().ContainKey("Auth0__Domain");
            envVars.Should().ContainKey("Auth0__ClientId");
            envVars.Should().ContainKey("Cloudinary__CloudName");
            envVars.Should().ContainKey("Cloudinary__ApiKey");

            //Frontendweb contain the connections for its Aspire co-dependent projects
            envVars.Should().Contain("services__event-api__https__0", "{event-api.bindings.https.url}");
            envVars.Should().Contain("services__event-api__http__0", "{event-api.bindings.http.url}");
            envVars.Should().Contain("services__registrations-api__https__0", "{registrations-api.bindings.https.url}");
            envVars.Should().Contain("services__registrations-api__http__0", "{registrations-api.bindings.http.url}");
            envVars.Should().Contain("services__cloudinary-image-signing__http__0", "{cloudinary-image-signing.bindings.http.url}");
        }


    }
   
}
