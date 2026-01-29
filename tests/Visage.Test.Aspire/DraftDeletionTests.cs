using Aspire.Hosting;
using Aspire.Hosting.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;
using TUnit.Core;
using Visage.Shared.Models;

namespace Visage.Test.Aspire;

/// <summary>
/// Integration tests for draft deletion functionality (US4 - Partial Progress Save)
/// Tests DELETE /api/profile/draft/{section} endpoint validation
/// </summary>
// Requires Auth0 - mark tests explicitly to avoid running in default CI test runs
[Category("RequiresAuth")]
[AuthRequired]
[NotInParallel]
public class DraftDeletionTests
{
    private static async Task<User> CreateTestUserAsync(HttpClient httpClient, string email)
    {
        var user = new User
        {
            FirstName = "Delete",
            LastName = "Test",
            Email = email,
            MobileNumber = "+919876543223",
            AddressLine1 = "Test Address",
            City = "Mumbai",
            State = "Maharashtra",
            PostalCode = "400001",
            GovtIdLast4Digits = "9900",
            GovtIdType = "Aadhaar",
            OccupationStatus = "Employed",
            CompanyName = "Visage Tests"
        };

        var response = await httpClient.PostAsJsonAsync("/api/users", user);
        response.IsSuccessStatusCode.Should().BeTrue($"/api/users should succeed but got {response.StatusCode}");

        var saved = await response.Content.ReadFromJsonAsync<User>();
        saved.Should().NotBeNull("/api/users should return the saved user");
        return saved!;
    }

    /// <summary>
    /// T051.7: Verify deleting an existing draft removes it from database
    /// Validates draft cleanup after form submission
    /// </summary>
    [Test]
    public async Task Draft_Deletion_Should_Remove_Existing_Draft()
    {
        AuthTestGuard.RequireAuthConfigured();
        // Arrange
        await TestAppContext.WaitForResourceAsync("userprofile-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("userprofile-api");

        // Attach authorization header for protected endpoints
        await TestAppContext.SetDefaultAuthHeader(httpClient);

        var createdUser = await CreateTestUserAsync(httpClient, $"delete-{Guid.NewGuid():N}@example.com");

        // Save a draft first
        var draft = new
        {
            UserId = createdUser.Id.Value,
            Section = "aide",
            DraftData = JsonSerializer.Serialize(new Dictionary<string, string> { ["Gender"] = "Male" })
        };

        var saveResponse = await httpClient.PostAsJsonAsync("/api/profile/draft", draft);
        saveResponse.IsSuccessStatusCode.Should().BeTrue("Draft should save successfully");

        // Verify draft exists
        var getBeforeDelete = await httpClient.GetAsync("/api/profile/draft/aide");
        getBeforeDelete.IsSuccessStatusCode.Should().BeTrue("Draft should exist before deletion");

        // Act - Delete the draft
        var deleteResponse = await httpClient.DeleteAsync("/api/profile/draft/aide");

        // Assert - Deletion successful
        deleteResponse.IsSuccessStatusCode.Should().BeTrue("Draft deletion should succeed");

        // Verify draft no longer exists
        var getAfterDelete = await httpClient.GetAsync("/api/profile/draft/aide");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound, "Draft should not exist after deletion");
    }

    /// <summary>
    /// T051.8: Verify deleting non-existent draft returns appropriate response
    /// </summary>
    [Test]
    public async Task Draft_Deletion_Should_Handle_Non_Existent_Draft_Gracefully()
    {
        AuthTestGuard.RequireAuthConfigured();
        // Arrange
        await TestAppContext.WaitForResourceAsync("userprofile-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("userprofile-api");

        // Attach authorization header for protected endpoints
        await TestAppContext.SetDefaultAuthHeader(httpClient);

        await CreateTestUserAsync(httpClient, $"nodrafttodelete-{Guid.NewGuid():N}@example.com");

        // Act - Try to delete a draft that doesn't exist
        var deleteResponse = await httpClient.DeleteAsync("/api/profile/draft/aide");

        // Assert - Should either return 204 No Content (idempotent) or 404 Not Found
        (deleteResponse.StatusCode == HttpStatusCode.NoContent || 
         deleteResponse.StatusCode == HttpStatusCode.NotFound).Should().BeTrue(
            "Deleting non-existent draft should return 204 or 404");
    }

    /// <summary>
    /// T051.10: Verify deleting mandatory section draft works
    /// </summary>
    [Test]
    public async Task Draft_Deletion_Should_Work_For_Mandatory_Section()
    {
        AuthTestGuard.RequireAuthConfigured();
        // Arrange
        await TestAppContext.WaitForResourceAsync("userprofile-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("userprofile-api");

        // Attach authorization header for protected endpoints
        await TestAppContext.SetDefaultAuthHeader(httpClient);

        var createdUser = await CreateTestUserAsync(httpClient, $"mandatorydelete-{Guid.NewGuid():N}@example.com");

        // Save mandatory section draft
        var draft = new
        {
            UserId = createdUser.Id.Value,
            Section = "mandatory",
            DraftData = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["FirstName"] = "Updated",
                ["Email"] = "updated@example.com"
            })
        };

        await httpClient.PostAsJsonAsync("/api/profile/draft", draft);

        // Act - Delete mandatory section draft
        var deleteResponse = await httpClient.DeleteAsync("/api/profile/draft/mandatory");

        // Assert
        deleteResponse.IsSuccessStatusCode.Should().BeTrue("Mandatory section draft deletion should work");

        // Verify draft was deleted
        var getAfterDelete = await httpClient.GetAsync("/api/profile/draft/mandatory");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound, "Draft should not exist after deletion");
    }

    /// <summary>
    /// T051.11: Verify draft deletion is idempotent (multiple deletes don't cause errors)
    /// </summary>
    [Test]
    public async Task Draft_Deletion_Should_Be_Idempotent()
    {
        AuthTestGuard.RequireAuthConfigured();
        // Arrange
        await TestAppContext.WaitForResourceAsync("userprofile-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("userprofile-api");

        // Attach authorization header for protected endpoints
        await TestAppContext.SetDefaultAuthHeader(httpClient);

        var createdUser = await CreateTestUserAsync(httpClient, $"idempotentdelete-{Guid.NewGuid():N}@example.com");

        // Save draft
        var draft = new
        {
            UserId = createdUser.Id.Value,
            Section = "aide",
            DraftData = JsonSerializer.Serialize(new Dictionary<string, string> { ["Test"] = "Value" })
        };

        await httpClient.PostAsJsonAsync("/api/profile/draft", draft);

        // Act - Delete the same draft multiple times
        var firstDelete = await httpClient.DeleteAsync("/api/profile/draft/aide");
        var secondDelete = await httpClient.DeleteAsync("/api/profile/draft/aide");
        var thirdDelete = await httpClient.DeleteAsync("/api/profile/draft/aide");

        // Assert - All deletes should succeed (idempotent)
        firstDelete.IsSuccessStatusCode.Should().BeTrue("First delete should succeed");
        (secondDelete.StatusCode == HttpStatusCode.NoContent || 
         secondDelete.StatusCode == HttpStatusCode.NotFound).Should().BeTrue(
            "Second delete should be idempotent");
        (thirdDelete.StatusCode == HttpStatusCode.NoContent || 
         thirdDelete.StatusCode == HttpStatusCode.NotFound).Should().BeTrue(
            "Third delete should be idempotent");
    }
}
