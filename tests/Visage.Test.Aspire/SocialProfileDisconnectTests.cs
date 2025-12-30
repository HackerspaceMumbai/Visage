using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using TUnit.Core;
using Visage.Shared.Models;
using Visage.Services.Registration;
using Microsoft.Extensions.Configuration;
namespace Visage.Test.Aspire;

[Category("RequiresAuth")]
[NotInParallel]
public sealed class SocialProfileDisconnectTests
{
    private static string GetTestUserEmail()
        => Environment.GetEnvironmentVariable("TEST_USER_EMAIL")
           ?? throw new InvalidOperationException("TEST_USER_EMAIL not set. Required for auth-backed social profile tests.");

    private static string GetRegistrationDbConnectionString()
    {
        var configuration = TestAppContext.App.Services.GetRequiredService<IConfiguration>();

        var cs = configuration.GetConnectionString("registrationdb")
                 ?? configuration["ConnectionStrings:registrationdb"]
                 ?? configuration["ConnectionStrings__registrationdb"];

        if (!string.IsNullOrWhiteSpace(cs))
            return cs;

        var match = configuration
            .AsEnumerable()
            .FirstOrDefault(kvp => kvp.Key.Contains("registrationdb", StringComparison.OrdinalIgnoreCase)
                                  && !string.IsNullOrWhiteSpace(kvp.Value));

        if (!string.IsNullOrWhiteSpace(match.Value))
            return match.Value!;

        throw new InvalidOperationException("Unable to resolve registrationdb connection string from configuration.");
    }

    private static RegistrantDB CreateRegistrantDb()
    {
        var options = new DbContextOptionsBuilder<RegistrantDB>()
            .UseSqlServer(GetRegistrationDbConnectionString())
            .Options;

        return new RegistrantDB(options);
    }

    private static async Task<Registrant> EnsureAuthBackedRegistrantAsync(HttpClient httpClient)
    {
        var email = GetTestUserEmail();

        var registrant = new Registrant
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
            CompanyName = "Visage Tests"
        };

        var response = await httpClient.PostAsJsonAsync("/register", registrant);
        response.IsSuccessStatusCode.Should().BeTrue($"/register should succeed but got {response.StatusCode}");

        var saved = await response.Content.ReadFromJsonAsync<Registrant>();
        saved.Should().NotBeNull("/register should return the saved registrant");
        saved!.Email.Should().Be(email);
        return saved;
    }

    [Test]
    public async Task Disconnect_Should_Clear_Verification_And_Record_Audit()
    {
        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");
        await EnsureAuthBackedRegistrantAsync(httpClient);

        var email = GetTestUserEmail();

        // Seed a verified LinkedIn profile for the current registrant
        await using (var db = CreateRegistrantDb())
        {
            var registrant = await db.Registrants
                .Where(r => r.Email == email)
                .OrderByDescending(r => r.ProfileCompletedAt)
                .FirstAsync();

            registrant.LinkedInProfile = $"https://www.linkedin.com/in/visage-test-disconnect-{Guid.NewGuid():N}";
            registrant.IsLinkedInVerified = true;
            registrant.LinkedInVerifiedAt = DateTime.UtcNow;
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
        await using var verifyDb = CreateRegistrantDb();
        var authRegistrant = await verifyDb.Registrants
            .AsNoTracking()
            .Where(r => r.Email == email)
            .OrderByDescending(r => r.ProfileCompletedAt)
            .FirstAsync();

        authRegistrant.IsLinkedInVerified.Should().BeFalse();
        authRegistrant.LinkedInProfile.Should().BeNull();

        var disconnectEvent = await verifyDb.SocialVerificationEvents
            .AsNoTracking()
            .Where(e => e.RegistrantId == authRegistrant.Id && e.Action == "disconnect")
            .OrderByDescending(e => e.OccurredAtUtc)
            .FirstOrDefaultAsync();

        disconnectEvent.Should().NotBeNull("disconnect should create an audit event");
        disconnectEvent!.Outcome.Should().Be("succeeded");
    }
}