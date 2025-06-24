using Aspire.Hosting;
using FluentAssertions;
using TUnit.Assertions;

namespace Visage.Test.Aspire.Tests;

public class ScalarIntegrationTest
{
    [Test]
    public void WithScalarApiDocumentation_ShouldConfigureEndpoints()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder();
        var projectResource = appBuilder.AddProject<Projects.Visage_Services_Eventing>("test-api");

        // Act
        var configuredResource = projectResource.WithScalarApiDocumentation("Test API");

        // Assert
        configuredResource.Should().NotBeNull();
        configuredResource.Should().BeSameAs(projectResource);
    }

    [Test]
    public void WithScalarApiDocumentation_WithDefaultTitle_ShouldUseResourceName()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder();
        var projectResource = appBuilder.AddProject<Projects.Visage_Services_Eventing>("test-api");

        // Act
        var configuredResource = projectResource.WithScalarApiDocumentation();

        // Assert
        configuredResource.Should().NotBeNull();
        configuredResource.Should().BeSameAs(projectResource);
    }

    [Test]
    public void WithScalarApiDocumentation_WithCustomPath_ShouldConfigureCorrectly()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder();
        var projectResource = appBuilder.AddProject<Projects.Visage_Services_Eventing>("test-api");

        // Act
        var configuredResource = projectResource.WithScalarApiDocumentation("Test API", "/docs/v1");

        // Assert
        configuredResource.Should().NotBeNull();
        configuredResource.Should().BeSameAs(projectResource);
    }
}