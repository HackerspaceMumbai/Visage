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
/// Integration tests for draft retrieval functionality (US4 - Partial Progress Save)
/// Tests T051: GET /api/profile/draft/{section} endpoint validation
/// </summary>
// Requires Auth0 - mark tests explicitly to avoid running in default CI test runs
[Category("RequiresAuth")]
[AuthRequired]
[NotInParallel]
public class DraftRetrievalTests
{
    private static async Task<User> CreateTestUserAsync(HttpClient httpClient, string email)
    {
        var user = new User
        {
            FirstName = "Retrieve",
            LastName = "Test",
            Email = email,
            MobileNumber = "+919876543217",
            AddressLine1 = "Test Address",
            City = "Mumbai",
            State = "Maharashtra",
            PostalCode = "400001",
            GovtIdLast4Digits = "1357",
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
    /// T051.1: Verify retrieving a valid, non-expired draft returns the saved data
    /// </summary>
    [Test]
    public async Task Draft_Retrieval_Should_Return_Valid_Non_Expired_Draft()
    {
        AuthTestGuard.RequireAuthConfigured();

        // Arrange
        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");

        // Attach authorization header for protected endpoints
        await TestAppContext.SetDefaultAuthHeader(httpClient);

        var createdUser = await CreateTestUserAsync(httpClient, $"retrieve-{Guid.NewGuid():N}@example.com");

        // Save a draft first
        var draftData = new Dictionary<string, string>
        {
            ["Gender"] = "Female",
            ["Religion"] = "Buddhist",
            ["LanguagesSpoken"] = "Marathi, English"
        };

        var draft = new
        {
            UserId = createdUser.Id.Value,
            Section = "aide",
            DraftData = JsonSerializer.Serialize(draftData)
        };

        var saveResponse = await httpClient.PostAsJsonAsync("/api/profile/draft", draft);
        saveResponse.IsSuccessStatusCode.Should().BeTrue();

        // Act - Retrieve the draft
        var retrieveResponse = await httpClient.GetAsync("/api/profile/draft/aide");

        // Assert
        retrieveResponse.IsSuccessStatusCode.Should().BeTrue("Draft retrieval should succeed");

        var retrievedDraft = await retrieveResponse.Content.ReadFromJsonAsync<JsonElement>();
        retrievedDraft.GetProperty("section").GetString().Should().Be("aide");

        var retrievedDataJson = retrievedDraft.GetProperty("draftData").GetString();
        var retrievedDataObj = JsonSerializer.Deserialize<Dictionary<string, string>>(retrievedDataJson!);

        retrievedDataObj.Should().ContainKey("Gender").WhoseValue.Should().Be("Female");
        retrievedDataObj.Should().ContainKey("Religion").WhoseValue.Should().Be("Buddhist");
        retrievedDataObj.Should().ContainKey("LanguagesSpoken").WhoseValue.Should().Be("Marathi, English");
    }

    /// <summary>
    /// T051.2: Verify retrieving non-existent draft returns 404
    /// </summary>
    [Test]
    public async Task Draft_Retrieval_Should_Return_404_For_Non_Existent_Draft()
    {
        AuthTestGuard.RequireAuthConfigured();

        // Arrange
        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");

        // Attach authorization header for protected endpoints
        await TestAppContext.SetDefaultAuthHeader(httpClient);

        // Act - Try to retrieve a draft that was never saved
        var retrieveResponse = await httpClient.GetAsync("/api/profile/draft/aide");

        // Assert
        retrieveResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "Non-existent draft should return 404");
    }

    /// <summary>
    /// T051.3: Verify retrieving expired draft returns 404
    /// Validates FR-008: Draft expiration after 30 days
    /// </summary>
    [Test]
    [Skip("Expiration testing requires time mocking - manual verification needed")]
    public async Task Draft_Retrieval_Should_Return_404_For_Expired_Draft()
    {
        AuthTestGuard.RequireAuthConfigured();

        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        await TestAppContext.SetDefaultAuthHeader(httpClient);

        // Skipped until time mocking exists.
        await CreateTestUserAsync(httpClient, $"expired-{Guid.NewGuid():N}@example.com");
    }

    /// <summary>
    /// T051.4: Verify retrieving draft that was already applied returns 404
    /// Validates IsApplied flag prevents duplicate restoration
    /// </summary>
    [Test]
    [Skip("IsApplied flag testing requires ProfileApi update endpoint integration")]
    public async Task Draft_Retrieval_Should_Return_404_For_Applied_Draft()
    {
        AuthTestGuard.RequireAuthConfigured();

        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        await TestAppContext.SetDefaultAuthHeader(httpClient);

        // Skipped until there is an API that marks drafts as applied.
        await CreateTestUserAsync(httpClient, $"applied-{Guid.NewGuid():N}@example.com");
    }

    // T051.5 (email-based lookup) removed: current API resolves the user via JWT claims.

    /// <summary>
    /// T051.6: Verify mandatory section draft retrieval
    /// </summary>
    [Test]
    public async Task Draft_Retrieval_Should_Work_For_Mandatory_Section()
    {
        AuthTestGuard.RequireAuthConfigured();

        // Arrange
        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");

        // Attach authorization header for protected endpoints
        await TestAppContext.SetDefaultAuthHeader(httpClient);

        var createdUser = await CreateTestUserAsync(httpClient, $"mandatoryretrieval-{Guid.NewGuid():N}@example.com");

        // Save mandatory section draft
        var mandatoryData = new Dictionary<string, string>
        {
            ["FirstName"] = "Partial",
            ["Email"] = "partial@example.com"
        };

        var draft = new
        {
            UserId = createdUser.Id.Value,
            Section = "mandatory",
            DraftData = JsonSerializer.Serialize(mandatoryData)
        };

        await httpClient.PostAsJsonAsync("/api/profile/draft", draft);

        // Act - Retrieve mandatory section draft
        var retrieveResponse = await httpClient.GetAsync("/api/profile/draft/mandatory");

        // Assert
        retrieveResponse.IsSuccessStatusCode.Should().BeTrue("Mandatory section draft retrieval should work");

        var retrievedDraft = await retrieveResponse.Content.ReadFromJsonAsync<JsonElement>();
        retrievedDraft.GetProperty("section").GetString().Should().Be("mandatory");
    }
}
