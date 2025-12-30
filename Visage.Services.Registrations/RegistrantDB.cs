using Microsoft.EntityFrameworkCore;
using StrictId.EFCore;
using Visage.Shared.Models;




namespace Visage.Services.Registration;



public class RegistrantDB: DbContext
{

    public RegistrantDB(DbContextOptions<RegistrantDB> options) : base(options)
    { }


    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.ConfigureStrictId();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Registrant>().Property(e => e.Id)
                                        .ValueGeneratedOnAdd()
                                        .HasStrictIdValueGenerator();

        modelBuilder.Entity<Registrant>()
            .HasIndex(r => r.LinkedInProfile)
            .IsUnique()
            .HasFilter("[IsLinkedInVerified] = 1 AND [LinkedInProfile] IS NOT NULL");

        modelBuilder.Entity<Registrant>()
            .HasIndex(r => r.LinkedInSubject)
            .IsUnique()
            .HasFilter("[IsLinkedInVerified] = 1 AND [LinkedInSubject] IS NOT NULL");

        modelBuilder.Entity<Registrant>()
            .HasIndex(r => r.GitHubProfile)
            .IsUnique()
            .HasFilter("[IsGitHubVerified] = 1 AND [GitHubProfile] IS NOT NULL");

        // T003: Configure DraftRegistration entity
        modelBuilder.Entity<DraftRegistration>().Property(e => e.Id)
                                        .ValueGeneratedOnAdd()
                                        .HasStrictIdValueGenerator();
        
        // T003: Configure foreign key relationship
        modelBuilder.Entity<DraftRegistration>()
            .HasOne<Registrant>()
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // T003: Configure index on UserId for efficient lookups
        modelBuilder.Entity<DraftRegistration>()
            .HasIndex(d => d.UserId);

        // T003: Configure index on ExpiresAt for efficient cleanup queries
        modelBuilder.Entity<DraftRegistration>()
            .HasIndex(d => d.ExpiresAt);

        // T040: Configure composite index on UserId + Section for draft retrieval
        modelBuilder.Entity<DraftRegistration>()
            .HasIndex(d => new { d.UserId, d.Section });

        // T040: Configure index on IsApplied for draft cleanup queries
        modelBuilder.Entity<DraftRegistration>()
            .HasIndex(d => d.IsApplied);

        // T004: Configure UserPreferences entity with UserId as primary key
        modelBuilder.Entity<UserPreferences>()
            .HasKey(u => u.UserId);

        // T004: Configure foreign key relationship to Registrant
        modelBuilder.Entity<UserPreferences>()
            .HasOne<Registrant>()
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SocialVerificationEvent>().Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasStrictIdValueGenerator();

        modelBuilder.Entity<SocialVerificationEvent>()
            .HasOne<Registrant>()
            .WithMany()
            .HasForeignKey(e => e.RegistrantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SocialVerificationEvent>()
            .HasIndex(e => new { e.RegistrantId, e.OccurredAtUtc });

        modelBuilder.Entity<SocialVerificationEvent>()
            .HasIndex(e => new { e.Provider, e.ProfileUrl });
    }


    public DbSet<Registrant> Registrants => Set<Registrant>();
    public DbSet<DraftRegistration> DraftRegistrations => Set<DraftRegistration>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
    public DbSet<SocialVerificationEvent> SocialVerificationEvents => Set<SocialVerificationEvent>();

}
