using Microsoft.EntityFrameworkCore;
using StrictId.EFCore;
using Visage.Shared.Models;

namespace Visage.Services.Eventing
{
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

            // Configure EventRegistration entity
            modelBuilder.Entity<EventRegistration>(entity =>
            {
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd()
                      .HasStrictIdValueGenerator();

                // Convert enum to string for database storage
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50);

                // Composite index for fast registration lookups (EventId + Auth0Subject)
                entity.HasIndex(e => new { e.EventId, e.Auth0Subject })
                      .IsUnique()
                      .HasDatabaseName("IX_EventRegistrations_EventId_Auth0Subject");

                // Index on Auth0Subject for user's registrations lookup
                entity.HasIndex(e => e.Auth0Subject)
                      .HasDatabaseName("IX_EventRegistrations_Auth0Subject");

                // Index on Status for filtering approved/pending registrations
                entity.HasIndex(e => e.Status)
                      .HasDatabaseName("IX_EventRegistrations_Status");

                // Index on CheckInPin for quick door check-in
                entity.HasIndex(e => e.CheckInPin)
                      .IsUnique()
                      .HasFilter("[CheckInPin] IS NOT NULL")
                      .HasDatabaseName("IX_EventRegistrations_CheckInPin");

                // Foreign key to Event (within same database)
                entity.HasOne<Event>()
                      .WithMany()
                      .HasForeignKey(e => e.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SessionCheckIn entity
            modelBuilder.Entity<SessionCheckIn>(entity =>
            {
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd()
                      .HasStrictIdValueGenerator();

                // Composite unique index for session attendance queries
                entity.HasIndex(e => new { e.EventRegistrationId, e.SessionId })
                      .IsUnique()
                      .HasDatabaseName("IX_SessionCheckIns_Registration_Session");

                // Index on CheckedInAt for time-based queries (compliance reports)
                entity.HasIndex(e => e.CheckedInAt)
                      .HasDatabaseName("IX_SessionCheckIns_CheckedInAt");

                // Foreign key to EventRegistration
                entity.HasOne<EventRegistration>()
                      .WithMany()
                      .HasForeignKey(e => e.EventRegistrationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
        public DbSet<SessionCheckIn> SessionCheckIns => Set<SessionCheckIn>();
    }
}


