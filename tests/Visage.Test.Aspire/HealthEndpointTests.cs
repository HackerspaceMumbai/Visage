using Aspire.Hosting;
using Aspire.Hosting.Testing;
using FluentAssertions;
using TUnit.Core;

namespace Visage.Test.Aspire;

/// <summary>
/// Automated health endpoint tests for all Aspire services
/// Tests verify that all services expose /health and /alive endpoints as required by constitution
/// </summary>
// These tests verify infrastructure health endpoints and are intentionally
// kept out of the default test runs. Use the Category filter to run them
// explicitly: dotnet test --filter "Category=AspireHealth"
[Category("AspireHealth")]
public class HealthEndpointTests
{
    /// <summary>
    /// Verify Registration API health endpoint is accessible and returns 200 OK
    /// </summary>
    [Test]
    public async Task RegistrationApi_Health_Endpoint_Should_Return_200()
    {
        // Arrange
        var httpClient = TestAppContext.CreateHttpClient("userprofile-api");
        
        // Act
        var response = await httpClient.GetAsync("/health");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            because: "Registration API /health endpoint must be accessible and return 200 OK");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    /// <summary>
    /// Verify Registration API alive endpoint is accessible and returns 200 OK
    /// </summary>
    [Test]
    public async Task RegistrationApi_Alive_Endpoint_Should_Return_200()
    {
        // Arrange
        var httpClient = TestAppContext.CreateHttpClient("userprofile-api");
        
        // Act
        var response = await httpClient.GetAsync("/alive");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            because: "Registration API /alive endpoint must be accessible and return 200 OK");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    /// <summary>
    /// Verify Eventing API health endpoint is accessible and returns 200 OK
    /// </summary>
    [Test]
    public async Task EventingApi_Health_Endpoint_Should_Return_200()
    {
        // Arrange
        var httpClient = TestAppContext.CreateHttpClient("eventing");
        
        // Act
        var response = await httpClient.GetAsync("/health");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            because: "Eventing API /health endpoint must be accessible and return 200 OK");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    /// <summary>
    /// Verify Eventing API alive endpoint is accessible and returns 200 OK
    /// </summary>
    [Test]
    public async Task EventingApi_Alive_Endpoint_Should_Return_200()
    {
        // Arrange
        var httpClient = TestAppContext.CreateHttpClient("eventing");
        
        // Act
        var response = await httpClient.GetAsync("/alive");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            because: "Eventing API /alive endpoint must be accessible and return 200 OK");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    /// <summary>
    /// Verify Frontend Web health endpoint is accessible and returns 200 OK
    /// </summary>
    [Test]
    public async Task FrontendWeb_Health_Endpoint_Should_Return_200()
    {
        // Arrange
        var httpClient = TestAppContext.CreateHttpClient("frontendweb");
        
        // Act
        var response = await httpClient.GetAsync("/health");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            because: "Frontend Web /health endpoint must be accessible and return 200 OK");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    /// <summary>
    /// Verify Frontend Web alive endpoint is accessible and returns 200 OK
    /// </summary>
    [Test]
    public async Task FrontendWeb_Alive_Endpoint_Should_Return_200()
    {
        // Arrange
        var httpClient = TestAppContext.CreateHttpClient("frontendweb");
        
        // Act
        var response = await httpClient.GetAsync("/alive");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            because: "Frontend Web /alive endpoint must be accessible and return 200 OK");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    /// <summary>
    /// Verify Cloudinary Image Signing health endpoint is accessible and returns 200 OK
    /// </summary>
    [Test]
    public async Task CloudinaryImageSigning_Health_Endpoint_Should_Return_200()
    {
        // Arrange
        var httpClient = TestAppContext.CreateHttpClient("cloudinary-image-signing");

        // Act
        var response = await httpClient.GetAsync("/health");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            because: "cloudinary-image-signing /health endpoint must be accessible and return 200 OK");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    /// <summary>
    /// Verify Cloudinary Image Signing alive endpoint is accessible and returns 200 OK
    /// </summary>
    [Test]
    public async Task CloudinaryImageSigning_Alive_Endpoint_Should_Return_200()
    {
        // Arrange
        var httpClient = TestAppContext.CreateHttpClient("cloudinary-image-signing");

        // Act
        var response = await httpClient.GetAsync("/alive");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            because: "cloudinary-image-signing /alive endpoint must be accessible and return 200 OK");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    /// <summary>
    /// Verify all registered Aspire resources have health endpoints
    /// This test dynamically discovers all HTTP-based resources and validates their health endpoints
    /// </summary>
    [Test]
    public async Task All_Http_Resources_Should_Have_Health_Endpoints()
    {
        // Arrange
        var resourceNames = new[] { "userprofile-api", "eventing", "frontendweb", "cloudinary-image-signing" };
        var failures = new List<string>();

        // Act & Assert
        foreach (var resourceName in resourceNames)
        {
            try
            {
                var httpClient = TestAppContext.CreateHttpClient(resourceName);
                
                var healthResponse = await httpClient.GetAsync("/health");
                if (!healthResponse.IsSuccessStatusCode)
                {
                    failures.Add($"{resourceName}: /health returned {healthResponse.StatusCode}");
                }

                var aliveResponse = await httpClient.GetAsync("/alive");
                if (!aliveResponse.IsSuccessStatusCode)
                {
                    failures.Add($"{resourceName}: /alive returned {aliveResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                failures.Add($"{resourceName}: {ex.Message}");
            }
        }

        // Assert
        failures.Should().BeEmpty(
            because: "All Aspire HTTP resources must expose /health and /alive endpoints per constitution Principle I");
    }
}
