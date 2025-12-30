using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using TUnit.Core;
using Visage.Shared.Models;
using Visage.Services.Registration;
using Microsoft.Extensions.Configuration;
namespace Visage.Test.Aspire;

[NotInParallel]
public sealed class RegistrantCurationSocialFieldsTests
{
    private static string GetRegistrationDbConnectionString()
        => ResolveRegistrationDbConnectionString();

    private static RegistrantDB CreateRegistrantDb()
    {
        var options = new DbContextOptionsBuilder<RegistrantDB>()
            .UseSqlServer(ResolveRegistrationDbConnectionString())
            .Options;

        return new RegistrantDB(options);
    }

    private static string ResolveRegistrationDbConnectionString()
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

    [Test]
    public async Task Register_List_Should_Include_Verified_Social_Fields()
    {
        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");

        // Seed a registrant with verified social fields
        string linkedin = $"https://www.linkedin.com/in/curation-{Guid.NewGuid():N}";
        string github = $"https://github.com/curation-{Guid.NewGuid():N}";

        await using (var db = CreateRegistrantDb())
        {
            var r = new Registrant
            {
                FirstName = "Curate",
                LastName = "Me",
                Email = $"curate-{Guid.NewGuid():N}@example.com",
                MobileNumber = "+919800000000",
                AddressLine1 = "Curation St",
                City = "Mumbai",
                State = "Maharashtra",
                PostalCode = "400001",
                OccupationStatus = "Employed",
                CompanyName = "Curation Co",

                LinkedInProfile = linkedin,
                IsLinkedInVerified = true,
                LinkedInVerifiedAt = DateTime.UtcNow,

                GitHubProfile = github,
                IsGitHubVerified = true,
                GitHubVerifiedAt = DateTime.UtcNow
            };

            db.Registrants.Add(r);
            await db.SaveChangesAsync();
        }

        var response = await httpClient.GetAsync("/register");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var registrants = await response.Content.ReadFromJsonAsync<Registrant[]>();
        registrants.Should().NotBeNull();
        registrants!.Should().Contain(r => r.LinkedInProfile == linkedin && r.IsLinkedInVerified && r.GitHubProfile == github && r.IsGitHubVerified);
    }
}