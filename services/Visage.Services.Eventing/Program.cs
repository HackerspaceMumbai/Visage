using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Visage.FrontEnd.Shared.Models;
using Microsoft.AspNetCore.Mvc;



var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.Services.AddDbContext<EventDB>(opt => opt.UseInMemoryDatabase("EventList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();



// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
            {
                options.WithTitle("Visage API")
                       .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            }
    );
}

app.UseHttpsRedirection();

var events = app.MapGroup("/events");

events.MapGet("/", GetAllEvents);

events.MapPost("/", ScheduleEvent)
    .WithName("Schedule Event")
    .WithOpenApi();

app.Run();


static async Task<IResult> GetAllEvents(EventDB db)
{
    return TypedResults.Ok(await db.Events.ToListAsync());
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






