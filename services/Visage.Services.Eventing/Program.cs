using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Visage.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;



var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.Services.AddDbContext<EventDB>(opt => opt.UseInMemoryDatabase("EventList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapScalarDefaults("Visage Event API");

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






