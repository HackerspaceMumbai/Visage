using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;
using StrictId;
using Visage.Shared.Models;
using Visage.Services.Registration.Repositories;

namespace Visage.Services.Registration;

public static class ProfileApi
{
    private static readonly string[] EmailClaimTypes =
    [
        "email",
        ClaimTypes.Email,
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
        "https://hackmum.in/claims/email"
    ];

    public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        // T037: Simplified authorization - just require authenticated user
        var group = app.MapGroup("/api/profile").RequireAuthorization();

        // T010: GET /api/profile/completion-status - Check profile completion status
        group.MapGet("/completion-status", async (
            HttpContext http,
            ProfileCompletionRepository repo,
            RegistrantDB db,
            ILogger<ProfileCompletionRepository> logger) =>
        {
            // DEBUG: Log raw Authorization header and attempt to decode JWT payload
            try
            {
                if (http.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    logger.LogInformation("DEBUG: Authorization Header: {AuthHeader}", authHeader.ToString());
                    var parts = authHeader.ToString().Split(' ');
                    if (parts.Length >= 2)
                    {
                        var token = parts[1];
                        try
                        {
                            // Attempt to decode JWT payload (safe, no signature validation here)
                            var jwtParts = token.Split('.');
                            if (jwtParts.Length >= 2)
                            {
                                string payload = jwtParts[1];
                                // Add padding if necessary
                                int mod4 = payload.Length % 4;
                                if (mod4 > 0) payload += new string('=', 4 - mod4);
                                var bytes = Convert.FromBase64String(payload);
                                var json = System.Text.Encoding.UTF8.GetString(bytes);
                                logger.LogInformation("DEBUG: Access token payload (truncated): {Payload}", json.Length > 1000 ? json.Substring(0, 1000) : json);
                            }
                            else
                            {
                                logger.LogInformation("DEBUG: Token does not appear to be a JWT (no dot separators)");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "DEBUG: Failed to decode access token payload");
                        }
                    }
                }
                else
                {
                    logger.LogInformation("DEBUG: Authorization header not present on request");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "DEBUG: Error while logging Authorization header");
            }

            // T013: Add OpenTelemetry tracing
            using var activity = Activity.Current?.Source.StartActivity("CheckProfileCompletionStatus");
            
            // Inspect claims on the principal and try common claim types for the user id
            try
            {
                logger.LogInformation("DEBUG: Claims on principal:");
                foreach (var c in http.User.Claims)
                {
                    logger.LogInformation("DEBUG: Claim {Type} = {Value}", c.Type, c.Value);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to enumerate claims on principal");
            }

            string? userId = null;
            // Prefer raw 'sub' claim
            userId = http.User.FindFirst("sub")?.Value;
            // Fallback to common name identifier claim types
            if (string.IsNullOrWhiteSpace(userId))
                userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                userId = http.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            activity?.SetTag("userId", userId);

            if (userId is null)
            {
                logger.LogWarning("Profile completion check failed: No user ID in token");
                return Results.Unauthorized();
            }

            Id<Registrant>? parsedUserId = null;

            // If token contains our internal StrictId, use it directly
            if (!Id<Registrant>.TryParse(userId, out var tmpParsed))
            {
                // Not an internal StrictId. Try to locate the registrant by email (common pattern for external Id providers)
                var email = ResolveEmail(http.User);
                if (string.IsNullOrWhiteSpace(email))
                {
                    logger.LogWarning("Profile completion check failed: Invalid user ID format {UserId} and no email claim to fallback", userId);
                    // Treat as user not found so frontend can redirect to registration flow
                    return Results.NotFound(new { error = "User profile not found" });
                }

                // T036: Simplified query - EF Core can't translate StrictId.Value
                // Just get first match by email with most complete profile
                var registrantByEmail = await db.Registrants
                    .AsNoTracking()
                    .Where(r => r.Email == email)
                    .OrderByDescending(r => r.IsProfileComplete)
                    .ThenByDescending(r => r.ProfileCompletedAt.HasValue)
                    .ThenByDescending(r => r.ProfileCompletedAt)
                    .FirstOrDefaultAsync();
                if (registrantByEmail is null)
                {
                    logger.LogWarning("Profile completion check failed: No registrant found for email {Email}", email);
                    return Results.NotFound(new { error = "User profile not found" });
                }

                parsedUserId = registrantByEmail.Id;
            }
            else
            {
                parsedUserId = tmpParsed;
            }

            // Defensive null check for compiler nullability analysis
            if (!parsedUserId.HasValue)
            {
                logger.LogError("Profile completion check failed: parsedUserId is unexpectedly null");
                return Results.Problem("Internal error processing user ID");
            }

            var status = await repo.GetCompletionStatusAsync(parsedUserId.Value);
            if (status is null)
            {
                logger.LogWarning("Profile completion check failed: User not found {UserId}", userId);
                return Results.NotFound(new { error = "User profile not found" });
            }

            logger.LogInformation(
                "Profile completion checked for user {UserId}: Complete={IsComplete}, AIDE={IsAideComplete}",
                userId,
                status.IsProfileComplete,
                status.IsAideProfileComplete);

            activity?.SetTag("isProfileComplete", status.IsProfileComplete);
            activity?.SetTag("isAideProfileComplete", status.IsAideProfileComplete);

            return Results.Ok(status);
        })
        .WithName("GetProfileCompletionStatus")
        .WithTags("Profile")
        .Produces<ProfileCompletionStatusDto>(200)
        .Produces(401)
        .Produces(404)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get profile completion status";
            operation.Description = "Returns completion status for both mandatory and AIDE profile fields. " +
                                  "Checks all 13 mandatory fields (personal info, address, government ID, occupation).";
            return operation;
        });
        
        // T037: Retrieve full profile by user ID for editing
        group.MapGet("/{userId}", async (string userId, RegistrantDB db, ILogger<RegistrantDB> logger) =>
        {
            // Validate StrictId format - if not, try email lookup
            Id<Registrant>? parsedUserId = null;
            if (!Id<Registrant>.TryParse(userId, out var tmpId))
            {
                // Try email lookup (for external auth providers like Auth0)
                logger.LogInformation("Profile GET: userId {UserId} is not a StrictId, attempting email lookup", userId);
                var registrantByEmail = await db.Registrants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Email == userId);
                
                if (registrantByEmail is null)
                {
                    logger.LogWarning("Profile GET: No registrant found for userId/email {UserId}", userId);
                    return Results.NotFound();
                }
                
                parsedUserId = registrantByEmail.Id;
            }
            else
            {
                parsedUserId = tmpId;
            }
            
            // Defensive null check for compiler nullability analysis
            if (!parsedUserId.HasValue)
            {
                logger.LogError("Profile GET: parsedUserId is unexpectedly null");
                return Results.Problem("Internal error processing user ID");
            }

            var registrant = await db.Registrants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == parsedUserId.Value);
            
            if (registrant is null)
            {
                logger.LogWarning("Profile GET: Registrant not found for ID {UserId}", parsedUserId);
                return Results.NotFound();
            }
            
            logger.LogInformation("Profile GET: Found registrant {Id} for userId {UserId}", registrant.Id, userId);
            
            // T037: Return full Registrant for ProfileEdit component
            return Results.Ok(registrant);
        });
        
        // T037: Update full profile by user ID
        group.MapPut("/{userId}", async (string userId, RegistrantDB db, Registrant updatedRegistrant, ILogger<RegistrantDB> logger) =>
        {
            // Validate StrictId format - if not, try email lookup
            Id<Registrant>? parsedUserId = null;
            if (!Id<Registrant>.TryParse(userId, out var tmpId))
            {
                logger.LogInformation("Profile PUT: userId {UserId} is not a StrictId, attempting email lookup", userId);
                var registrantByEmail = await db.Registrants
                    .FirstOrDefaultAsync(r => r.Email == userId);
                
                if (registrantByEmail is null)
                {
                    logger.LogWarning("Profile PUT: No registrant found for userId/email {UserId}", userId);
                    return Results.NotFound();
                }
                
                parsedUserId = registrantByEmail.Id;
            }
            else
            {
                parsedUserId = tmpId;
            }
            
            // Defensive null check for compiler nullability analysis
            if (!parsedUserId.HasValue)
            {
                logger.LogError("Profile PUT: parsedUserId is unexpectedly null");
                return Results.Problem("Internal error processing user ID");
            }

            var registrant = await db.Registrants
                .FirstOrDefaultAsync(r => r.Id == parsedUserId.Value);
            
            if (registrant is null)
            {
                logger.LogWarning("Profile PUT: Registrant not found for ID {UserId}", parsedUserId);
                return Results.NotFound();
            }
            
            // T037: Update AIDE fields (allow editing)
            // We do NOT allow editing mandatory fields (FirstName, LastName, etc.)
            registrant.GenderIdentity = updatedRegistrant.GenderIdentity;
            registrant.SelfDescribeGender = updatedRegistrant.SelfDescribeGender;
            registrant.AgeRange = updatedRegistrant.AgeRange;
            registrant.Ethnicity = updatedRegistrant.Ethnicity;
            registrant.SelfDescribeEthnicity = updatedRegistrant.SelfDescribeEthnicity;
            registrant.LanguageProficiency = updatedRegistrant.LanguageProficiency;
            registrant.SelfDescribeLanguage = updatedRegistrant.SelfDescribeLanguage;
            registrant.EducationalBackground = updatedRegistrant.EducationalBackground;
            registrant.SelfDescribeEducation = updatedRegistrant.SelfDescribeEducation;
            registrant.Disability = updatedRegistrant.Disability;
            registrant.DisabilityDetails = updatedRegistrant.DisabilityDetails;
            registrant.DietaryRequirements = updatedRegistrant.DietaryRequirements;
            registrant.SelfDescribeDietary = updatedRegistrant.SelfDescribeDietary;
            registrant.LgbtqIdentity = updatedRegistrant.LgbtqIdentity;
            registrant.ParentalStatus = updatedRegistrant.ParentalStatus;
            registrant.FirstTimeAttendee = updatedRegistrant.FirstTimeAttendee;
            registrant.HowDidYouHear = updatedRegistrant.HowDidYouHear;
            registrant.SelfDescribeHowDidYouHear = updatedRegistrant.SelfDescribeHowDidYouHear;
            registrant.AreasOfInterest = updatedRegistrant.AreasOfInterest;
            registrant.SelfDescribeAreasOfInterest = updatedRegistrant.SelfDescribeAreasOfInterest;
            registrant.VolunteerOpportunities = updatedRegistrant.VolunteerOpportunities;
            registrant.AdditionalSupport = updatedRegistrant.AdditionalSupport;
            registrant.Religion = updatedRegistrant.Religion;
            registrant.Caste = updatedRegistrant.Caste;
            registrant.Neighborhood = updatedRegistrant.Neighborhood;
            registrant.ModeOfTransportation = updatedRegistrant.ModeOfTransportation;
            registrant.SocioeconomicBackground = updatedRegistrant.SocioeconomicBackground;
            registrant.Neurodiversity = updatedRegistrant.Neurodiversity;
            registrant.CaregivingResponsibilities = updatedRegistrant.CaregivingResponsibilities;
            
            // Also allow updating LinkedIn/GitHub
            registrant.LinkedInProfile = updatedRegistrant.LinkedInProfile;
            registrant.GitHubProfile = updatedRegistrant.GitHubProfile;
            
            // Check if AIDE profile is now complete
            // (This logic should match ProfileCompletionRepository.IsAideComplete)
            // Updated: Removed FirstTimeAttendee, AreasOfInterest, VolunteerOpportunities (event-management fields)
            // Added: SocioeconomicBackground, Neurodiversity, CaregivingResponsibilities (AIDE mandate fields)
            var isAideComplete = !string.IsNullOrWhiteSpace(registrant.GenderIdentity) &&
                                !string.IsNullOrWhiteSpace(registrant.AgeRange) &&
                                !string.IsNullOrWhiteSpace(registrant.Ethnicity) &&
                                !string.IsNullOrWhiteSpace(registrant.LanguageProficiency) &&
                                !string.IsNullOrWhiteSpace(registrant.EducationalBackground) &&
                                !string.IsNullOrWhiteSpace(registrant.Disability) &&
                                !string.IsNullOrWhiteSpace(registrant.DietaryRequirements) &&
                                !string.IsNullOrWhiteSpace(registrant.LgbtqIdentity) &&
                                !string.IsNullOrWhiteSpace(registrant.ParentalStatus) &&
                                !string.IsNullOrWhiteSpace(registrant.HowDidYouHear) &&
                                !string.IsNullOrWhiteSpace(registrant.Religion) &&
                                !string.IsNullOrWhiteSpace(registrant.Caste) &&
                                !string.IsNullOrWhiteSpace(registrant.Neighborhood) &&
                                !string.IsNullOrWhiteSpace(registrant.ModeOfTransportation) &&
                                !string.IsNullOrWhiteSpace(registrant.SocioeconomicBackground) &&
                                !string.IsNullOrWhiteSpace(registrant.Neurodiversity) &&
                                !string.IsNullOrWhiteSpace(registrant.CaregivingResponsibilities);
            
            registrant.IsAideProfileComplete = isAideComplete;
            if (isAideComplete && registrant.AideProfileCompletedAt == null)
            {
                registrant.AideProfileCompletedAt = DateTime.UtcNow;
                logger.LogInformation("AIDE profile completed for registrant {Id}", registrant.Id);
            }
            
            await db.SaveChangesAsync();
            
            logger.LogInformation("Profile updated for registrant {Id}", registrant.Id);
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

            Id<Registrant>? parsedUserId = null;
            if (!Id<Registrant>.TryParse(userId, out var tmpId))
            {
                var email = ResolveEmail(http.User);
                if (string.IsNullOrWhiteSpace(email))
                    return Results.NotFound();

                var registrantByEmail = await db.Registrants
                    .AsNoTracking()
                    .Where(r => r.Email == email)
                    .OrderByDescending(r => r.IsProfileComplete)
                    .ThenByDescending(r => r.ProfileCompletedAt ?? DateTime.MinValue)
                    .ThenByDescending(r => r.Id.Value)
                    .FirstOrDefaultAsync();
                if (registrantByEmail is null)
                    return Results.NotFound();

                parsedUserId = registrantByEmail.Id;
            }
            else
            {
                parsedUserId = tmpId;
            }

            // Defensive null check for compiler nullability analysis
            if (!parsedUserId.HasValue)
            {
                logger.LogError("Profile card GET: parsedUserId is unexpectedly null");
                return Results.Problem("Internal error processing user ID");
            }

            var registrant = await db.Registrants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == parsedUserId.Value);

            if (registrant is null)
                return Results.NotFound();

            return Results.Ok(new UserProfileDto
            {
                Name = registrant.FirstName + " " + registrant.LastName,
                Email = registrant.Email,
                LinkedIn = registrant.LinkedInProfile ?? string.Empty,
                GitHub = registrant.GitHubProfile ?? string.Empty,
                //RegistrationDate = registrant
            });
        });

        group.MapPut("/", async (HttpContext http, RegistrantDB db, UserProfileDto dto) =>
        {
            var userId = http.User.FindFirst("sub")?.Value;
            if (userId is null)
                return Results.Unauthorized();

            Id<Registrant>? parsedUserId = null;
            if (!Id<Registrant>.TryParse(userId, out var tmpId2))
            {
                var email = ResolveEmail(http.User);
                if (string.IsNullOrWhiteSpace(email))
                    return Results.NotFound();

                var registrantByEmail = await db.Registrants
                    .Where(r => r.Email == email)
                    .OrderByDescending(r => r.IsProfileComplete)
                    .ThenByDescending(r => r.ProfileCompletedAt ?? DateTime.MinValue)
                    .ThenByDescending(r => r.Id.Value)
                    .FirstOrDefaultAsync();
                if (registrantByEmail is null)
                    return Results.NotFound();

                parsedUserId = registrantByEmail.Id;
            }
            else
            {
                parsedUserId = tmpId2;
            }

            // Defensive null check for compiler nullability analysis
            if (!parsedUserId.HasValue)
            {
                return Results.Problem("Internal error processing user ID");
            }

            var registrant = await db.Registrants
                .FirstOrDefaultAsync(r => r.Id == parsedUserId.Value);

            if (registrant is null)
                return Results.NotFound();

            registrant.FirstName = dto.Name;
            registrant.LinkedInProfile = dto.LinkedIn;
            registrant.GitHubProfile = dto.GitHub;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // T026: POST /api/profile/preferences/aide-banner/dismiss - Dismiss AIDE banner for 30 days
        group.MapPost("/preferences/aide-banner/dismiss", async (
            HttpContext http,
            RegistrantDB db,
            UserPreferencesRepository preferencesRepo,
            ILogger<UserPreferencesRepository> logger) =>
        {
            var userId = http.User.FindFirst("sub")?.Value;

            if (userId is null)
            {
                logger.LogWarning("AIDE banner dismissal failed: No user ID in token");
                return Results.Unauthorized();
            }
            Id<Registrant>? parsedUserId = null;
            if (!Id<Registrant>.TryParse(userId, out var tmpId3))
            {
                var email = ResolveEmail(http.User);
                if (string.IsNullOrWhiteSpace(email))
                {
                    logger.LogWarning("AIDE banner dismissal failed: Invalid user ID format {UserId} and no email claim", userId);
                    return Results.NotFound(new { error = "User profile not found" });
                }

                var registrantByEmail = await db.Registrants.FirstOrDefaultAsync(r => r.Email == email);
                if (registrantByEmail is null)
                {
                    logger.LogWarning("AIDE banner dismissal failed: No registrant found for email {Email}", email);
                    return Results.NotFound(new { error = "User profile not found" });
                }

                parsedUserId = registrantByEmail.Id;
            }
            else
            {
                parsedUserId = tmpId3;
            }

            // Defensive null check for compiler nullability analysis
            if (!parsedUserId.HasValue)
            {
                logger.LogError("AIDE banner dismissal: parsedUserId is unexpectedly null");
                return Results.Problem("Internal error processing user ID");
            }

            await preferencesRepo.DismissAideBannerAsync(parsedUserId.Value);

            logger.LogInformation("AIDE banner dismissed for user {UserId}", userId);
            
            return Results.Ok(new { message = "AIDE banner dismissed successfully" });
        })
        .WithName("DismissAideBanner")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Dismiss AIDE completion banner";
            operation.Description = "Dismisses the AIDE profile completion banner for 30 days. " +
                "The banner will reappear after 30 days if the AIDE profile is still incomplete.";
            return operation;
        });

        // T027: GET /api/profile/preferences/aide-banner - Get AIDE banner status
        group.MapGet("/preferences/aide-banner", async (
            HttpContext http,
            RegistrantDB db,
            UserPreferencesRepository preferencesRepo,
            ILogger<UserPreferencesRepository> logger) =>
        {
            var userId = http.User.FindFirst("sub")?.Value;

            if (userId is null)
            {
                logger.LogWarning("AIDE banner status check failed: No user ID in token");
                return Results.Unauthorized();
            }
            Id<Registrant>? parsedUserId = null;
            if (!Id<Registrant>.TryParse(userId, out var tmpId4))
            {
                var email = ResolveEmail(http.User);
                if (string.IsNullOrWhiteSpace(email))
                {
                    logger.LogWarning("AIDE banner status check failed: Invalid user ID format {UserId} and no email claim", userId);
                    return Results.NotFound(new { error = "User profile not found" });
                }

                var registrantByEmail = await db.Registrants
                    .AsNoTracking()
                    .Where(r => r.Email == email)
                    .OrderByDescending(r => r.IsProfileComplete)
                    .ThenByDescending(r => r.ProfileCompletedAt ?? DateTime.MinValue)
                    .ThenByDescending(r => r.Id.Value)
                    .FirstOrDefaultAsync();
                if (registrantByEmail is null)
                {
                    logger.LogWarning("AIDE banner status check failed: No registrant found for email {Email}", email);
                    return Results.NotFound(new { error = "User profile not found" });
                }

                parsedUserId = registrantByEmail.Id;
            }
            else
            {
                parsedUserId = tmpId4;
            }

            // Defensive null check for compiler nullability analysis
            if (!parsedUserId.HasValue)
            {
                logger.LogError("AIDE banner status check: parsedUserId is unexpectedly null");
                return Results.Problem("Internal error processing user ID");
            }

            var preferences = await preferencesRepo.GetByUserIdAsync(parsedUserId.Value);

            var dto = new UserPreferencesDto
            {
                UserId = parsedUserId.Value.ToString(),
                AideBannerDismissedAt = preferences?.AideBannerDismissedAt,
                UpdatedAt = preferences?.UpdatedAt ?? DateTime.UtcNow
            };

            logger.LogInformation("AIDE banner status retrieved for user {UserId}", userId);
            
            return Results.Ok(dto);
        })
        .WithName("GetAideBannerStatus")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get AIDE banner status";
            operation.Description = "Retrieves the user's AIDE banner dismissal status. " +
                "Returns null for AideBannerDismissedAt if never dismissed, or the dismissal timestamp.";
            return operation;
        });

        // T041: POST /api/profile/draft - Save registration draft
        group.MapPost("/draft", async (
            HttpContext http,
            RegistrantDB db,
            RegistrationDraftDto draftDto,
            ILogger<RegistrantDB> logger) =>
        {
            // Try to get user ID from sub claim, fallback to email
            var userId = http.User.FindFirst("sub")?.Value;
            Id<Registrant>? parsedUserId = null;

            if (!string.IsNullOrWhiteSpace(userId) && Id<Registrant>.TryParse(userId, out var tmpId))
            {
                parsedUserId = tmpId;
            }
            else
            {
                // Fallback to email-based lookup
                var email = ResolveEmail(http.User);
                if (string.IsNullOrWhiteSpace(email))
                {
                    logger.LogWarning("Draft save failed: No user ID in token and no email claim");
                    return Results.Unauthorized();
                }

                var registrantByEmail = await db.Registrants.FirstOrDefaultAsync(r => r.Email == email);
                if (registrantByEmail is null)
                {
                    logger.LogWarning("Draft save failed: No registrant found for email {Email}", email);
                    return Results.NotFound(new { error = "User profile not found" });
                }

                parsedUserId = registrantByEmail.Id;
                logger.LogInformation("Draft save: Resolved user ID {UserId} from email {Email}", parsedUserId, email);
            }

            // T042: Validate section field
            if (draftDto.Section != "mandatory" && draftDto.Section != "aide")
            {
                logger.LogWarning("Draft save failed: Invalid section {Section}", draftDto.Section);
                return Results.BadRequest(new { error = "Section must be 'mandatory' or 'aide'" });
            }

            // Defensive null check for compiler nullability analysis
            if (!parsedUserId.HasValue)
            {
                logger.LogError("Draft save: parsedUserId is unexpectedly null");
                return Results.Problem("Internal error processing user ID");
            }

            var now = DateTime.UtcNow;
            
            // T043: Check for existing draft for this user + section
            var existingDraft = await db.DraftRegistrations
                .FirstOrDefaultAsync(d => d.UserId == parsedUserId.Value && d.Section == draftDto.Section);

            if (existingDraft is not null)
            {
                // T044: Update existing draft
                existingDraft.DraftData = draftDto.DraftData;
                existingDraft.UpdatedAt = now;
                existingDraft.ExpiresAt = now.AddDays(30); // FR-008: 30-day TTL
                existingDraft.DataHash = draftDto.DataHash;
                existingDraft.IsApplied = false; // Reset if user is editing again

                logger.LogInformation("Updated draft for user {UserId}, section {Section}", parsedUserId, draftDto.Section);
            }
            else
            {
                // T045: Create new draft
                var newDraft = new DraftRegistration
                {
                    UserId = parsedUserId.Value,
                    Section = draftDto.Section,
                    DraftData = draftDto.DraftData,
                    CreatedAt = now,
                    UpdatedAt = now,
                    ExpiresAt = now.AddDays(30), // FR-008: 30-day TTL
                    DataHash = draftDto.DataHash,
                    IsApplied = false
                };

                db.DraftRegistrations.Add(newDraft);
                logger.LogInformation("Created new draft for user {UserId}, section {Section}", parsedUserId, draftDto.Section);
            }

            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Draft saved successfully", section = draftDto.Section });
        })
        .WithName("SaveDraft")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Save registration draft";
            operation.Description = "Saves partial registration progress as a draft with 30-day expiration. " +
                "Supports both 'mandatory' and 'aide' sections. Auto-saves every 30 seconds per FR-008.";
            return operation;
        });

        // T046: GET /api/profile/draft/{section} - Retrieve registration draft
        group.MapGet("/draft/{section}", async (
            string section,
            HttpContext http,
            RegistrantDB db,
            ILogger<RegistrantDB> logger) =>
        {
            // Try to get user ID from sub claim, fallback to email
            var userId = http.User.FindFirst("sub")?.Value;
            Id<Registrant>? parsedUserId = null;

            if (!string.IsNullOrWhiteSpace(userId) && Id<Registrant>.TryParse(userId, out var tmpId))
            {
                parsedUserId = tmpId;
            }
            else
            {
                // Fallback to email-based lookup
                var email = ResolveEmail(http.User);
                if (string.IsNullOrWhiteSpace(email))
                {
                    logger.LogWarning("Draft retrieval failed: No user ID in token and no email claim");
                    return Results.Unauthorized();
                }

                var registrantByEmail = await db.Registrants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Email == email);
                if (registrantByEmail is null)
                {
                    logger.LogWarning("Draft retrieval failed: No registrant found for email {Email}", email);
                    return Results.NotFound(new { error = "User profile not found" });
                }

                parsedUserId = registrantByEmail.Id;
                logger.LogInformation("Draft retrieval: Resolved user ID {UserId} from email {Email}", parsedUserId, email);
            }

            // Defensive null check for compiler nullability analysis
            if (!parsedUserId.HasValue)
            {
                logger.LogError("Draft retrieval: parsedUserId is unexpectedly null");
                return Results.Problem("Internal error processing user ID");
            }

            // T047: Validate section parameter
            if (section != "mandatory" && section != "aide")
            {
                logger.LogWarning("Draft retrieval failed: Invalid section {Section}", section);
                return Results.BadRequest(new { error = "Section must be 'mandatory' or 'aide'" });
            }

            var now = DateTime.UtcNow;
            
            // T048: Retrieve draft and check expiration
            var draft = await db.DraftRegistrations
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == parsedUserId.Value && d.Section == section);

            if (draft is null)
            {
                logger.LogInformation("No draft found for user {UserId}, section {Section}", parsedUserId, section);
                return Results.NotFound(new { error = "No draft found" });
            }

            // T049: Check if draft expired or was applied
            if (draft.ExpiresAt < now)
            {
                logger.LogInformation("Draft expired for user {UserId}, section {Section}. Expired at {ExpiresAt}", 
                    parsedUserId, section, draft.ExpiresAt);
                return Results.NotFound(new { error = "Draft has expired" });
            }

            if (draft.IsApplied)
            {
                logger.LogInformation("Draft already applied for user {UserId}, section {Section}", parsedUserId, section);
                return Results.NotFound(new { error = "Draft has been applied" });
            }

            logger.LogInformation("Retrieved draft for user {UserId}, section {Section}", parsedUserId, section);

            var response = new RegistrationDraftDto
            {
                UserId = parsedUserId.Value.ToString(),
                DraftData = draft.DraftData,
                Section = draft.Section,
                SavedAt = draft.UpdatedAt,
                ExpiresAt = draft.ExpiresAt,
                DataHash = draft.DataHash
            };

            return Results.Ok(response);
        })
        .WithName("GetDraft")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Retrieve registration draft";
            operation.Description = "Retrieves saved draft for the specified section ('mandatory' or 'aide'). " +
                "Returns 404 if no draft exists, expired, or already applied.";
            return operation;
        });

        // T050: DELETE /api/profile/draft/{section} - Delete registration draft
        group.MapDelete("/draft/{section}", async (
            string section,
            HttpContext http,
            RegistrantDB db,
            ILogger<RegistrantDB> logger) =>
        {
            // Try to get user ID from sub claim, fallback to email
            var userId = http.User.FindFirst("sub")?.Value;
            Id<Registrant>? parsedUserId = null;

            if (!string.IsNullOrWhiteSpace(userId) && Id<Registrant>.TryParse(userId, out var tmpId))
            {
                parsedUserId = tmpId;
            }
            else
            {
                // Fallback to email-based lookup
                var email = ResolveEmail(http.User);
                if (string.IsNullOrWhiteSpace(email))
                {
                    logger.LogWarning("Draft deletion failed: No user ID in token and no email claim");
                    return Results.Unauthorized();
                }

                var registrantByEmail = await db.Registrants.FirstOrDefaultAsync(r => r.Email == email);
                if (registrantByEmail is null)
                {
                    logger.LogWarning("Draft deletion failed: No registrant found for email {Email}", email);
                    return Results.NotFound(new { error = "User profile not found" });
                }

                parsedUserId = registrantByEmail.Id;
                logger.LogInformation("Draft deletion: Resolved user ID {UserId} from email {Email}", parsedUserId, email);
            }

            // Defensive null check for compiler nullability analysis
            if (!parsedUserId.HasValue)
            {
                logger.LogError("Draft deletion: parsedUserId is unexpectedly null");
                return Results.Problem("Internal error processing user ID");
            }

            // Validate section parameter
            if (section != "mandatory" && section != "aide")
            {
                logger.LogWarning("Draft deletion failed: Invalid section {Section}", section);
                return Results.BadRequest(new { error = "Section must be 'mandatory' or 'aide'" });
            }

            var draft = await db.DraftRegistrations
                .FirstOrDefaultAsync(d => d.UserId == parsedUserId.Value && d.Section == section);

            if (draft is null)
            {
                logger.LogInformation("No draft to delete for user {UserId}, section {Section}", parsedUserId, section);
                return Results.NotFound(new { error = "No draft found" });
            }

            db.DraftRegistrations.Remove(draft);
            await db.SaveChangesAsync();

            logger.LogInformation("Deleted draft for user {UserId}, section {Section}", parsedUserId, section);

            return Results.NoContent();
        })
        .WithName("DeleteDraft")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Delete registration draft";
            operation.Description = "Deletes saved draft for the specified section ('mandatory' or 'aide'). " +
                "Called when user completes registration or explicitly clears draft.";
            return operation;
        });

        // T087: POST /api/profile/social/link-callback - Handle OAuth callback for social connections
        group.MapPost("/social/link-callback", async (
            HttpContext http,
            RegistrantDB db,
            SocialProfileLinkDto linkDto,
            ILogger<RegistrantDB> logger) =>
        {
            if (string.IsNullOrWhiteSpace(linkDto.Provider) || string.IsNullOrWhiteSpace(linkDto.ProfileUrl))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid request",
                    detail: "Provider and profileUrl are required.");
            }

            var normalizedProvider = linkDto.Provider.Trim().ToLowerInvariant();
            var normalizedProfileUrl = linkDto.ProfileUrl.Trim();
            var now = DateTime.UtcNow;

            var registrant = await ResolveRegistrantForCurrentUserAsync(http, db, logger, asTracking: true);
            if (registrant is null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Registrant profile not found");
            }

            // Defensive: record attempt (succeeds or fails)
            db.SocialVerificationEvents.Add(new SocialVerificationEvent
            {
                RegistrantId = registrant.Id,
                Provider = normalizedProvider,
                Action = "link_attempt",
                ProfileUrl = normalizedProfileUrl,
                OccurredAtUtc = now,
                Outcome = "attempted"
            });

            // Validate provider and enforce uniqueness (409) before attempting to update.
            if (normalizedProvider is not ("linkedin" or "github"))
            {
                db.SocialVerificationEvents.Add(new SocialVerificationEvent
                {
                    RegistrantId = registrant.Id,
                    Provider = normalizedProvider,
                    Action = "link_failed",
                    ProfileUrl = normalizedProfileUrl,
                    OccurredAtUtc = now,
                    Outcome = "failed",
                    FailureReason = "invalid_provider"
                });

                await db.SaveChangesAsync();
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid provider",
                    detail: $"Invalid provider: {linkDto.Provider}");
            }

            if (normalizedProvider == "linkedin")
            {
                var isConflict = await db.Registrants
                    .AsNoTracking()
                    .AnyAsync(r => r.IsLinkedInVerified
                                   && r.LinkedInProfile == normalizedProfileUrl
                                   && r.Id != registrant.Id);
                if (isConflict)
                {
                    db.SocialVerificationEvents.Add(new SocialVerificationEvent
                    {
                        RegistrantId = registrant.Id,
                        Provider = normalizedProvider,
                        Action = "link_failed",
                        ProfileUrl = normalizedProfileUrl,
                        OccurredAtUtc = now,
                        Outcome = "failed",
                        FailureReason = "conflict"
                    });

                    await db.SaveChangesAsync();
                    return Results.Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        type: "https://visage.app/problems/social-profile-conflict",
                        title: "Social profile already linked",
                        detail: "This LinkedIn/GitHub account is already verified for another registrant.");
                }

                registrant.LinkedInProfile = normalizedProfileUrl;
                registrant.IsLinkedInVerified = true;
                registrant.LinkedInVerifiedAt = now;
            }
            else
            {
                var isConflict = await db.Registrants
                    .AsNoTracking()
                    .AnyAsync(r => r.IsGitHubVerified
                                   && r.GitHubProfile == normalizedProfileUrl
                                   && r.Id != registrant.Id);
                if (isConflict)
                {
                    db.SocialVerificationEvents.Add(new SocialVerificationEvent
                    {
                        RegistrantId = registrant.Id,
                        Provider = normalizedProvider,
                        Action = "link_failed",
                        ProfileUrl = normalizedProfileUrl,
                        OccurredAtUtc = now,
                        Outcome = "failed",
                        FailureReason = "conflict"
                    });

                    await db.SaveChangesAsync();
                    return Results.Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        type: "https://visage.app/problems/social-profile-conflict",
                        title: "Social profile already linked",
                        detail: "This LinkedIn/GitHub account is already verified for another registrant.");
                }

                registrant.GitHubProfile = normalizedProfileUrl;
                registrant.IsGitHubVerified = true;
                registrant.GitHubVerifiedAt = now;
            }

            db.SocialVerificationEvents.Add(new SocialVerificationEvent
            {
                RegistrantId = registrant.Id,
                Provider = normalizedProvider,
                Action = "link_succeeded",
                ProfileUrl = normalizedProfileUrl,
                OccurredAtUtc = now,
                Outcome = "succeeded"
            });

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Check if this is specifically a uniqueness violation
                var isUniqueConstraintViolation = ex.InnerException?.Message?.Contains("IX_Registrants_LinkedInSubject") == true
                    || ex.InnerException?.Message?.Contains("IX_Registrants_LinkedInProfile") == true
                    || ex.InnerException?.Message?.Contains("IX_Registrants_GitHubProfile") == true;
                
                if (!isUniqueConstraintViolation)
                {
                    logger.LogError(ex, "Unexpected database error during social link");
                    throw; // Re-throw non-conflict errors
                }
                
                logger.LogWarning(ex, "Social link callback failed due to uniqueness constraint");

                db.ChangeTracker.Clear();
                db.SocialVerificationEvents.Add(new SocialVerificationEvent
                {
                    RegistrantId = registrant.Id,
                    Provider = normalizedProvider,
                    Action = "link_failed",
                    ProfileUrl = normalizedProfileUrl,
                    OccurredAtUtc = DateTime.UtcNow,
                    Outcome = "failed",
                    FailureReason = "conflict"
                });
                await db.SaveChangesAsync();

                return Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    type: "https://visage.app/problems/social-profile-conflict",
                    title: "Social profile already linked",
                    detail: "This LinkedIn/GitHub account is already verified for another registrant.");
            }

            var sanitizedProviderForLog = normalizedProvider
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

            logger.LogInformation("Verified {Provider} profile for user {UserId}: {ProfileUrl}", sanitizedProviderForLog, registrant.Id, normalizedProfileUrl);

            return Results.Ok(new
            {
                message = $"{normalizedProvider} profile linked successfully",
                profileUrl = normalizedProfileUrl,
                verifiedAt = now
            });
        })
        .WithName("LinkSocialProfile")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Link OAuth-verified social profile";
            operation.Description = "Stores OAuth-verified LinkedIn or GitHub profile URL after successful authentication. " +
                "This endpoint is called by the frontend after successful Auth0 social connection. " +
                "Only accepts 'linkedin' or 'github' as provider values.";
            return operation;
        });

        // T088: GET /api/profile/social/status - Get social connection status
        group.MapGet("/social/status", async (
            HttpContext http,
            RegistrantDB db,
            ILogger<RegistrantDB> logger) =>
        {
            var registrant = await ResolveRegistrantForCurrentUserAsync(http, db, logger, asTracking: false);
            if (registrant is null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Registrant profile not found");
            }

            var status = new SocialConnectionStatusDto
            {
                LinkedIn = new SocialProviderStatusDto
                {
                    IsConnected = registrant.IsLinkedInVerified,
                    ProfileUrl = registrant.LinkedInProfile,
                    VerifiedAt = registrant.LinkedInVerifiedAt
                },
                GitHub = new SocialProviderStatusDto
                {
                    IsConnected = registrant.IsGitHubVerified,
                    ProfileUrl = registrant.GitHubProfile,
                    VerifiedAt = registrant.GitHubVerifiedAt
                }
            };

            logger.LogInformation("Retrieved social status for user {UserId}", registrant.Id);
            return Results.Ok(status);
        })
        .WithName("GetSocialConnectionStatus")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get social connection status";
            operation.Description = "Retrieves the verification status and profile URLs for LinkedIn and GitHub connections.";
            return operation;
        });

        // T090: POST /api/profile/social/disconnect - Disconnect a verified social profile
        group.MapPost("/social/disconnect", async (
            HttpContext http,
            RegistrantDB db,
            SocialDisconnectDto dto,
            ILogger<RegistrantDB> logger) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Provider))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid request",
                    detail: "Provider is required.");
            }

            var provider = dto.Provider.Trim().ToLowerInvariant();
            var now = DateTime.UtcNow;

            var registrant = await ResolveRegistrantForCurrentUserAsync(http, db, logger, asTracking: true);
            if (registrant is null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Registrant profile not found");
            }

            switch (provider)
            {
                case "linkedin":
                    registrant.LinkedInProfile = null;
                    registrant.IsLinkedInVerified = false;
                    registrant.LinkedInVerifiedAt = null;
                    break;
                case "github":
                    registrant.GitHubProfile = null;
                    registrant.IsGitHubVerified = false;
                    registrant.GitHubVerifiedAt = null;
                    break;
                default:
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid provider",
                        detail: $"Invalid provider: {dto.Provider}");
            }

            db.SocialVerificationEvents.Add(new SocialVerificationEvent
            {
                RegistrantId = registrant.Id,
                Provider = provider,
                Action = "disconnect",
                OccurredAtUtc = now,
                Outcome = "succeeded"
            });

            await db.SaveChangesAsync();

            var status = new SocialConnectionStatusDto
            {
                LinkedIn = new SocialProviderStatusDto
                {
                    IsConnected = registrant.IsLinkedInVerified,
                    ProfileUrl = registrant.LinkedInProfile,
                    VerifiedAt = registrant.LinkedInVerifiedAt
                },
                GitHub = new SocialProviderStatusDto
                {
                    IsConnected = registrant.IsGitHubVerified,
                    ProfileUrl = registrant.GitHubProfile,
                    VerifiedAt = registrant.GitHubVerifiedAt
                }
            };

            return Results.Ok(status);
        })
        .WithName("DisconnectSocialProfile")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Disconnect verified social profile";
            operation.Description = "Clears stored profile URL and verification flags for the specified provider.";
            return operation;
        });
    }

    private static async Task<Registrant?> ResolveRegistrantForCurrentUserAsync(
        HttpContext http,
        RegistrantDB db,
        ILogger logger,
        bool asTracking)
    {
        var userId = http.User.FindFirst("sub")?.Value;

        // Prefer internal StrictId when present.
        if (!string.IsNullOrWhiteSpace(userId) && Id<Registrant>.TryParse(userId, out var strictId))
        {
            var byIdQuery = asTracking ? db.Registrants : db.Registrants.AsNoTracking();
            return await byIdQuery.FirstOrDefaultAsync(r => r.Id == strictId);
        }

        var email = ResolveEmail(http.User);
        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning("Registrant resolution failed: no StrictId in sub and no email claim.");
            return null;
        }

        var query = asTracking ? db.Registrants : db.Registrants.AsNoTracking();

        // Prefer the most recently completed profile when duplicates exist.
        return await query
            .Where(r => r.Email == email)
            .OrderByDescending(r => r.IsProfileComplete)
            .ThenByDescending(r => r.ProfileCompletedAt.HasValue)
            .ThenByDescending(r => r.ProfileCompletedAt)
            .FirstOrDefaultAsync();
    }

    private static string? ResolveEmail(ClaimsPrincipal user)
    {
        return EmailClaimTypes
            .Select(claimType => user.FindFirst(claimType)?.Value)
            .FirstOrDefault(claimValue => !string.IsNullOrWhiteSpace(claimValue));
    }
}
