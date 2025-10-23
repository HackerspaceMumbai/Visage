using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Visage.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;



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
    db.Database.Migrate();
    
    // Seed sample data in development if database is empty
    if (app.Environment.IsDevelopment() && !db.Events.Any())
    {
        var sampleEvents = new[]
        {
            new Event
            {
                Title = "Open Source Saturday",
                Description = "Join us for a day of open source contributions, learning, and collaboration!",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                StartTime = new TimeOnly(10, 0),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                EndTime = new TimeOnly(17, 0),
                Location = "Hackerspace Mumbai",
                Type = "Workshop",
                Theme = "Open Source",
                Hashtag = "OSSaturday",
                CoverPicture = "https://res.cloudinary.com/demo/image/upload/sample.jpg"
            },
            new Event
            {
                Title = "Docker & Kubernetes Workshop",
                Description = "Hands-on workshop covering containerization with Docker and orchestration with Kubernetes.",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
                StartTime = new TimeOnly(14, 0),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
                EndTime = new TimeOnly(18, 0),
                Location = "Mumbai Tech Hub",
                Type = "Workshop",
                Theme = "DevOps",
                Hashtag = "K8sWorkshop",
                CoverPicture = "https://res.cloudinary.com/demo/image/upload/mountain.jpg"
            },
            new Event
            {
                Title = "AI & Machine Learning Meetup",
                Description = "Exploring the latest in AI and ML with local experts and practitioners.",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(21)),
                StartTime = new TimeOnly(18, 30),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(21)),
                EndTime = new TimeOnly(21, 0),
                Location = "Colaba Library",
                Type = "Meetup",
                Theme = "AI/ML",
                Hashtag = "AIMeetup",
                CoverPicture = "https://res.cloudinary.com/demo/image/upload/forest.jpg"
            },
            new Event
            {
                Title = "Web Development Bootcamp",
                Description = "Intensive bootcamp covering modern web development with .NET and Blazor.",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                StartTime = new TimeOnly(9, 0),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-28)),
                EndTime = new TimeOnly(18, 0),
                Location = "Hackerspace Mumbai",
                Type = "Bootcamp",
                Theme = "Web Development",
                Hashtag = "WebBootcamp",
                CoverPicture = "https://res.cloudinary.com/demo/image/upload/code.jpg"
            }
        };
        
        db.Events.AddRange(sampleEvents);
        db.SaveChanges();
        
        Console.WriteLine($"Seeded {sampleEvents.Length} sample events for development");
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

app.Run();


static async Task<IResult> GetAllEvents(EventDB db)
{
    return TypedResults.Ok(await db.Events.ToListAsync());
}

static async Task<IResult> GetUpcomingEvents(EventDB db)
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var upcoming = await db.Events
        .Where(e => e.StartDate >= today)
        .ToListAsync();
    return TypedResults.Ok(upcoming);
}

static async Task<IResult> GetPastEvents(EventDB db)
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var past = await db.Events
        .Where(e => e.EndDate < today)
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
        return TypedResults.BadRequest(ex.Message);
    }
}






