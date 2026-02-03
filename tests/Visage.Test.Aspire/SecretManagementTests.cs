using Aspire.Hosting;
using FluentAssertions;
using TUnit.Core;

namespace Visage.Test.Aspire;

/// <summary>
/// Tests for secret management configuration
/// Verifies that the AppHost properly handles User Secrets in Development
/// and Azure Key Vault in non-Development environments
/// These tests do not require Docker/Aspire to be running
/// </summary>
[Category("SecretManagement")]
[NotInParallel] // Run these tests sequentially to avoid resource conflicts
public class SecretManagementTests
{
    /// <summary>
    /// Gets the repository root directory by traversing up from the test assembly location
    /// </summary>
    private static string GetRepositoryRoot()
    {
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var directory = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation)!);
        
        // Traverse up until we find the directory containing Visage.slnx
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Visage.slnx")))
        {
            directory = directory.Parent;
        }
        
        if (directory == null)
        {
            throw new DirectoryNotFoundException("Could not find repository root containing Visage.slnx");
        }
        
        return directory.FullName;
    }
    
    /// <summary>
    /// Verify that User Secrets are properly initialized for all service projects
    /// </summary>
    [Test]
    public void Service_Projects_Should_Have_UserSecretsId_Configured()
    {
        // Arrange
        var repoRoot = GetRepositoryRoot();
        
        var projectPaths = new Dictionary<string, string>
        {
            ["AppHost"] = Path.Combine(repoRoot, "Visage.AppHost", "Visage.AppHost.csproj"),
            ["FrontEnd.Web"] = Path.Combine(repoRoot, "Visage.FrontEnd", "Visage.FrontEnd.Web", "Visage.FrontEnd.Web.csproj"),
            ["Services.Eventing"] = Path.Combine(repoRoot, "services", "Visage.Services.Eventing", "Visage.Services.Eventing.csproj"),
            ["Services.Registrations"] = Path.Combine(repoRoot, "services", "Visage.Services.Registrations", "Visage.Services.Registrations.csproj")
        };
        
        // Act & Assert
        foreach (var (projectName, projectPath) in projectPaths)
        {
            File.Exists(projectPath).Should().BeTrue($"{projectName} project file should exist at {projectPath}");
            
            var projectContent = File.ReadAllText(projectPath);
            projectContent.Should().Contain("<UserSecretsId>",
                $"{projectName} should have UserSecretsId configured for local secret management");
        }
    }
    
    /// <summary>
    /// Verify that AppHost references required secret management packages
    /// </summary>
    [Test]
    public void AppHost_Should_Reference_AzureKeyVault_Packages()
    {
        // Arrange
        var repoRoot = GetRepositoryRoot();
        var appHostCsprojPath = Path.Combine(repoRoot, "Visage.AppHost", "Visage.AppHost.csproj");
        
        // Act
        var projectContent = File.ReadAllText(appHostCsprojPath);
        
        // Assert - Verify Azure packages are referenced
        projectContent.Should().Contain("Azure.Identity",
            "AppHost should reference Azure.Identity for Managed Identity authentication");
        projectContent.Should().Contain("Azure.Extensions.AspNetCore.Configuration.Secrets",
            "AppHost should reference Azure.Extensions.AspNetCore.Configuration.Secrets for Key Vault integration");
    }
    
    /// <summary>
    /// Verify that AppHost.cs has Key Vault configuration logic
    /// </summary>
    [Test]
    public void AppHost_Should_Have_KeyVault_Configuration_Logic()
    {
        // Arrange
        var repoRoot = GetRepositoryRoot();
        var appHostPath = Path.Combine(repoRoot, "Visage.AppHost", "AppHost.cs");
        
        // Act
        var appHostContent = File.ReadAllText(appHostPath);
        
        // Assert - Verify Key Vault configuration is present
        appHostContent.Should().Contain("Multi-Environment Secret Management",
            "AppHost should have multi-environment secret management documentation");
        appHostContent.Should().Contain("IsDevelopment()",
            "AppHost should check for Development environment");
        appHostContent.Should().Contain("KeyVault:VaultName",
            "AppHost should read KeyVault:VaultName configuration");
        appHostContent.Should().Contain("DefaultAzureCredential",
            "AppHost should use DefaultAzureCredential for Managed Identity");
        appHostContent.Should().Contain("AddAzureKeyVault",
            "AppHost should call AddAzureKeyVault to configure Key Vault");
    }
    
    /// <summary>
    /// Verify that SECRETS.md documentation exists and is comprehensive
    /// </summary>
    [Test]
    public void SECRETS_Documentation_Should_Exist_And_Be_Comprehensive()
    {
        // Arrange
        var repoRoot = GetRepositoryRoot();
        var secretsDocPath = Path.Combine(repoRoot, "SECRETS.md");
        
        // Act
        File.Exists(secretsDocPath).Should().BeTrue($"SECRETS.md documentation should exist at {secretsDocPath}");
        var secretsContent = File.ReadAllText(secretsDocPath);
        
        // Assert - Verify documentation covers all required topics
        var requiredSections = new[]
        {
            "Local Development Setup",
            "Production Configuration",
            "Azure Key Vault",
            "Managed Identity",
            "User Secrets",
            "Security Best Practices",
            "Troubleshooting",
            "auth0-clientsecret",
            "oauth-linkedin-clientid",
            "oauth-github-clientid",
            "cloudinary-apikey",
            "DefaultAzureCredential"
        };
        
        foreach (var section in requiredSections)
        {
            secretsContent.Should().Contain(section,
                $"SECRETS.md should document '{section}' for complete secret management guidance");
        }
    }
    
    /// <summary>
    /// Verify that README.md references the SECRETS.md file
    /// </summary>
    [Test]
    public void README_Should_Reference_SECRETS_Documentation()
    {
        // Arrange
        var repoRoot = GetRepositoryRoot();
        var readmePath = Path.Combine(repoRoot, "README.md");
        
        // Act
        var readmeContent = File.ReadAllText(readmePath);
        
        // Assert
        readmeContent.Should().Contain("SECRETS.md",
            "README.md should reference SECRETS.md to guide developers to secret setup instructions");
    }
}
