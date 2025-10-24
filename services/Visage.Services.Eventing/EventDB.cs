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
        modelBuilder.Entity<Event>(entity =>
        {
            // StrictId configuration
            entity.Property(e => e.Id)
                  .ValueGeneratedOnAdd()
                  .HasStrictIdValueGenerator();

            // String length constraints
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.CoverPicture).HasMaxLength(500);
            entity.Property(e => e.Hashtag).HasMaxLength(100);
            entity.Property(e => e.Theme).HasMaxLength(200);

            // Obsolete properties removed from model; no ignores required

            // Decimal precision for AttendeesPercentage (e.g., 99.99%)
            entity.Property(e => e.AttendeesPercentage)
                  .HasPrecision(5, 2);

            // Indexes for query performance
            entity.HasIndex(e => e.StartDate)
                  .HasDatabaseName("IX_Events_StartDate");
            
            entity.HasIndex(e => e.EndDate)
                  .HasDatabaseName("IX_Events_EndDate");
            
            entity.HasIndex(e => e.Location)
                  .HasDatabaseName("IX_Events_Location");
            
            entity.HasIndex(e => e.Type)
                  .HasDatabaseName("IX_Events_Type");
            
            // Composite index for common queries (upcoming/past events)
            entity.HasIndex(e => new { e.StartDate, e.Type })
                  .HasDatabaseName("IX_Events_StartDate_Type");
        });
    }

    public DbSet<Event> Events => Set<Event>();
}


