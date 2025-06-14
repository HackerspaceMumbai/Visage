using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrictId;
using Visage.Shared.Models;

namespace Visage.Services.Registration;

public static class ProfileApi
{
    public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile").RequireAuthorization("profile:read-write");
        
        // Retrieve profile by user ID
        group.MapGet("/{userId}", async (string userId, RegistrantDB db) =>
        {
            // Validate StrictId format
            if (!Id<Registrant>.TryParse(userId, out var parsedId))
                return Results.BadRequest("Invalid user ID");
            
            var registrant = await db.Registrants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == parsedId);
            if (registrant is null)
                return Results.NotFound();
            
            return Results.Ok(new UserProfileDto
            {
                // Combine first and last name for display
                Name = registrant.FirstName + (string.IsNullOrWhiteSpace(registrant.LastName) ? "" : " " + registrant.LastName),
                Email = registrant.Email,
                LinkedIn = registrant.LinkedInProfile ?? string.Empty,
                GitHub = registrant.GitHubProfile ?? string.Empty,
                // No registration timestamp stored; default to now
                RegistrationDate = DateTime.Now
            });
        });
        
        // Update profile by user ID
        group.MapPut("/{userId}", async (string userId, RegistrantDB db, UserProfileDto dto) =>
        {
            if (!Id<Registrant>.TryParse(userId, out var parsedId))
                return Results.BadRequest("Invalid user ID");
            
            var registrant = await db.Registrants
                .FirstOrDefaultAsync(r => r.Id == parsedId);
            if (registrant is null)
                return Results.NotFound();
            
            // Update only profile properties
            // Optionally split dto.Name into FirstName/LastName
            registrant.LinkedInProfile = dto.LinkedIn;
            registrant.GitHubProfile = dto.GitHub;
            await db.SaveChangesAsync();
            
            return Results.NoContent();
        });

        group.MapGet("/", async (HttpContext http, RegistrantDB db, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("AuthorizationLogger");

            if (http.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString();
                logger.LogInformation("Authorization Header: {Token}", token);
            }
            else
            {
                logger.LogWarning("Authorization header not found.");
            }


            var userId = http.User.FindFirst("sub")?.Value;
            logger.LogInformation("userId: " + userId);

            if (userId is null)
                return Results.Unauthorized();

            if (!Id<Registrant>.TryParse(userId, out var parsedUserId))
                return Results.BadRequest("Invalid user ID format.");

            var registrant = await db.Registrants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == parsedUserId);

            if (registrant is null)
                return Results.NotFound();

            return Results.Ok(new UserProfileDto
            {
                Name = registrant.FirstName + " " + registrant.LastName,
                Email = registrant.Email,
                LinkedIn = registrant.LinkedInProfile,
                GitHub = registrant.GitHubProfile,
                //RegistrationDate = registrant
            });
        });

        group.MapPut("/", async (HttpContext http, RegistrantDB db, UserProfileDto dto) =>
        {
            var userId = http.User.FindFirst("sub")?.Value;
            if (userId is null)
                return Results.Unauthorized();

            if (!Id<Registrant>.TryParse(userId, out var parsedUserId))
                return Results.BadRequest("Invalid user ID format.");

            var registrant = await db.Registrants
                .FirstOrDefaultAsync(r => r.Id == parsedUserId);

            if (registrant is null)
                return Results.NotFound();

            registrant.FirstName = dto.Name;
            registrant.LinkedInProfile = dto.LinkedIn;
            registrant.GitHubProfile = dto.GitHub;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}
