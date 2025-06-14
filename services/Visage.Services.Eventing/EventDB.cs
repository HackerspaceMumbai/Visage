using Microsoft.EntityFrameworkCore;
using StrictId.EFCore;
using Visage.Shared.Models;

public class EventDB: DbContext
{
    public EventDB(DbContextOptions<EventDB> options) : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.ConfigureStrictId();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>().Property(e => e.Id)
                                        .ValueGeneratedOnAdd()
                                        .HasStrictIdValueGenerator();
    }

    public DbSet<Event> Events => Set<Event>();
}


