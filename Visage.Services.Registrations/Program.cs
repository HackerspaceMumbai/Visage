using Microsoft.AspNetCore.Mvc;
using Visage.FrontEnd.Shared.Models;
using Visage.Services.Registration;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Internal;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddServiceDefaults();

builder.Services.AddDbContext<RegistrantDB>(opt => opt.UseInMemoryDatabase("Registrations"));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Visage Registration API")
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    }
    );
}

app.UseHttpsRedirection();

//List<Registrant>? registrations = new List<Registrant>();


app.MapPost("/register", async Task<Results<Created<Registrant>, BadRequest>> ([FromBody] Registrant inputRegistrant, RegistrantDB db) =>
 {
     try
     {
         await db.Registrants.AddAsync(inputRegistrant);
         await db.SaveChangesAsync();
         return TypedResults.Created($"/register/{inputRegistrant.Id}", inputRegistrant);
     }
     catch (Exception)
     {
         return TypedResults.BadRequest();
     }
 });


// return all the registrants   
app.MapGet("/register", async Task<IEnumerable<Registrant>> (RegistrantDB db) =>
{
    return await db.Registrants.ToListAsync();
});

app.Run();


