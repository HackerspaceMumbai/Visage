using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Visage.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Visage.Services.Eventing;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Use SQL Server with Aspire-provided connection string (replaces in-memory DB)
builder.AddSqlServerDbContext<EventDB>("eventingdb");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();



// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventDB>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply database migrations");
        throw;
    }
    
    // Seed sample data in development if database is empty
    if (app.Environment.IsDevelopment() && !await db.Events.AnyAsync())
    {
        var now = DateTime.UtcNow;
        var sampleEvents = new[]
        {
            new Event
            {
                Title = "Open Source Saturday",
                Description = "Join us for a day of open source contributions, learning, and collaboration!",
                StartDate = DateOnly.FromDateTime(now.AddDays(7).Date),
                StartTime = new TimeOnly(10, 0),
                EndDate = DateOnly.FromDateTime(now.AddDays(7).Date),
                EndTime = new TimeOnly(17, 0),
                Location = "Hackerspace Mumbai",
                Type = "Workshop",
                Theme = "Open Source",
                Hashtag = "OSSaturday",
                CoverPicture = "https://res.cloudinary.com/demo/image/upload/v1312461204/sample.jpg"
            },
            new Event
            {
                Title = "Docker & Kubernetes Workshop",
                Description = "Hands-on workshop covering containerization with Docker and orchestration with Kubernetes.",
                StartDate = DateOnly.FromDateTime(now.AddDays(14).Date),
                StartTime = new TimeOnly(14, 0),
                EndDate = DateOnly.FromDateTime(now.AddDays(14).Date),
                EndTime = new TimeOnly(18, 0),
                Location = "Mumbai Tech Hub",
                Type = "Workshop",
                Theme = "DevOps",
                Hashtag = "K8sWorkshop",
                CoverPicture = "https://res.cloudinary.com/demo/image/upload/v1652345874/docs/models.jpg"
            },
            new Event
            {
                Title = "AI & Machine Learning Meetup",
                Description = "Exploring the latest in AI and ML with local experts and practitioners.",
                StartDate = DateOnly.FromDateTime(now.AddDays(21).Date),
                StartTime = new TimeOnly(18, 30),
                EndDate = DateOnly.FromDateTime(now.AddDays(21).Date),
                EndTime = new TimeOnly(21, 0),
                Location = "Colaba Library",
                Type = "Meetup",
                Theme = "AI/ML",
                Hashtag = "AIMeetup",
                CoverPicture = "https://res.cloudinary.com/demo/image/upload/v1652366604/docs/colored_pencils.jpg"
            },
            new Event
            {
                Title = "Web Development Bootcamp",
                Description = "Intensive bootcamp covering modern web development with .NET and Blazor.",
                StartDate = DateOnly.FromDateTime(now.AddDays(-30).Date),
                StartTime = new TimeOnly(9, 0),
                EndDate = DateOnly.FromDateTime(now.AddDays(-28).Date),
                EndTime = new TimeOnly(18, 0),
                Location = "Hackerspace Mumbai",
                Type = "Bootcamp",
                Theme = "Web Development",
                Hashtag = "WebBootcamp",
                CoverPicture = "https://res.cloudinary.com/demo/image/upload/v1652345767/docs/couple.jpg"
            }
        };
        
        db.Events.AddRange(sampleEvents);
        await db.SaveChangesAsync();
        
        logger.LogInformation("Seeded {Count} sample events for development", sampleEvents.Length);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
            {
                options.WithTitle("Visage Event API")
                       .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            }
    );
}

app.UseHttpsRedirection();

var events = app.MapGroup("/events");

events.MapGet("/", GetAllEvents);
events.MapGet("/upcoming", GetUpcomingEvents)
    .WithName("Get Upcoming Events")
    .WithOpenApi();
events.MapGet("/past", GetPastEvents)
    .WithName("Get Past Events")
    .WithOpenApi();

events.MapPost("/", ScheduleEvent)
    .WithName("Schedule Event")
    .WithOpenApi();

// Event Registration endpoints
var registrations = app.MapGroup("/registrations");

registrations.MapPost("/", RegisterForEvent)
    .WithName("Register for Event")
    .WithOpenApi();

registrations.MapGet("/my", GetMyRegistrations)
    .WithName("Get My Registrations")
    .WithOpenApi();

registrations.MapPost("/{id}/approve", ApproveRegistration)
    .WithName("Approve Registration")
    .WithOpenApi();

// Check-in/Check-out endpoints (optimized for door speed)
var checkin = app.MapGroup("/checkin");

checkin.MapPost("/", CheckInToSession)
    .WithName("Check-in to Session")
    .WithOpenApi();

checkin.MapPost("/checkout", CheckOutFromSession)
    .WithName("Check-out from Session")
    .WithOpenApi();

checkin.MapGet("/pin/{pin}", LookupByPin)
    .WithName("Lookup Registration by PIN")
    .WithOpenApi();

app.MapDefaultEndpoints();

app.Run();


static async Task<IResult> GetAllEvents(EventDB db)
{
    return TypedResults.Ok(await db.Events.ToListAsync());
}

static async Task<IResult> GetUpcomingEvents(EventDB db)
{
    var now = DateTime.UtcNow;
    var today = DateOnly.FromDateTime(now);
    var currentTime = TimeOnly.FromDateTime(now);
    
    var upcoming = await db.Events
        .Where(e => e.StartDate > today || (e.StartDate == today && e.StartTime >= currentTime))
        .OrderBy(e => e.StartDate)
        .ThenBy(e => e.StartTime)
        .ToListAsync();
    return TypedResults.Ok(upcoming);
}

static async Task<IResult> GetPastEvents(EventDB db)
{
    var now = DateTime.UtcNow;
    var today = DateOnly.FromDateTime(now);
    var currentTime = TimeOnly.FromDateTime(now);
    
    var past = await db.Events
        .Where(e => e.EndDate < today || (e.EndDate == today && e.EndTime < currentTime))
        .OrderByDescending(e => e.StartDate)
        .ThenByDescending(e => e.StartTime)
        .ToListAsync();
    return TypedResults.Ok(past);
}

static async Task<Results<BadRequest<String>, Created<Event>>> ScheduleEvent([FromBody]Event EventDetails, EventDB db)
{
    try
    {
        db.Events.Add(EventDetails);
        await db.SaveChangesAsync();

        // Assuming the Event object has an Id property that is set upon saving (e.g., auto-incremented by the database)
        // Construct the URI where the newly created event can be accessed
        // For example, if you have an endpoint to get an event by its Id
        string newEventUri = $"/events/{EventDetails.Id}";

        // Return a Created result with the URI and the event object
        return TypedResults.Created(newEventUri, EventDetails);
    }
    catch (Exception ex)
    {
        // Do not expose exception details to prevent information disclosure
        return TypedResults.BadRequest("Failed to schedule event. Please check your input and try again.");
    }
}

// Event Registration handlers

static async Task<Results<BadRequest<string>, Created<EventRegistration>>> RegisterForEvent(
    [FromBody] RegisterEventRequest request, 
    EventDB db, 
    HttpContext http)
{
    var auth0Sub = http.User.FindFirst("sub")?.Value;
    if (string.IsNullOrEmpty(auth0Sub))
    {
        return TypedResults.BadRequest("Unauthorized: Auth0 subject not found");
    }

    // Check if event exists
    var eventExists = await db.Events.AnyAsync(e => e.Id == request.EventId);
    if (!eventExists)
    {
        return TypedResults.BadRequest($"Event {request.EventId} not found");
    }

    // Check if user already registered for this event
    var existingReg = await db.EventRegistrations
        .FirstOrDefaultAsync(r => r.EventId == request.EventId && r.Auth0Subject == auth0Sub);
    
    if (existingReg != null)
    {
        return TypedResults.BadRequest("You are already registered for this event");
    }

    var registration = new EventRegistration
    {
        EventId = request.EventId,
        Auth0Subject = auth0Sub,
        Status = RegistrationStatus.Pending,
        AdditionalDetails = request.AdditionalDetails,
        RegisteredAt = DateTime.UtcNow
    };

    db.EventRegistrations.Add(registration);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/registrations/{registration.Id}", registration);
}

static async Task<Ok<List<EventRegistration>>> GetMyRegistrations(EventDB db, HttpContext http)
{
    var auth0Sub = http.User.FindFirst("sub")?.Value;
    if (string.IsNullOrEmpty(auth0Sub))
    {
        return TypedResults.Ok(new List<EventRegistration>());
    }

    var registrations = await db.EventRegistrations
        .Where(r => r.Auth0Subject == auth0Sub)
        .OrderByDescending(r => r.RegisteredAt)
        .ToListAsync();

    return TypedResults.Ok(registrations);
}

static async Task<Results<NotFound, ForbidHttpResult, BadRequest<string>, Ok<EventRegistration>>> ApproveRegistration(
    StrictId.Id<EventRegistration> id, 
    EventDB db, 
    HttpContext http)
{
    // Verify approver has admin privileges
    if (!http.User.IsInRole("VisageAdmin"))
    {
        return TypedResults.Forbid();
    }

    var registration = await db.EventRegistrations.FindAsync(id);
    if (registration == null)
    {
        return TypedResults.NotFound();
    }

    if (registration.Status != RegistrationStatus.Pending)
    {
        return TypedResults.BadRequest($"Registration is already {registration.Status}");
    }

    var approverSub = http.User.FindFirst("sub")?.Value;
    
    registration.Status = RegistrationStatus.Approved;
    registration.ApprovedAt = DateTime.UtcNow;
    registration.ApprovedBy = approverSub;
    registration.CheckInPin = GenerateCheckInPin();

    await db.SaveChangesAsync();

    return TypedResults.Ok(registration);
}

// Check-in handlers (optimized for speed)

static async Task<Results<BadRequest<string>, Ok<CheckInResponse>>> CheckInToSession(
    [FromBody] CheckInRequest request, 
    EventDB db, 
    HttpContext http)
{
    var auth0Sub = http.User.FindFirst("sub")?.Value;
    if (string.IsNullOrEmpty(auth0Sub))
    {
        return TypedResults.BadRequest("Unauthorized: Auth0 subject not found");
    }

    // Fast lookup with composite index: EventId + Auth0Subject
    var registration = await db.EventRegistrations
        .AsNoTracking()
        .FirstOrDefaultAsync(r => 
            r.EventId == request.EventId && 
            r.Auth0Subject == auth0Sub &&
            r.Status == RegistrationStatus.Approved);

    if (registration == null)
    {
        return TypedResults.BadRequest("No approved registration found for this event");
    }

    // Check if already checked in to this session
    var existingCheckIn = await db.SessionCheckIns
        .FirstOrDefaultAsync(c => 
            c.EventRegistrationId == registration.Id && 
            c.SessionId == request.SessionId &&
            c.CheckedOutAt == null);

    if (existingCheckIn != null)
    {
        return TypedResults.BadRequest($"Already checked in to session {request.SessionId}");
    }

    var checkIn = new SessionCheckIn
    {
        EventRegistrationId = registration.Id,
        SessionId = request.SessionId,
        CheckedInAt = DateTime.UtcNow,
        CheckInMethod = "JWT"
    };

    db.SessionCheckIns.Add(checkIn);
    await db.SaveChangesAsync();

    return TypedResults.Ok(new CheckInResponse(
        checkIn.Id,
        registration.Id,
        request.SessionId,
        checkIn.CheckedInAt,
        "Checked in successfully"));
}

static async Task<Results<BadRequest<string>, Ok<CheckOutResponse>>> CheckOutFromSession(
    [FromBody] CheckOutRequest request,
    EventDB db)
{
    var checkIn = await db.SessionCheckIns.FindAsync(request.CheckInId);
    if (checkIn == null)
    {
        return TypedResults.BadRequest("Check-in record not found");
    }

    if (checkIn.CheckedOutAt != null)
    {
        return TypedResults.BadRequest("Already checked out");
    }

    var now = DateTime.UtcNow;
    checkIn.CheckedOutAt = now;
    checkIn.StayDurationMinutes = (int)(now - checkIn.CheckedInAt).TotalMinutes;

    await db.SaveChangesAsync();

    return TypedResults.Ok(new CheckOutResponse(
        checkIn.Id,
        now,
        checkIn.StayDurationMinutes ?? 0,
        "Checked out successfully"));
}

static async Task<Results<BadRequest<string>, NotFound, Ok<EventRegistration>>> LookupByPin(
    string pin, 
    EventDB db,
    HttpContext http)
{
    // Require authentication
    var auth0Sub = http.User.FindFirst("sub")?.Value;
    if (string.IsNullOrEmpty(auth0Sub))
    {
        return TypedResults.BadRequest("Authentication required");
    }

    // Fast lookup using CheckInPin index
    var registration = await db.EventRegistrations
        .AsNoTracking()
        .FirstOrDefaultAsync(r => r.CheckInPin == pin);

    if (registration == null)
    {
        return TypedResults.NotFound();
    }

    return TypedResults.Ok(registration);
}

// Helper methods

static string GenerateCheckInPin()
{
    // Generates cryptographically secure 4-digit PIN (1000-9999)
    // Upper bound 10000 is exclusive, so range is [1000, 10000) = [1000, 9999]
    return RandomNumberGenerator.GetInt32(1000, 10000).ToString();
}

// Request/Response DTOs

// Use shared contracts so frontend and tests can reuse models.
// (Defined in `Visage.Shared.Models.CheckInDtos`)






