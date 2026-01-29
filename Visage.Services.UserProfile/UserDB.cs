using Microsoft.EntityFrameworkCore;
using StrictId.EFCore;
using Visage.Shared.Models;

namespace Visage.Services.UserProfile;

/// <summary>
/// Database context for user management and event registrations.
/// </summary>
public class UserDB : DbContext
{
    public UserDB(DbContextOptions<UserDB> options) : base(options)
    { }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.ConfigureStrictId();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasStrictIdValueGenerator();

            // Unique indexes for verified social profiles
            entity.HasIndex(u => u.LinkedInProfile)
                .IsUnique()
                .HasFilter("[IsLinkedInVerified] = 1 AND [LinkedInProfile] IS NOT NULL");

            entity.HasIndex(u => u.LinkedInSubject)
                .IsUnique()
                .HasFilter("[IsLinkedInVerified] = 1 AND [LinkedInSubject] IS NOT NULL");

            entity.HasIndex(u => u.GitHubProfile)
                .IsUnique()
                .HasFilter("[IsGitHubVerified] = 1 AND [GitHubProfile] IS NOT NULL");

            // Email index for lookups
            entity.HasIndex(u => u.Email);
        });

        // EventRegistration entity configuration
        modelBuilder.Entity<EventRegistration>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasStrictIdValueGenerator();

            // Foreign key to User
            entity.HasOne(er => er.User)
                .WithMany(u => u.EventRegistrations)
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign key to Event (no navigation from Event side to avoid cross-db issues)
            entity.HasIndex(er => er.EventId);

            // Composite unique index: one registration per user per event
            entity.HasIndex(er => new { er.UserId, er.EventId })
                .IsUnique();

            // Index for status queries
            entity.HasIndex(er => er.Status);

            // Index for date-based queries
            entity.HasIndex(er => er.RegisteredAt);
        });

        // DraftRegistration entity configuration
        modelBuilder.Entity<DraftRegistration>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasStrictIdValueGenerator();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(d => d.UserId);
            entity.HasIndex(d => d.ExpiresAt);
            entity.HasIndex(d => new { d.UserId, d.Section });
            entity.HasIndex(d => d.IsApplied);
        });

        // UserPreferences entity configuration
        modelBuilder.Entity<UserPreferences>(entity =>
        {
            entity.HasKey(u => u.UserId);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SocialVerificationEvent entity configuration
        modelBuilder.Entity<SocialVerificationEvent>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasStrictIdValueGenerator();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.OccurredAtUtc });
            entity.HasIndex(e => new { e.Provider, e.ProfileUrl });
        });
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<DraftRegistration> DraftRegistrations => Set<DraftRegistration>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
    public DbSet<SocialVerificationEvent> SocialVerificationEvents => Set<SocialVerificationEvent>();
}
