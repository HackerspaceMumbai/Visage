using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using TUnit.Core;
using Visage.Shared.Models;
using Visage.Services.UserProfile;
using Microsoft.Extensions.Configuration;

namespace Visage.Test.Aspire;

[Category("RequiresAuth")]
[NotInParallel]
public sealed class SocialProfileDisconnectTests
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

        var response = await httpClient.PostAsJsonAsync("/register", user);
        response.IsSuccessStatusCode.Should().BeTrue($"/register should succeed but got {response.StatusCode}");

        var saved = await response.Content.ReadFromJsonAsync<User>();
        saved.Should().NotBeNull("/register should return the saved user");
        saved!.Email.Should().Be(email);
        return saved;
    }

    [Test]
    public async Task Disconnect_Should_Clear_Verification_And_Record_Audit()
    {
        await TestAppContext.WaitForResourceAsync("userprofile-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("userprofile-api");
        await EnsureAuthBackedUserAsync(httpClient);

        var email = GetTestUserEmail();

        // Seed a verified LinkedIn profile for the current user
        await using (var db = CreateUserDb())
        {
            var user = await db.Users
                .Where(u => u.Email == email)
                .OrderByDescending(u => u.ProfileCompletedAt)
                .FirstAsync();

            user.LinkedInProfile = $"https://www.linkedin.com/in/visage-test-disconnect-{Guid.NewGuid():N}";
            user.IsLinkedInVerified = true;
            user.LinkedInVerifiedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        await TestAppContext.SetDefaultAuthHeader(httpClient);

        var dto = new SocialDisconnectDto { Provider = "linkedin" };
        var response = await httpClient.PostAsJsonAsync("/api/profile/social/disconnect", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var status = await response.Content.ReadFromJsonAsync<SocialConnectionStatusDto>();
        status.Should().NotBeNull();
        status!.LinkedIn.IsConnected.Should().BeFalse();
        status.LinkedIn.ProfileUrl.Should().BeNull();
        status.LinkedIn.VerifiedAt.Should().BeNull();

        // Verify DB cleared and audit recorded
        await using var verifyDb = CreateUserDb();
        var authUser = await verifyDb.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .OrderByDescending(u => u.ProfileCompletedAt)
            .FirstAsync();

        authUser.LinkedInProfile.Should().BeNull();
        authUser.IsLinkedInVerified.Should().BeFalse();

        var audit = await verifyDb.SocialVerificationEvents
            .AsNoTracking()
            .Where(e => e.UserId == authUser.Id && e.Provider == "linkedin" && e.Action == "disconnect")
            .OrderByDescending(e => e.OccurredAtUtc)
            .FirstOrDefaultAsync();

        audit.Should().NotBeNull("disconnect should write a durable audit event");
    }
}