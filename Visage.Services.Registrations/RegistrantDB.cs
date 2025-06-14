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
    }


    public DbSet<Registrant> Registrants => Set<Registrant>();

}
