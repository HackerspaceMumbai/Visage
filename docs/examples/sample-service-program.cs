using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components (includes Scalar)
builder.AddServiceDefaults();

// Add your services
builder.Services.AddDbContext<SampleDb>(opt => opt.UseInMemoryDatabase("SampleData"));

var app = builder.Build();

// Configure Scalar API documentation
app.MapScalarDefaults("Sample API");

app.UseHttpsRedirection();

// Define API endpoints
var api = app.MapGroup("/api/items");

api.MapGet("/", async (SampleDb db) => await db.Items.ToListAsync())
   .WithName("GetAllItems")
   .WithOpenApi();

api.MapGet("/{id}", async (int id, SampleDb db) =>
    await db.Items.FindAsync(id) is Item item
        ? Results.Ok(item)
        : Results.NotFound())
   .WithName("GetItem")
   .WithOpenApi();

api.MapPost("/", async (Item item, SampleDb db) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/api/items/{item.Id}", item);
})
   .WithName("CreateItem")
   .WithOpenApi();

app.Run();

// Sample data models
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SampleDb : DbContext
{
    public SampleDb(DbContextOptions<SampleDb> options) : base(options) { }
    public DbSet<Item> Items { get; set; }
}