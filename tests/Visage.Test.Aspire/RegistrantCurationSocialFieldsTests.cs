using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using TUnit.Core;
using Visage.Shared.Models;
using Visage.Services.UserProfile;
using Microsoft.Extensions.Configuration;

namespace Visage.Test.Aspire;

[NotInParallel]
public sealed class RegistrantCurationSocialFieldsTests
{
    private static string GetUserProfileDbConnectionString()
        => ResolveUserProfileDbConnectionString();

    private static UserDB CreateUserDb()
    {
        var options = new DbContextOptionsBuilder<UserDB>()
            .UseSqlServer(ResolveUserProfileDbConnectionString())
            .Options;

        return new UserDB(options);
    }

    private static string ResolveUserProfileDbConnectionString()
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

    [Test]
    public async Task Register_List_Should_Include_Verified_Social_Fields()
    {
        await TestAppContext.WaitForResourceAsync("registrations-api", KnownResourceStates.Running, TimeSpan.FromSeconds(90));

        var httpClient = TestAppContext.CreateHttpClient("registrations-api");

        // Seed a user with verified social fields
        string linkedin = $"https://www.linkedin.com/in/curation-{Guid.NewGuid():N}";
        string github = $"https://github.com/curation-{Guid.NewGuid():N}";

        await using (var db = CreateUserDb())
        {
            var u = new User
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
                GitHubVerifiedAt = DateTime.UtcNow,

                IsProfileComplete = true,
                ProfileCompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(u);
            await db.SaveChangesAsync();
        }

        var response = await httpClient.GetAsync("/register");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<User[]>();
        users.Should().NotBeNull();
        users!.Should().Contain(u => u.LinkedInProfile == linkedin && u.IsLinkedInVerified && u.GitHubProfile == github && u.IsGitHubVerified);
    }
}