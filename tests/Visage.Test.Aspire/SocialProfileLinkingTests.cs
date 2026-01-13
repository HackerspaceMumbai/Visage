using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TUnit.Core;
using Visage.Services.UserProfile;
using Visage.Shared.Models;

namespace Visage.Test.Aspire;

/// <summary>
/// Integration tests for OAuth-verified social profile linking (US1).
/// Validates status + link-callback behavior, uniqueness conflicts, and durable audit trail.
/// </summary>
// Requires Auth0 - mark tests explicitly to avoid running in default CI test runs
[Category("RequiresAuth")]
[NotInParallel]
public sealed class SocialProfileLinkingTests
{
    private static string GetTestUserEmail()
        => Environment.GetEnvironmentVariable("TEST_USER_EMAIL")
           ?? throw new InvalidOperationException("TEST_USER_EMAIL not set. Required for auth-backed social profile tests.");

    private static string GetUserProfileDbConnectionString()
    {
        var configuration = TestAppContext.App.Services.GetRequiredService<IConfiguration>();

        var cs = configuration.GetConnectionString("userprofiledb")
                 ?? configuration["ConnectionStrings:userprofiledb"]
                 ?? configuration["ConnectionStrings__userprofiledb"];

        if (!string.IsNullOrWhiteSpace(cs))
            return cs;

        var match = configuration
            .AsEnumerable()
            .FirstOrDefault(kvp => kvp.Key.Contains("userprofiledb", StringComparison.OrdinalIgnoreCase)
                                  && !string.IsNullOrWhiteSpace(kvp.Value));

        if (!string.IsNullOrWhiteSpace(match.Value))
            return match.Value!;

        throw new InvalidOperationException("Unable to resolve userprofiledb connection string from configuration.");
    }

    private static UserDB CreateUserDb()
    {
        var options = new DbContextOptionsBuilder<UserDB>()
            .UseSqlServer(GetUserProfileDbConnectionString())
            .Options;

        return new UserDB(options);
    }

    private static async Task<User> EnsureAuthBackedUserAsync(HttpClient httpClient)
    {
        var email = GetTestUserEmail();

        var user = new User
        {
            FirstName = "Auth",
            LastName = "User",
            Email = email,
            MobileNumber = "+919876543299",
            AddressLine1 = "Test Address",
            City = "Mumbai",
            State = "Maharashtra",
            PostalCode = "400001",
            GovtIdLast4Digits = "1234",
            GovtIdType = "Aadhaar",
            OccupationStatus = "Employed",
            CompanyName = "Visage Tests",
            IsProfileComplete = true,
            ProfileCompletedAt = DateTime.UtcNow
        };

        // /register is an upsert endpoint by email.
        var response = await httpClient.PostAsJsonAsync("/register", user);
        response.IsSuccessStatusCode.Should().BeTrue($"/register should succeed but got {response.StatusCode}");

        var saved = await response.Content.ReadFromJsonAsync<User>();
        saved.Should().NotBeNull("/register should return the saved user");
        saved!.Email.Should().Be(email);
        return saved;
    }

    private static async Task ResetSocialStateForEmailAsync(string email)
    {
        await using var db = CreateUserDb();

        var user = await db.Users
            .Where(u => u.Email == email)
            .OrderByDescending(u => u.ProfileCompletedAt)
            .FirstOrDefaultAsync();

        if (user is null)
            return;

        user.LinkedInProfile = null;
        user.GitHubProfile = null;

        user.IsLinkedInVerified = false;
        user.LinkedInVerifiedAt = null;
        user.IsGitHubVerified = false;
        user.GitHubVerifiedAt = null;

        // Clean audit events for deterministic assertions.
        await db.SocialVerificationEvents
            .Where(e => e.UserId == user.Id)
            .ExecuteDeleteAsync();

        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Social_Status_Should_Default_To_Disconnected_When_Not_Linked()
    {
        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        await EnsureAuthBackedUserAsync(httpClient);

        var email = GetTestUserEmail();
        await ResetSocialStateForEmailAsync(email);

        await TestAppContext.SetDefaultAuthHeader(httpClient);

        var response = await httpClient.GetAsync("/api/profile/social/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var status = await response.Content.ReadFromJsonAsync<SocialConnectionStatusDto>();
        status.Should().NotBeNull();

        status!.LinkedIn.IsConnected.Should().BeFalse();
        status.LinkedIn.ProfileUrl.Should().BeNull();
        status.LinkedIn.VerifiedAt.Should().BeNull();

        status.GitHub.IsConnected.Should().BeFalse();
        status.GitHub.ProfileUrl.Should().BeNull();
        status.GitHub.VerifiedAt.Should().BeNull();
    }

    [Test]
    public async Task Social_LinkCallback_Should_Persist_Verification_And_Create_Audit_Event()
    {
        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        await EnsureAuthBackedUserAsync(httpClient);

        var email = GetTestUserEmail();
        await ResetSocialStateForEmailAsync(email);

        await TestAppContext.SetDefaultAuthHeader(httpClient);

        var linkedInUrl = $"https://www.linkedin.com/in/visage-test-{Guid.NewGuid():N}";
        var linkDto = new SocialProfileLinkDto
        {
            Provider = "linkedin",
            ProfileUrl = linkedInUrl
        };

        var response = await httpClient.PostAsJsonAsync("/api/profile/social/link-callback", linkDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var responseDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        responseDoc.RootElement.GetProperty("profileUrl").GetString().Should().Be(linkedInUrl);
        responseDoc.RootElement.GetProperty("verifiedAt").GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(2));

        // Verify status reflects new linkage
        var statusResponse = await httpClient.GetAsync("/api/profile/social/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var status = await statusResponse.Content.ReadFromJsonAsync<SocialConnectionStatusDto>();
        status!.LinkedIn.IsConnected.Should().BeTrue();
        status.LinkedIn.ProfileUrl.Should().Be(linkedInUrl);
        status.LinkedIn.VerifiedAt.Should().NotBeNull();

        // Verify DB persisted + audit event written
        await using var db = CreateUserDb();
        var user = await db.Users
            .AsNoTracking()
            .Where(r => r.Email == email)
            .OrderByDescending(r => r.ProfileCompletedAt)
            .FirstAsync();

        user.IsLinkedInVerified.Should().BeTrue();
        user.LinkedInProfile.Should().Be(linkedInUrl);
        user.LinkedInVerifiedAt.Should().NotBeNull();

        var auditEvents = await db.SocialVerificationEvents
            .AsNoTracking()
            .Where(e => e.UserId == user.Id && e.Provider == "linkedin")
            .ToListAsync();

        auditEvents.Should().NotBeEmpty("a successful link should create an audit event");
        auditEvents.Should().Contain(e => e.Outcome == "succeeded" && e.ProfileUrl == linkedInUrl);
    }

    [Test]
    public async Task Social_LinkCallback_Should_Return_409_When_Profile_Is_Already_Verified_By_Another_Registrant()
    {
        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        await EnsureAuthBackedUserAsync(httpClient);

        var email = GetTestUserEmail();
        await ResetSocialStateForEmailAsync(email);

        var conflictUrl = $"https://www.linkedin.com/in/visage-conflict-{Guid.NewGuid():N}";

        // Seed a conflicting verified registrant directly in DB.
        await using (var db = CreateUserDb())
        {
            var otherRegistrant = new User
            {
                FirstName = "Other",
                LastName = "Registrant",
                Email = $"other-{Guid.NewGuid():N}@example.com",
                MobileNumber = "+919876543298",
                AddressLine1 = "Other Address",
                City = "Mumbai",
                State = "Maharashtra",
                PostalCode = "400001",
                GovtIdLast4Digits = "5678",
                GovtIdType = "Aadhaar",
                OccupationStatus = "Employed",
                CompanyName = "Other Co",

                LinkedInProfile = conflictUrl,
                IsLinkedInVerified = true,
                LinkedInVerifiedAt = DateTime.UtcNow
            };

            db.Users.Add(otherRegistrant);
            await db.SaveChangesAsync();
        }

        await TestAppContext.SetDefaultAuthHeader(httpClient);

        var linkDto = new SocialProfileLinkDto
        {
            Provider = "linkedin",
            ProfileUrl = conflictUrl
        };

        var response = await httpClient.PostAsJsonAsync("/api/profile/social/link-callback", linkDto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.Should().Be("application/problem+json");

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(409);
        doc.RootElement.GetProperty("type").GetString().Should().Be("https://visage.app/problems/social-profile-conflict");

        // Verify an audit event is recorded for the failed attempt.
        await using var verifyDb = CreateUserDb();
        var authRegistrant = await verifyDb.Users
            .AsNoTracking()
            .Where(r => r.Email == email)
            .OrderByDescending(r => r.ProfileCompletedAt)
            .FirstAsync();

        var failureAudit = await verifyDb.SocialVerificationEvents
            .AsNoTracking()
            .Where(e => e.UserId == authRegistrant.Id && e.Provider == "linkedin" && e.Outcome == "failed")
            .OrderByDescending(e => e.OccurredAtUtc)
            .FirstOrDefaultAsync();

        failureAudit.Should().NotBeNull("conflict attempts must be auditable");
        failureAudit!.ProfileUrl.Should().Be(conflictUrl);
        failureAudit.FailureReason.Should().NotBeNullOrWhiteSpace();
    }
}
